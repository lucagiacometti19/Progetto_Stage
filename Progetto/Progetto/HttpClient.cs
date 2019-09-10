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
            /**OLD**/
            //string request = "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car";

            //foreach (GeoPoint p in g)
            //{
            //    request += "&loc=" + p.Latitude.ToString().Replace(',', '.') + "," + p.Longitude.ToString().Replace(',', '.');
            //}
            //request += "&sort=true";
            //Requests.Add(request);

            /**NEW**/
            string request = "http://routing.pointsecurity.it:8085/route/v1/driving/";

            foreach (GeoPoint p in g)
            {
                if(request.EndsWith("/"))
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
            using(client = new HttpClient())
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
            /**OLD**/
            //string jsResult1 = input.Replace("itinero.JSONP.callbacks.route2(", "");
            //string jsResult2 = jsResult1.Substring(0, (jsResult1.Length - 2));
            //JToken contourManifest = JObject.Parse(jsResult2);
            //JToken features = contourManifest.SelectToken("features");

            /**NEW**/
            JToken contourManifest = JObject.Parse(input);
            JToken routes = contourManifest.SelectToken("routes");
            var routesChildren = routes.Children();
            JToken legs = routesChildren.ToArray()[0].SelectToken("legs");
            JToken steps = legs.SelectToken("steps");
            double lat = 0;
            double lon = 0;

            for (int i = 0; i < steps.Count(); i++)
            {
                JToken intersections = steps[i].SelectToken("intersections");
                for (int x = 0; x < intersections.Count(); x++)
                {
                    JToken location = intersections[x].SelectToken("location");
                    string c = location.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);
                    Console.WriteLine(c);
                    c = c.Substring(3, c.Length - 4);
                    Console.WriteLine(c);
                    List<string> coord = c.Split(',').ToList();

                    for (int y  = 0; y < coord.Count - 1; y = y + 2)
                    {
                        //if(lat != Convert.ToDouble(points[x + 1]) && lon != Convert.ToDouble(points[x]))
                        //{
                        lat = Convert.ToDouble(coord[x + 1], CultureInfo.InvariantCulture);
                        lon = Convert.ToDouble(coord[x], CultureInfo.InvariantCulture);

                        Point.Add(new GpxPoint()
                        {
                            Longitude = lon,
                            Latitude = lat
                        });
                        //}
                    }
                }
                

                /**OLD**/
                //JToken geometry = features[i].SelectToken("geometry");
                //JToken coordinates = geometry.SelectToken("coordinates");
                //string c = coordinates.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);
                //c = c.Substring(3, c.Length - 4);
                //string[] parameters = { "[    ", ",    ", "  ],  ", "  ]" };
                //List<string> points = c.Split(parameters, StringSplitOptions.None).ToList();
                //for (int s = 0; s < points.Count; s++)
                //{
                //    if (points[s] == "")
                //    {
                //        points.Remove(points[s]);
                //    }
                //}


                //for (int x = 0; x < points.Count - 1; x = x + 2)
                //{
                //    //if(lat != Convert.ToDouble(points[x + 1]) && lon != Convert.ToDouble(points[x]))
                //    //{
                //    lat = Convert.ToDouble(points[x + 1], CultureInfo.InvariantCulture);
                //    lon = Convert.ToDouble(points[x], CultureInfo.InvariantCulture);

                //    Point.Add(new GpxPoint()
                //    {
                //        Longitude = lon,
                //        Latitude = lat
                //    });
                //    //}
                //}
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