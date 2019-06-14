using DevExpress.Map.Native;
using DevExpress.Xpf.Map;
using Gpx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;


namespace Progetto
{
    public class CustomRouteProvider : InformationDataProviderBase
    {
        protected new CustomRouteData Data { get { return (CustomRouteData)base.Data; } }
        public IEnumerable<GeoPoint> Route { get { return Data.Route; } }
        public override bool IsBusy
        {
            get
            {
                return false;
            }
        }

        protected override IInformationData CreateData()
        {
            return new CustomRouteData();
        }

        public async Task CalculateRoute(ObservableCollection<GpxPoint> list, bool value)
        {
            await Data.CalculateRoute(list, value);
        }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        protected override MapDependencyObject CreateObject()
        {
            return new CustomRouteProvider();
        }
    }

    public class CustomRouteData : IInformationData
    {

        public static MapItem[] Items { get; set; }

        readonly List<GeoPoint> route = new List<GeoPoint>();
        public List<GeoPoint> Route { get { return route; } }
        public event EventHandler<RequestCompletedEventArgs> OnDataResponse;
        RequestCompletedEventArgs CreateEventArgs(bool value)
        {
            Items = new MapItem[1];
            //Items[1] = new MapPushpin() { Location = route[0], Text = "A", Information = route[0].ToString() };
            //Items[2] = new MapPushpin() { Location = route[route.Count - 1], Text = "B", Information = route[route.Count - 1].ToString() };

            MapPolyline polyline = new MapPolyline()
            {
                IsGeodesic = true,
                Stroke = value ? new SolidColorBrush() { Color = Colors.Red } : new SolidColorBrush() { Color = Colors.Blue },
                StrokeStyle = new StrokeStyle() { Thickness = 2 }
            };
            for (int i = 0; i < route.Count; i++)
                polyline.Points.Add(route[i]);
            Items[0] = polyline;


            return new RequestCompletedEventArgs(Items, null, false, null);
        }

        protected void RaiseChanged(bool value)
        {
            OnDataResponse?.Invoke(this, CreateEventArgs(value));
        }

        public async Task CalculateRoute(ObservableCollection<GpxPoint> list, bool value)
        {
            await CalculateRouteCore(list, value);
            RaiseChanged(value);
        }

        async Task CalculateRouteCore(ObservableCollection<GpxPoint> list, bool value)
        {
            this.route.Clear();
            //foreach (GpxPoint p in list)
            //{
            //    route.Add(new GeoPoint() { Latitude = p.Latitude, Longitude = p.Longitude });
            //}

            if (value)
            {
                await HttpMessage.HttpRouteRequest(list);
                foreach (GpxPoint p in HttpMessage.Point)
                {
                    route.Add(new GeoPoint() { Latitude = p.Latitude, Longitude = p.Longitude });
                }
            }
            else
            {
                foreach (GpxPoint p in list)
                {
                    route.Add(new GeoPoint() { Latitude = p.Latitude, Longitude = p.Longitude });
                }
            }

        }

        public static async Task<GeocodeResponse> GetAddressFromPoint(GeoPoint point)
        {
            ReverseGeocoder rev = new ReverseGeocoder();
            ReverseGeocodeRequest request = new ReverseGeocodeRequest()
            {
                Longitude = point.Longitude,
                Latitude = point.Latitude,
                ZoomLevel = 18
            };
            return await rev.ReverseGeocode(request);
        }

    }
}
