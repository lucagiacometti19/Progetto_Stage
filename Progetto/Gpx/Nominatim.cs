using System;
using System.Collections.Generic;
using System.Text;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using System.Threading.Tasks;

namespace Gpx
{
    public static class Nominatim
    {
        public static async Task<GeocodeResponse> GetAddress(double Lat, double Lon)
        {
            var geoCoder = new ReverseGeocoder();
            var request = new ReverseGeocodeRequest()
            {
                Longitude = Lon,
                Latitude = Lat,
                ZoomLevel = 18
            };
            return await geoCoder.ReverseGeocode(request);
          
        }
    }
}
