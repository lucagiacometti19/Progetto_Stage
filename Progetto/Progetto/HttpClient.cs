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

namespace Progetto
{
    public class Product
    {
        public GeoPoint p1 { get; set; }
        public GeoPoint p2 { get; set; }
    }

    public class HttpMessage
    {
        public static HttpClient client = new HttpClient();

        public static object EntityUtils { get; private set; }

        //public static void ShowProduct(Product product)
        //{
        //    Console.WriteLine($"Start: {product.p1}\tArrive: " +
        //        $"{product.p2}");
        //}

        //public static async Task<Uri> CreateProductAsync(Product product)
        //{
        //    HttpResponseMessage response = await client.PostAsJsonAsync(
        //        "api/products", product);
        //    response.EnsureSuccessStatusCode();

        //    // return URI of the created resource.
        //    return response.Headers.Location;
        //}

        public static async Task<Product> GetProductAsync(string path)
        {
            //string path = "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car&loc=43.264206,11.550751&loc=43.110007,11.535645&sort=true";
            Product product = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                //product = await UpdateProductAsync(product, path);
                string jsonResult = await response.Content.ReadAsStringAsync();
                jsonResult = jsonResult.Replace("itinero.JSONP.callbacks.route2(", "");
                jsonResult = jsonResult.Substring(0, (jsonResult.Length - 2));
                ConvertFromJson(jsonResult);
            }
            return product;
        }

        //public static async Task<Product> UpdateProductAsync(Product product, string path)
        //{
        //    HttpResponseMessage response = await client.PutAsJsonAsync(path, product);
        //    response.EnsureSuccessStatusCode();

        //    // Deserialize the updated product from the response body.
        //    product = await response.Content.ReadAsAsync<Product>();
        //    return product;
        //}

        //public static async Task<HttpStatusCode> DeleteProductAsync(string id)
        //{
        //    HttpResponseMessage response = await client.DeleteAsync(
        //        $"api/products/{id}");
        //    return response.StatusCode;
        //}

        //public static void Main()
        //{
        //    RunAsync().GetAwaiter().GetResult();
        //}

        public static async Task RunAsync(string p)
        {
            // Update port # in the following line.
            client.BaseAddress = new Uri("http://routing.pointsecurity.it:8085/italy");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // Create a new product
                Product product = new Product
                {
                    p1 = new GeoPoint(45, 12),
                    p2 = new GeoPoint(45, 11),
                };

                //var url = await CreateProductAsync(product);
                //Console.WriteLine($"Created at {url}");

                // Get the product
                product = await GetProductAsync(p);
                //ShowProduct(product);

                // Update the product
                //Console.WriteLine("Updating price...");
                //product.Price = 80;
                //await UpdateProductAsync(product);

                //// Get the updated product
                //product = await GetProductAsync(url.PathAndQuery);
                //ShowProduct(product);

                // Delete the product
                //var statusCode = await DeleteProductAsync(product.Id);
                //Console.WriteLine($"Deleted (HTTP Status = {(int)statusCode})");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }


        public static string RequestAssembler(GeoPoint p1, GeoPoint p2)
        {
            return "http://routing.pointsecurity.it:8085/italy/routing?callback=itinero.JSONP.callbacks.route2&profile=car&loc=" + 
                    p1.Latitude.ToString().Replace(',', '.') + "," + 
                    p1.Longitude.ToString().Replace(',', '.') + 
                    "&loc=" + p2.Latitude.ToString().Replace(',', '.') + "," + 
                    p2.Longitude.ToString().Replace(',', '.') + "&sort=true";
        }


        public static void ConvertFromJson(string input)
        {
            JToken contourManifest = JObject.Parse(input);

            JToken features = contourManifest.SelectToken("features");

            for (int i = 0; i < features.Count(); i++)
            {
                JToken geometry = features[i].SelectToken("geometry");
                JToken coordinates = geometry.SelectToken("coordinates");
                string c = coordinates.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);//.Replace("[", string.Empty).Replace("]", string.Empty);
                c = c.Substring(3, c.Length - 4);
                Console.WriteLine(c);
            }
            //arrGeometrys[] = {
            //      arrPoint1[] = { arrCoordintes[][] },
            //      arrPoint2[] = { arrCoordinates[][] },
            //      ... }

        }
    }
}
