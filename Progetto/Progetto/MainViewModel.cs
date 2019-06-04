using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Microsoft.Win32;
using System;
using Gpx;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Progetto
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            GpxPointsCollection = new ObservableCollection<GpxPoint>();

        }

        private ObservableCollection<GpxPoint> gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return gpxPointsCollection; }
            set { gpxPointsCollection = value; RaisePropertyChanged(); }
        }

        //private BingRouteDataProvider routeDataProvider;
        //public BingRouteDataProvider RouteDataProvider
        //{
        //    get { return routeDataProvider; }
        //    set { routeDataProvider = value; RaisePropertyChanged(); }
        //}


        //public void CreateRoute(ObservableCollection<GpxPoint> points)
        //{
        //    List<RouteWaypoint> waypoints = new List<RouteWaypoint>();

        //    for (int i = 0; i < 24; i++)
        //    {
        //        waypoints.Add(new RouteWaypoint($"Route point {i}", new GeoPoint(points[i].Latitude, points[i].Longitude)));
        //    }

        //    RouteDataProvider = new BingRouteDataProvider
        //    {
        //        BingKey = "mWuksRMdb006DVTVeRoy~VBby6uhRsgm_fGc4n7RuVA~AmHfaZlNw0sc5TxdXkuC6wuf13uenZlF184AN_kdfZZuyj_VCS4BKOqfXF0KUsqi"
        //    };

        //    RouteDataProvider.CalculateRoute(waypoints);
        //    routeDataProvider.LayerItemsGenerating += routeLayerItemsGenerating;
        //} 

        //private void routeLayerItemsGenerating(object sender, LayerItemsGeneratingEventArgs e)
        //{
        //    if (e.Cancelled || (e.Error != null)) return;
        //    int counter = 0;
        //    foreach (MapItem item in e.Items)
        //        if (item is MapPushpin)
        //        {
        //            if (counter == 0 || counter == e.Items.Length - 2)
        //                item.Visible = true;
        //            else
        //                item.Visible = false;
        //            counter++;
        //        }
        //}


        //private RouteProvider routeDataProvider;
        //public RouteProvider RouteDataProvider
        //{
        //    get { return routeDataProvider; }
        //    set { routeDataProvider = value; RaisePropertyChanged(); }
        //}


        //public void CreateRoute()
        //{
        //    RouteProvider provider = new RouteProvider();
        //    RouteDataProvider = provider;
        //    provider.CalculateRoute(new GeoPoint(70, 60), new GeoPoint(-60, -70));
        //}


        private ObservableCollection<MapPolyline> polylineCollection;
        public ObservableCollection<MapPolyline> PolylineCollection
        {
            get { return polylineCollection; }
            set { polylineCollection = value; RaisePropertyChanged(); }
        }

        private void CreatePolylines(ObservableCollection<GpxPoint> points)
        {
            MapPolyline pl = new MapPolyline();
            pl.Stroke = new SolidColorBrush(Color.FromRgb(0, 17, 255));
            foreach (GpxPoint px in points)
            {
                pl.Points.Add(new GeoPoint(px.Latitude, px.Longitude));
            }
            PolylineCollection.Add(pl);
        }




        private DelegateCommand importCommand;
        public DelegateCommand ImportCommand
        {
            get { return importCommand ?? (importCommand = new DelegateCommand(Import)); }
        }

        private async void Import()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Importa file"
            };
            if ((bool)open.ShowDialog())
            {
                GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);
                //CreateRoute(GpxPointsCollection);

                GeoPoint p1 = new GeoPoint() { Latitude = 43.264206, Longitude = 11.550751, };
                GeoPoint p2 = new GeoPoint() { Latitude = 43.110007,  Longitude = 11.535645, };
                string request = HttpMessage.RequestAssembler(p1, p2);
                await HttpMessage.RunAsync(request);
            }
        }
    }
}