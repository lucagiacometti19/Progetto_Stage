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
            using (HttpResponseMessage response = await client.GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    string jsonResult = await response.Content.ReadAsStringAsync();
                    jsonResult = jsonResult.Replace("itinero.JSONP.callbacks.route2(", "");
                    jsonResult = jsonResult.Substring(0, (jsonResult.Length - 2));
                    ConvertFromJson(jsonResult);
                }
            }
        }

        public static string RequestAssembler(GeoPoint p1, GeoPoint p2, GeoPoint p3, GeoPoint p4, GeoPoint p5)
        {
            return "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car&loc=" + 
                    p1.Latitude.ToString().Replace(',', '.') + 
                    "," + 
                    p1.Longitude.ToString().Replace(',', '.') + 
                    "&loc=" + 
                    p2.Latitude.ToString().Replace(',', '.') +
                    "," + 
                    p2.Longitude.ToString().Replace(',', '.') +
                    "&loc=" +
                    p3.Latitude.ToString().Replace(',', '.') +
                    "," +
                    p3.Longitude.ToString().Replace(',', '.') +
                    "&loc=" +
                    p4.Latitude.ToString().Replace(',', '.') +
                    "," +
                    p4.Longitude.ToString().Replace(',', '.') +
                    "&loc=" +
                    p5.Latitude.ToString().Replace(',', '.') +
                    "," +
                    p5.Longitude.ToString().Replace(',', '.') +
                    "&sort=true";
        }

        public static void ConvertFromJson(string input)
        {
            JToken contourManifest = JObject.Parse(input);
            JToken features = contourManifest.SelectToken("features");

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
                    double lat = Convert.ToDouble(points[x + 1], CultureInfo.InvariantCulture);
                    double lon = Convert.ToDouble(points[x], CultureInfo.InvariantCulture);

                    Point.Add(new GeoPoint()
                    {
                        Longitude = lon,
                        Latitude = lat
                    });
                }
            }
        }
    }
}
