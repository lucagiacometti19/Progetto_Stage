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
            Routes = new ObservableCollection<MapItem>();
        }

        private ObservableCollection<GpxPoint> gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return gpxPointsCollection; }
            set { gpxPointsCollection = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<GpxPoint> gpxTracePoints;
        public ObservableCollection<GpxPoint> GpxTracePoints
        {
            get { return gpxTracePoints; }
            set { gpxTracePoints = value; RaisePropertyChanged(); }
        }
        //private ObservableCollection<GeoPoint> geoPointsCollection;
        //public ObservableCollection<GeoPoint> GeoPointsCollection
        //{
        //    get { return geoPointsCollection; }
        //    set { geoPointsCollection = value; RaisePropertyChanged(); }
        //}

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

        private ObservableCollection<MapItem> routes;
        public ObservableCollection<MapItem> Routes
        {
            get { return routes; }
            set { routes = value; }
        }


        public async Task CreateMapPushpinAsync(GeoPoint point)
        {
            if (MapItems.Count() >= 3)
            {
                MapItems = new ObservableCollection<MapItem>();
                GpxPointsCollection = new ObservableCollection<GpxPoint>();
                HttpMessage.Reset();
            }
            GpxPointsCollection.Add(new GpxPoint() { Latitude = point.Latitude, Longitude = point.Longitude });
            MapItem mapPushpin = new MapPushpin();

            Console.WriteLine("Richiesta address");
            if (GpxPointsCollection.Count % 2 == 1)
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
                CreateRoute(GpxPointsCollection, true);
            }
            MapItems.Add(mapPushpin);
        }

        /// <summary>
        /// Si appoggia a CustomRouteProvider per visualizzare la route su osm
        /// </summary>
        /// <param name="gpxPoints">lista di gpx point della route</param>
        /// <param name="value">true per richiedere la polilinea al server, false per mostrare la polilinea che unisce i punti senza richiesta al server</param>
        public async void CreateRoute(ObservableCollection<GpxPoint> gpxPoints, bool value)
        {
            CustomRouteProvider RouteProvider = new CustomRouteProvider();
            await RouteProvider.CalculateRoute(gpxPoints, value);
            if (value)
            {
                foreach (MapItem m in CustomRouteData.Items)
                {
                    MapItems.Add(m);
                }
            }
            else
            {
                foreach (MapItem m in CustomRouteData.Items)
                {
                    Routes.Add(m);
                }
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
            try
            {

                OpenFileDialog open = new OpenFileDialog
                {
                    Filter = "Xml files (*.xml)|*.xml",
                    Title = "Importa file"
                };
                if ((bool)open.ShowDialog())
                {
                    GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);
                    GpxTracePoints = GpxPointsCollection;

                    timerTot.Start();
                    CreateRoute(gpxPointsCollection, true);
                    timerTot.Stop();
                    Console.WriteLine($"Tempo tot: { timerTot.ElapsedMilliseconds }");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
            GpxTracePoints = new ObservableCollection<GpxPoint>();
            Routes = new ObservableCollection<MapItem>();
            MapItems = new ObservableCollection<MapItem>();
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
            var reportViewModel = new ReportViewModel();
            reportViewModel.Points = new ObservableCollection<GpxPoint>();
            if (GpxTracePoints != null && GpxTracePoints.Count() != 0)
            {
                for (int i = 0; i < GpxTracePoints.Count - 1; i++)
                {
                    if (GpxTracePoints[i].Speed > 150)
                    {
                        GpxTracePoints[i].Speed = 150;
                    }

                    var timeSpan = (GpxTracePoints[i].Start - GpxTracePoints[i + 1].Start);
                    if (timeSpan > new TimeSpan(0, 5, 0))
                    {
                        if (GpxReader.CalcoloDistanza(GpxTracePoints[i], GpxTracePoints[i + 1]) < 100)
                        {
                            reportViewModel.Points.Add(new GpxPoint() { Speed = GpxTracePoints[i].Speed, Start = GpxTracePoints[i].Start, Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
                            reportViewModel.Points.Add(new GpxPoint() { Speed = 0, Start = GpxTracePoints[i].Start.AddSeconds(-1), Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
                            reportViewModel.Points.Add(new GpxPoint() { Speed = 0, Start = GpxTracePoints[i + 1].Start.AddSeconds(1), Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
                        }
                    }
                    else
                    {
                        reportViewModel.Points.Add(new GpxPoint() { Speed = GpxTracePoints[i].Speed, Start = GpxTracePoints[i].Start, Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
                    }
                }
            }
            r.DataContext = reportViewModel;
            r.Owner = Application.Current.MainWindow;
            r.ShowDialog();
        }

        //private DelegateCommand nominatimm;
        //public DelegateCommand Nominatimm
        //{
        //    get { return nominatimm ?? (nominatimm = new DelegateCommand(Nominatimmm)); }
        //}

        //public async void Nominatimmm()
        //{
        //    var result = await Gpx.Nominatim.GetAddress(42, 12);
        //    string address = result.DisplayName;
        //    MessageBox.Show(address);
        //}
    }
}