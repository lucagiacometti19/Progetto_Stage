using DevExpress.Xpf.Map;
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
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Media;

namespace Progetto
{
    class RouteProvider
    {
        //campi
        private MapPolyline polyline = new MapPolyline()
        {
            Stroke = new SolidColorBrush(Color.FromRgb(0, 17, 255)),
            IsGeodesic = true,
            EnableHighlighting = false
        };


        //metodi
        public async Task<dynamic> GetAsyncJsonObject(GeoPoint p1, GeoPoint p2)
        {
            string url = "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car&loc=" +
                p1.Latitude.ToString(new CultureInfo("en-US")) + "," + p1.Longitude.ToString(new CultureInfo("en-US")) + "&loc=" +
                p2.Latitude.ToString(new CultureInfo("en-US")) + "," + p2.Longitude.ToString(new CultureInfo("en-US")) + "&sort=true";
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://routing.pointsecurity.it:8085/italy");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (HttpResponseMessage responseMessage = await client.GetAsync(url))
                {
                    responseMessage.EnsureSuccessStatusCode();
                    string json = await responseMessage.Content.ReadAsStringAsync();
                    json = json.Substring(json.IndexOf('{'));
                    json = json.Substring(0, json.Length - 2);
                    return JsonConvert.DeserializeObject<dynamic>(json);
                }
            }
        }

        public async Task<MapPolyline> GetPolylineFromRouteProvider(GeoPoint p1, GeoPoint p2)
        {
            dynamic jObject = await GetAsyncJsonObject(p1, p2);
            double latitude = 0;
            double longitude = 0;
            foreach (var feature in jObject.features)
            {
                if (feature.geometry.type.Value == "Point") continue;
                else foreach (var coord in feature.geometry.coordinates)
                    {
                        if (longitude != Convert.ToDouble(coord[0].Value) && latitude != Convert.ToDouble(coord[1].Value))
                        {
                            longitude = Convert.ToDouble(coord[0].Value);
                            latitude = Convert.ToDouble(coord[1].Value);
                            polyline.Points.Add(new GeoPoint(Convert.ToDouble(coord[1].Value), Convert.ToDouble(coord[0].Value)));
                        }
                    }
            }
            return polyline;
        }
    }
}
