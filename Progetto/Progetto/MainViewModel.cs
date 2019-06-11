using DevExpress.Map;
using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Gpx;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Progetto
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            GpxPointsCollection = new ObservableCollection<GpxPoint>();
            //PolylineCollection = new ObservableCollection<MapPolyline>();
            MapItems = new ObservableCollection<MapItem>();
        }

        private ObservableCollection<GpxPoint> gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return gpxPointsCollection; }
            set { gpxPointsCollection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<GeoPoint> geoPointsCollection;
        public ObservableCollection<GeoPoint> GeoPointsCollection
        {
            get { return geoPointsCollection; }
            set { geoPointsCollection = value; RaisePropertyChanged(); }
        }

        //private ObservableCollection<MapPolyline> polylineCollection;
        //public ObservableCollection<MapPolyline> PolylineCollection
        //{
        //    get { return polylineCollection; }
        //    set { polylineCollection = value; RaisePropertyChanged(); }
        //}


        private ObservableCollection<MapItem> mapItems;
        public ObservableCollection<MapItem> MapItems
        {
            get { return mapItems; }
            set { mapItems = value; RaisePropertyChanged(); }

        }
        Stopwatch timerRequest = new Stopwatch();
        Stopwatch timerTot = new Stopwatch();



       public void CreateMapPushpin(GeoPoint point)
        {
            if(MapItems.Count() == 3)
            {
                MapItems = new ObservableCollection<MapItem>();
                GpxPointsCollection = new ObservableCollection<GpxPoint>();
                HttpMessage.Reset();
            }
            GpxPointsCollection.Add(new GpxPoint() { Latitude = point.Latitude, Longitude = point.Longitude });
            MapItem mapPushpin = new MapPushpin() { Location = point };
            MapItems.Add(mapPushpin);
            if (GpxPointsCollection.Count % 2 == 0)
            {
                CreateRoute(GpxPointsCollection);
            }
        }

        public async void CreateRoute(ObservableCollection<GpxPoint> gpxPoints)
        {
            CustomRouteProvider RouteProvider = new CustomRouteProvider();
            //infoLayer.DataProvider = provider;
            await RouteProvider.CalculateRoute(gpxPoints);
            foreach (MapItem m in CustomRouteData.Items)
            {
                MapItems.Add(m);
            }
        }

        //private void CreatePolylines(ObservableCollection<GeoPoint> points)
        //{
        //    Console.WriteLine("Creando la polilinea..");

        //    MapPolyline pl = new MapPolyline();
        //    pl.Stroke = new SolidColorBrush(Color.FromRgb(0, 17, 255));
        //    foreach (GeoPoint px in points)
        //    {
        //        pl.Points.Add(px);
        //    }
        //    PolylineCollection.Add(pl);
        //    Console.WriteLine("Polilinea creata con successo");

        //}


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
                //timerTot.Start();
                //int n = 0;
                GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);

                //int pointForRequest = 75;
                //for (int c = 0; c < GpxPointsCollection.Count; c += pointForRequest)
                //{
                //    List<GeoPoint> g = new List<GeoPoint>();
                //    for (int i = 0; (i < pointForRequest) && (i + c) < GpxPointsCollection.Count; i += 4)
                //    {
                //        GeoPoint p = null;
                //        if (c < pointForRequest)
                //        {
                //            p = new GeoPoint() { Latitude = GpxPointsCollection[i].Latitude, Longitude = GpxPointsCollection[i].Longitude };
                //        }
                //        else if (c >= pointForRequest)
                //        {
                //            p = new GeoPoint() { Latitude = GpxPointsCollection[i + c].Latitude, Longitude = GpxPointsCollection[i + c].Longitude };
                //        }
                //        g.Add(p);
                //    }
                //    HttpMessage.RequestAssembler(g);
                //    Console.WriteLine($"Request {c} ok");
                //    Console.WriteLine($"Request {n} ok");
                //    n++;
                //}

                //timerRequest.Start();
                //int c2 = 0;
                //foreach (string s in HttpMessage.Requests)
                //{
                //    await HttpMessage.RunAsync(s);
                //    Console.WriteLine($"RunAsync {c2} ok");
                //    c2++;
                //}
                //timerRequest.Stop();
                //Console.WriteLine($"tempo totale ricezione: {timerRequest.ElapsedMilliseconds}");

                //int c3 = 0;
                //foreach (string s in HttpMessage.Results)
                //{
                //    HttpMessage.ConvertFromJson(s);
                //    Console.WriteLine($"Json {c3} ok");
                //    c3++;
                //}

                //GeoPointsCollection = HttpMessage.Point;
                //Console.WriteLine(GeoPointsCollection.Count);
                //CreatePolylines(GeoPointsCollection);

                //CreateRoute(gpxPointsCollection);
                //timerTot.Stop();
                Console.WriteLine($"Tempo tot: { timerTot.ElapsedMilliseconds }");
            }
        }
    }
}