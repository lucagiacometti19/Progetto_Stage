using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using DevExpress.Xpf.Map;
using Gpx;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Globalization;


namespace Progetto
{
    public class HttpMessage
    {
        public static HttpClient client;
        public static object EntityUtils { get; private set; }

        private static ObservableCollection<GeoPoint> point = new ObservableCollection<GeoPoint>();
        public static ObservableCollection<GeoPoint> Point
        {
            get { return point; }
            set { point = value; }
        }

        public static async Task RunAsync(string p)
        {
            // Update port # in the following line.
            using (client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://routing.pointsecurity.it:8085/italy");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    await GetProductAsync(p);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ReadLine();
        }

        public static async Task GetProductAsync(string path)
        {
            Console.WriteLine("Tentativo di ricezione della richiesta");
            using (HttpResponseMessage response = await client.GetAsync(path))
            {
                Console.WriteLine("Ricezione richiesta");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Richiesta ricevuta correttamente");
                    string jsonResult = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(jsonResult);
                    jsonResult = jsonResult.Replace("itinero.JSONP.callbacks.route2(", "");
                    jsonResult = jsonResult.Substring(0, (jsonResult.Length - 2));
                    ConvertFromJson(jsonResult);
                }
            }
        }

        public static string RequestAssembler(List<GeoPoint> g)
        {
            Console.WriteLine("Invio richiesta");
            string request = "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car";

            foreach ( GeoPoint p in g)
            {
                request += "&loc=" + p.Latitude.ToString().Replace(',', '.') + "," + p.Longitude.ToString().Replace(',', '.');
            }
            request += "&sort=true";

            //"http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car" + 
            //       "&loc=" +
            //       p1.Latitude.ToString().Replace(',', '.') + "," + p1.Longitude.ToString().Replace(',', '.') + 
            //       "&loc=" + 
            //       p2.Latitude.ToString().Replace(',', '.') + "," + p2.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p3.Latitude.ToString().Replace(',', '.') + "," + p3.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p4.Latitude.ToString().Replace(',', '.') + "," + p4.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p5.Latitude.ToString().Replace(',', '.') +  "," + p5.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p6.Latitude.ToString().Replace(',', '.') + "," + p6.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p7.Latitude.ToString().Replace(',', '.') + "," + p7.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p8.Latitude.ToString().Replace(',', '.') + "," + p8.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p9.Latitude.ToString().Replace(',', '.') + "," + p9.Longitude.ToString().Replace(',', '.') +
            //       "&loc=" +
            //       p10.Latitude.ToString().Replace(',', '.') + "," + p10.Longitude.ToString().Replace(',', '.') +
            //       "&sort=true";

            return request;
        }

        public static void ConvertFromJson(string input)
        {
            Console.WriteLine("Lettura Json");

            JToken contourManifest = JObject.Parse(input);
            JToken features = contourManifest.SelectToken("features");
            double lat = 0;
            double lon = 0;

            for (int i = 0; i < features.Count(); i++)
            {
                JToken geometry = features[i].SelectToken("geometry");
                JToken coordinates = geometry.SelectToken("coordinates");
                string c = coordinates.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);
                c = c.Substring(3, c.Length - 4);
                string[] parameters = { "[    ", ",    ", "  ],  ", "  ]" };
                List<string> points = c.Split(parameters, StringSplitOptions.None).ToList();
                for (int s = 0; s < points.Count; s++)
                {
                    if (points[s] == "")
                    {
                        points.Remove(points[s]);
                    }
                }


                for (int x = 0; x < points.Count - 1; x = x + 2)
                {
                    //if(lat != Convert.ToDouble(points[x + 1]) && lon != Convert.ToDouble(points[x]))
                    //{
                        lat = Convert.ToDouble(points[x + 1], CultureInfo.InvariantCulture);
                        lon = Convert.ToDouble(points[x], CultureInfo.InvariantCulture);

                        Point.Add(new GeoPoint()
                        {
                            Longitude = lon,
                            Latitude = lat
                        });
                    //}
                }
            }
            Console.WriteLine("Fine lettura");

        }
    }
}
