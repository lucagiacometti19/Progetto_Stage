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



       public async Task CreateMapPushpinAsync(GeoPoint point)
        {
            if(MapItems.Count() >= 3)
            {
                MapItems = new ObservableCollection<MapItem>();
                GpxPointsCollection = new ObservableCollection<GpxPoint>();
                HttpMessage.Reset();
            }
            GpxPointsCollection.Add(new GpxPoint() { Latitude = point.Latitude, Longitude = point.Longitude });
            MapItem mapPushpin = new MapPushpin();

            Console.WriteLine("Richiesta address");
            if(GpxPointsCollection.Count % 2 == 1)
            {
                var address = await CustomRouteData.GetAddressFromPoint(point);
                Console.WriteLine(address);
                mapPushpin = new MapPushpin() { Location = point, Text = "A", Information = address.DisplayName };
            }
            if (GpxPointsCollection.Count % 2 == 0)
            {
                var address = await CustomRouteData.GetAddressFromPoint(point);
                Console.WriteLine(address);
                mapPushpin = new MapPushpin() { Location = point, Text = "B", Information = address.DisplayName };
                CreateRoute(GpxPointsCollection);
            }
            MapItems.Add(mapPushpin);
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
                GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);

                timerTot.Start();
                //int n = 0;
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
                await HttpMessage.HttpRouteRequest(GpxPointsCollection);
                timerTot.Stop();
                Console.WriteLine($"Tempo tot: { timerTot.ElapsedMilliseconds }");
            }
        }

        private DelegateCommand resetAll;
        public DelegateCommand ResetAll
        {
            get { return resetAll ?? (resetAll = new DelegateCommand(Reset)); }
        }

        public void Reset()
        {
            GpxPointsCollection = new ObservableCollection<GpxPoint>();
            MapItems = new ObservableCollection<MapItem>();
            GeoPointsCollection = new ObservableCollection<GeoPoint>();
            HttpMessage.Reset();
        }

        private DelegateCommand report;
        public DelegateCommand Report
        {
            get { return report ?? (report = new DelegateCommand(ShowReport)); }
        }

        public void ShowReport()
        {
            Report r = new Report();
            r.DataContext = new ReportViewModel();
            ((ReportViewModel)r.DataContext).Points = new ObservableCollection<Point>();
            foreach (var point in GpxPointsCollection)
            {
                ((ReportViewModel)r.DataContext).Points.Add(new Point(point.Latitude, point.Longitude));
            }
            r.Show();       
        }
    }
}  