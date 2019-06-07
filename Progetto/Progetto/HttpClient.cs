using DevExpress.Xpf.Map;
using Gpx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


namespace Progetto
{
    public class HttpMessage
    {
        public static HttpClient client;

        private static ObservableCollection<GeoPoint> point = new ObservableCollection<GeoPoint>();
        public static ObservableCollection<GeoPoint> Point
        {
            get { return point; }
            set { point = value; }
        }

        private static ObservableCollection<string> requests = new ObservableCollection<string>();
        public static ObservableCollection<string> Requests
        {
            get { return requests; }
            set { requests = value; }
        }

        private static ObservableCollection<string> results = new ObservableCollection<string>();
        public static ObservableCollection<string> Results
        {
            get { return results; }
            set { results = value; }
        }

        public static void RequestAssembler(List<GeoPoint> g)
        {
            string request = "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car";

            foreach (GeoPoint p in g)
            {
                request += "&loc=" + p.Latitude.ToString().Replace(',', '.') + "," + p.Longitude.ToString().Replace(',', '.');
            }
            request += "&sort=true";
            Requests.Add(request);
        }


        public static async Task RunAsync(string p)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = null,
                PreAuthenticate = true,
                UseDefaultCredentials = false,
            };
            using (client = new HttpClient(httpClientHandler))
            {
                client.BaseAddress = new Uri("http://routing.pointsecurity.it:8085/italy");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpRequestMessage rm = new HttpRequestMessage(new HttpMethod("GET"), p);
                    var response = await client.SendAsync(rm);
                    await GetProductAsync(Convert.ToString(p));

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
                response.EnsureSuccessStatusCode();
                using (HttpContent content = response.Content)
                {
                    Console.WriteLine("Richiesta ricevuta correttamente");
                    string jsonResult = await content.ReadAsStringAsync();
                    Console.WriteLine(jsonResult);
                    Results.Add(jsonResult);
                }
            }
        }

        public static void ConvertFromJson(string input)
        {
            Console.WriteLine("Lettura Json");
            string jsResult1 = input.Replace("itinero.JSONP.callbacks.route2(", "");
            string jsResult2 = jsResult1.Substring(0, (jsResult1.Length - 2));
            JToken contourManifest = JObject.Parse(jsResult2);
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
