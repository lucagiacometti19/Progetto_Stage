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

        private static List<string> hints;

        public static List<string> Hints
        {
            get { return hints; }
            set { hints = value; }
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
            request += "?overview=false&steps=true&geometries=geojson";
            Requests.Add(request);
        }


        public static async Task RunAsync(string p, int numberOfRequest)
        {
            using (client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://routing.pointsecurity.it:8085");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    if (numberOfRequest == 0)
                    {
                        await GetProductAsync(p);
                    }
                    else
                    {
                        string request = Requests[numberOfRequest];
                        string[] splittedRequest = request.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                        string newRequest = splittedRequest[0] + "?" + "hints=" + Hints[0] + ";" + Hints[1] + "&" + splittedRequest[1];
                        await GetProductAsync(newRequest);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
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
            //ottiene le coordinate dei punti
            JToken routes = contourManifest.SelectToken("routes");
            var legs = routes.ToArray()[0].SelectTokens("legs");
            for (int z = 0; z < legs.Children().Count(); z++)
            {
                var steps = legs.Children().ToArray()[z].SelectTokens("steps");
                var stepsChildren = steps.Children();
                double lat = 0;
                double lon = 0;


                for (int i = 0; i < stepsChildren.Count(); i++)
                {
                    var geometry = stepsChildren.ToArray()[i].SelectToken("geometry");
                    var coordinates = geometry.SelectToken("coordinates");

                    Console.WriteLine(coordinates.ToArray()[0].Count());
                    for (int x = 0; x < coordinates.ToArray().Count(); x++)
                    {
                        var intersectionsChildren = coordinates.ToArray()[x];
                        lat = Convert.ToDouble(intersectionsChildren.ToArray()[1]);
                        lon = Convert.ToDouble(intersectionsChildren.ToArray()[0]);

                        Point.Add(new GpxPoint()
                        {
                            Longitude = lon,
                            Latitude = lat
                        });
                    }
                }
            }

            //ottiene gli hints
            Hints = new List<string>();
            JToken waypoints = contourManifest.SelectToken("waypoints");
            var waypointsChildren = waypoints.Children();
            for (int i = 0; i < waypointsChildren.Count(); i++)
            {
                var hint = waypointsChildren.ToArray()[i].SelectToken("hint");
                Hints.Add(hint.ToString());
            }
        }

        public static async Task HttpRouteRequest(ObservableCollection<GpxPoint> points)
        {
            int n = 0;
            int pointForRequest = 2;
            try
            {
                for (int c = 0; c < points.Count; c += pointForRequest)
                {
                    List<GeoPoint> g = new List<GeoPoint>();
                    for (int i = 0; (i < pointForRequest) && (i + c) < points.Count; i++)
                    {
                        GeoPoint p = null;
                        p = new GeoPoint() { Latitude = points[i + c].Latitude, Longitude = points[i + c].Longitude };
                        g.Add(p);
                    }
                    RequestAssembler(g);
                    Console.WriteLine($"Request {c} ok");
                    Console.WriteLine($"Request {n} ok");
                    n++;
                }
            }
            catch { }


            int c2 = 0;
            while (true)
            {
                try
                {
                    await RunAsync(Requests[c2], c2);
                    //qui devo decifrare gli il json
                    //PROVA
                    ConvertFromJson(Results[c2]);
                    //PROVA
                    Console.WriteLine($"RunAsync {c2} ok");
                    c2++;
                }
                catch { break; }
            }
        }

        public static void ThinPointCollection()
        {
            Console.WriteLine(Point.Count);
            ObservableCollection<GpxPoint> thinnedList = new ObservableCollection<GpxPoint>();
            int index = 1;
            int thinnedIndex = 0;
            thinnedList.Add(Point[0]);
            while (true)
            {
                try
                {
                    if (Tolleranza(thinnedList[thinnedIndex], Point[index], 0.1))
                    {
                        thinnedIndex++;
                        thinnedList.Add(Point[index]);
                    }
                    else
                        index++;
                }
                catch { break; }
            }
            Console.WriteLine($"{thinnedList.Count} thinned");
            Point = thinnedList;
        }

        //tolleranza in km
        private static bool Tolleranza(GpxPoint p1, GpxPoint p2, double tolleranza)
        {
            if (GpxReader.CalcoloDistanza(p1, p2) > tolleranza)
            {
                return true;
            }
            else
            {
                return false;
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