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

        private static ObservableCollection<GpxPoint> point = new ObservableCollection<GpxPoint>();
        public static ObservableCollection<GpxPoint> Point
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
            string request = "http://routing.pointsecurity.it:8085/route/v1/driving/";

            foreach (GeoPoint p in g)
            {
                if (request.EndsWith("/"))
                {
                    request += p.Longitude.ToString().Replace(',', '.') + "," + p.Latitude.ToString().Replace(',', '.');
                }
                else
                {
                    request += ";" + p.Longitude.ToString().Replace(',', '.') + "," + p.Latitude.ToString().Replace(',', '.');
                }
            }
            request += "?overview=false&steps=true";
            Requests.Add(request);
        }


        public static async Task RunAsync(string p)
        {
            using (client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://routing.pointsecurity.it:8085");
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
            JToken contourManifest = JObject.Parse(input);
            JToken routes = contourManifest.SelectToken("routes");
            var routesChildren = routes.Children();
            JToken legs = routesChildren.ToArray()[0].SelectToken("legs");
            var legsChildren = legs.Children();
            Console.WriteLine(legsChildren.ToArray()[0]);
            var steps = legsChildren.ToArray()[0].SelectTokens("steps");
            var stepsChildren = steps.Children();
            double lat = 0;
            double lon = 0;

            for (int i = 0; i < stepsChildren.Count(); i++)
            {
                var intersections = stepsChildren.ToArray()[i].SelectTokens("intersections");

                for (int x = 0; x < intersections.Count(); x++)
                {
                    var intersectionsChildren = intersections.ToArray()[x].Children();
                    JToken location = intersectionsChildren.ToArray()[0].SelectToken("location");
                    var coordinates = location.Children();
                    lat = Convert.ToDouble(coordinates.ToArray()[1]);
                    lon = Convert.ToDouble(coordinates.ToArray()[0]);

                    Point.Add(new GpxPoint()
                    {
                        Longitude = lon,
                        Latitude = lat
                    });
                }
            }
            Console.WriteLine("Fine lettura");
        }



        public static async Task HttpRouteRequest(ObservableCollection<GpxPoint> points)
        {
            int n = 0;
            int pointForRequest = 75;
            for (int c = 0; c < points.Count; c += pointForRequest)
            {
                List<GeoPoint> g = new List<GeoPoint>();
                for (int i = 0; (i < pointForRequest) && (i + c) < points.Count; i++)
                {
                    GeoPoint p = null;
                    if (c < pointForRequest)
                    {
                        p = new GeoPoint() { Latitude = points[i].Latitude, Longitude = points[i].Longitude };
                    }
                    else if (c >= pointForRequest)
                    {
                        p = new GeoPoint() { Latitude = points[i + c].Latitude, Longitude = points[i + c].Longitude };
                    }
                    g.Add(p);
                }
                RequestAssembler(g);
                Console.WriteLine($"Request {c} ok");
                Console.WriteLine($"Request {n} ok");
                n++;
            }


            int c2 = 0;
            foreach (string s in Requests)
            {
                await RunAsync(s);
                Console.WriteLine($"RunAsync {c2} ok");
                c2++;
            }


            int c3 = 0;
            foreach (string s in Results)
            {
                ConvertFromJson(s);
                Console.WriteLine($"Json {c3} ok");
                c3++;
            }
        }


        public static void Reset()
        {
            Point = new ObservableCollection<GpxPoint>();
            Requests = new ObservableCollection<string>();
            Results = new ObservableCollection<string>();
        }
    }
}