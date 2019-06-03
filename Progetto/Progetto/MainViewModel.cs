using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Gpx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace Progetto
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            GpxPointsCollection = new ObservableCollection<GpxPoint>();
            PolylineCollection = new ObservableCollection<MapPolyline>();
        }

        private ObservableCollection<GpxPoint> gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return gpxPointsCollection; }
            set { gpxPointsCollection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<MapPolyline> polylineCollection;
        public ObservableCollection<MapPolyline> PolylineCollection
        {
            get { return polylineCollection; }
            set { polylineCollection = value; RaisePropertyChanged(); }
        }

        private BingRouteDataProvider bingRouteData;
        public BingRouteDataProvider BingRouteData
        {
            get { return bingRouteData; }
            set { bingRouteData = value; RaisePropertyChanged(); }
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

        private void CreateRoute(ObservableCollection<GpxPoint> points)
        {
            List<RouteWaypoint> waypoints = new List<RouteWaypoint>();
            for (int i = 0; i < 24; i++)
            {
                waypoints.Add(new RouteWaypoint($"Route point {i}", new GeoPoint(points[i].Latitude, points[i].Longitude)));
            }
            BingRouteData = new BingRouteDataProvider
            {
                //MaxVisibleResultCount = 25,
                //RouteStroke = new SolidColorBrush(Color.FromRgb(7, 59, 142)),
                BingKey = "mWuksRMdb006DVTVeRoy~VBby6uhRsgm_fGc4n7RuVA~AmHfaZlNw0sc5TxdXkuC6wuf13uenZlF184AN_kdfZZuyj_VCS4BKOqfXF0KUsqi"
            };
            BingRouteData.LayerItemsGenerating += routeLayerItemsGenerating;
            BingRouteData.CalculateRoute(waypoints);
        }

        private void routeLayerItemsGenerating(object sender, LayerItemsGeneratingEventArgs e)
        {
            if (e.Cancelled || (e.Error != null)) return;
            int counter = 0;
            foreach (MapItem item in e.Items)
            {
                if(item is MapPushpin)
                {
                    if (counter == 0 || counter == e.Items.Length - 2)
                        item.Visible = true;
                    else
                        item.Visible = false;
                    counter++;
                }
            }
        }

        private AsyncCommand importCommand;
        public AsyncCommand ImportCommand
        {
            get { return importCommand ?? (importCommand = new AsyncCommand(Import)); }
        }
        private async Task Import()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Importa file"
            };
            if ((bool)open.ShowDialog())
            {
                GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);
                //CreatePolylines(GpxPointsCollection);
                CreateRoute(GpxPointsCollection);
            }
        }
    }
}
