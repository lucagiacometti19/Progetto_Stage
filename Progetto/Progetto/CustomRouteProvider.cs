﻿using DevExpress.Map.Native;
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

        public async Task CalculateRoute(ObservableCollection<GpxPoint> list)
        {
            await Data.CalculateRoute(list);
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
        RequestCompletedEventArgs CreateEventArgs()
        {
            Items = new MapItem[1];
            //Items[1] = new MapPushpin() { Location = route[0], Text = "A", Information = route[0].ToString() };
            //Items[2] = new MapPushpin() { Location = route[route.Count - 1], Text = "B", Information = route[route.Count - 1].ToString() };

            MapPolyline polyline = new MapPolyline()
            {
                IsGeodesic = true,
                Stroke = new SolidColorBrush() { Color = Colors.Red },
                StrokeStyle = new StrokeStyle() { Thickness = 4 }
            };
            for (int i = 0; i < route.Count; i++)
                polyline.Points.Add(route[i]);
            Items[0] = polyline;


            return new RequestCompletedEventArgs(Items, null, false, null);
        }

        protected void RaiseChanged()
        {
            if (OnDataResponse != null)
                OnDataResponse(this, CreateEventArgs());
        }

        public async Task CalculateRoute(ObservableCollection<GpxPoint> list)
        {
            await CalculateRouteCore(list);
            RaiseChanged();
        }

        async Task CalculateRouteCore(ObservableCollection<GpxPoint> list)
        {
            this.route.Clear();
            //foreach (GpxPoint p in list)
            //{
            //    route.Add(new GeoPoint() { Latitude = p.Latitude, Longitude = p.Longitude });
            //}
            await HttpMessage.HttpRouteRequest(list);
            foreach (GeoPoint p in HttpMessage.Point)
            {
                route.Add(p);
            }
        }

        public static async Task<GeocodeResponse> GetAddressFromPoint(GeoPoint point)
        {
            return await new ReverseGeocoder().ReverseGeocode(new ReverseGeocodeRequest
            {
                Longitude = point.Longitude,
                Latitude = point.Latitude,
                ZoomLevel = 18
            });
        }

    }
}
