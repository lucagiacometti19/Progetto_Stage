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
            gpxPointsCollection = new ObservableCollection<GpxPoint>();
            mapItems = new ObservableCollection<MapItem>();
            routes = new ObservableCollection<MapItem>();
            currentViewModel = new RouteViewModel();
            routeViewModels = new ObservableCollection<RouteViewModel>();
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

        private ObservableCollection<MapItem> mapItems;
        public ObservableCollection<MapItem> MapItems
        {
            get { return mapItems; }
            set { mapItems = value; RaisePropertyChanged(); }

        }

        private ObservableCollection<MapItem> routes;
        public ObservableCollection<MapItem> Routes
        {
            get { return routes; }
            set { routes = value; RaisePropertyChanged(); }
        }

        //Report properties
        private ObservableCollection<RouteViewModel> routeViewModels;

        public ObservableCollection<RouteViewModel> RouteViewModels
        {
            get { return routeViewModels; }
            set { routeViewModels = value; RaisePropertyChanged(); }
        }

        private RouteViewModel currentViewModel;

        public RouteViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { currentViewModel = value; RaisePropertyChanged(); }
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
                    //Aggiungo!
                    string name = Path.GetFileNameWithoutExtension(open.FileName);
                    bool newRouteVM = true;
                    foreach (var routeViewModel in RouteViewModels ?? Enumerable.Empty<RouteViewModel>())
                    {
                        if (name == routeViewModel.Nome)
                        {
                            newRouteVM = false;
                            break;
                        }
                    }
                    if (newRouteVM)
                    {
                        var newRoute = new RouteViewModel() { Nome = name, MainRoute = GpxTracePoints };
                        RouteViewModels.Add(newRoute);
                        CurrentViewModel = newRoute;
                    }
                    //creo la route da mostrate su osm
                    CreateRoute(gpxPointsCollection, false);

                    //calcolo le proprietà della route
                    CalculateStationaryPoints();
                    GetSubroutes();
                    await GetStationaryPoints();
                    CalculateMaxSpeed();
                    CalculateMediumSpeed();
                    CalculateMinSpeed();
                    CalculateRouteLenght();
                    CalculateStart();
                    CalculateEnd();
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

        #region Metodi per il report
        private void CalculateStationaryPoints()
        {
            var mainRoute = CurrentViewModel.MainRoute;
            var iteractions = mainRoute.Count;
            for (int i = 0; i < iteractions - 1; i++)
            {
                if (mainRoute[i].Speed > 150)
                {
                    mainRoute[i].Speed = 150;
                }

                var timeSpan = (mainRoute[i].Start - mainRoute[i + 1].Start);

                double speed = mainRoute[i].Speed;
                DateTime start1 = mainRoute[i].Start;
                double lon = mainRoute[i].Longitude;
                double lat = mainRoute[i].Latitude;

                if (timeSpan > new TimeSpan(0, 5, 0))
                {

                    DateTime start2 = mainRoute[i + 1].Start;

                    if (GpxReader.CalcoloDistanza(mainRoute[i], mainRoute[i + 1]) > 0.0005)
                    {
                        //"Finish" viene modificata per iterare la lista in cerca di subroutes più velocemente in seguito
                        mainRoute[i] = new GpxPoint() { Speed = 0, Start = start1.AddSeconds(-1), Longitude = lon, Latitude = lat, Finish = new DateTime(1) };
                        mainRoute[i + 1] = new GpxPoint() { Speed = 0, Start = start2.AddSeconds(1), Longitude = lon, Latitude = lat, Finish = new DateTime(1) };
                    }
                }
            }
        }

        private async Task GetStationaryPoints()
        {
            try
            {
                CurrentViewModel.PuntiStazionamento = new ObservableCollection<string>();
                int index = 0;
                for (int i = 0; i < CurrentViewModel.MainRoute.Count; i++)
                {
                    if (index - i > 0) { continue; }
                    if (CurrentViewModel.MainRoute[i].Speed == 0)
                    {
                        index = i + 1;
                        while (index < CurrentViewModel.MainRoute.Count && CurrentViewModel.MainRoute[index].Speed == 0)
                        {
                            index++;
                        }
                        var geocoderResult = await Gpx.Nominatim.GetAddress(CurrentViewModel.MainRoute[index - 1].Latitude, CurrentViewModel.MainRoute[index - 1].Longitude);
                        string address = geocoderResult.DisplayName;
                        TimeSpan span = CurrentViewModel.MainRoute[i].Start - CurrentViewModel.MainRoute[index - 1].Start;
                        if (span > new TimeSpan(0, 0, 10))
                            CurrentViewModel.PuntiStazionamento.Add($"Stazionamento alle: {CurrentViewModel.MainRoute[index - 1].Start}| di durata: {span}| a {address}");
                        else
                            CurrentViewModel.PuntiStazionamento.Add($"Stazionamento alle: {CurrentViewModel.MainRoute[index - 1].Start}| a {address}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CalculateMaxSpeed()
        {
            CurrentViewModel.VelocitaMassima = 0;
            foreach (var p in CurrentViewModel.MainRoute)
            {
                if (CurrentViewModel.VelocitaMassima < p.Speed)
                {
                    CurrentViewModel.VelocitaMassima = p.Speed;
                }
            }
        }

        public void CalculateMinSpeed()
        {
            CurrentViewModel.VelocitaMinima = 0;
            foreach (var p in CurrentViewModel.MainRoute)
            {
                if (CurrentViewModel.VelocitaMinima > p.Speed && p.Speed != 0)
                {
                    CurrentViewModel.VelocitaMinima = p.Speed;
                }
            }
        }

        public void CalculateMediumSpeed()
        {
            CurrentViewModel.VelocitaMedia = 0;
            double somma = 0;
            foreach (var p in CurrentViewModel.MainRoute)
            {
                somma += p.Speed;
            }
            CurrentViewModel.VelocitaMedia = somma / CurrentViewModel.MainRoute.Count;
        }

        public void CalculateRouteLenght()
        {
            CurrentViewModel.LunghezzaPercorso = 0;
            for (int i = 0; i < CurrentViewModel.MainRoute.Count - 1; i++)
            {
                double distanza = GpxReader.CalcoloDistanza(CurrentViewModel.MainRoute[i], CurrentViewModel.MainRoute[i + 1]);
                if(distanza != Double.NaN && !double.IsNaN(distanza))
                    CurrentViewModel.LunghezzaPercorso += distanza;
            }
        }

        public void CalculateStart()
        {
            CurrentViewModel.OraInizio = CurrentViewModel.MainRoute[0].Start.ToString();
        }

        public void CalculateEnd()
        {
            CurrentViewModel.OraFine = CurrentViewModel.MainRoute[CurrentViewModel.MainRoute.Count - 1].Start.ToString();
        }

        //Eseguire sempre prima "CalculateStationaryPoints" poi "GetSubroutes"
        public void GetSubroutes()
        {
            ObservableCollection<GpxPoint> subRoute = new ObservableCollection<GpxPoint>();
            bool alreadyAdded = true;
            foreach (var point in CurrentViewModel.MainRoute ?? Enumerable.Empty<GpxPoint>())
            {
                if (point.Finish != new DateTime(1))
                {
                    subRoute.Add(point);
                    alreadyAdded = false;
                }
                else if (!alreadyAdded)
                {
                    if (subRoute.Count > 2)
                    {
                        CurrentViewModel.SegmentsCollection.Add(new RouteViewModel() { MainRoute = subRoute, Nome = "Sotto-route" });
                        subRoute = new ObservableCollection<GpxPoint>();
                        alreadyAdded = true;
                    }
                }
            }
        }
        #endregion

        private DelegateCommand report;
        public DelegateCommand Report
        {
            get { return report ?? (report = new DelegateCommand(ShowReport)); }
        }

        public void ShowReport()
        {
            Report r = new Report();
            r.DataContext = this;
            //OLD-----------------------------------------------------------------------------------------
            //var reportViewModel = new ReportViewModel();
            //reportViewModel.Points = new ObservableCollection<GpxPoint>();
            //if (GpxTracePoints != null && GpxTracePoints.Count() != 0)
            //{
            //    for (int i = 0; i < GpxTracePoints.Count - 1; i++)
            //    {
            //        if (GpxTracePoints[i].Speed > 150)
            //        {
            //            GpxTracePoints[i].Speed = 150;
            //        }

            //        var timeSpan = (GpxTracePoints[i].Start - GpxTracePoints[i + 1].Start);
            //        if (timeSpan > new TimeSpan(0, 5, 0))
            //        {
            //            if (GpxReader.CalcoloDistanza(GpxTracePoints[i], GpxTracePoints[i + 1]) < 100)
            //            {
            //                reportViewModel.Points.Add(new GpxPoint() { Speed = GpxTracePoints[i].Speed, Start = GpxTracePoints[i].Start, Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
            //                reportViewModel.Points.Add(new GpxPoint() { Speed = 0, Start = GpxTracePoints[i].Start.AddSeconds(-1), Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
            //                reportViewModel.Points.Add(new GpxPoint() { Speed = 0, Start = GpxTracePoints[i + 1].Start.AddSeconds(1), Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
            //            }
            //        }
            //        else
            //        {
            //            reportViewModel.Points.Add(new GpxPoint() { Speed = GpxTracePoints[i].Speed, Start = GpxTracePoints[i].Start, Longitude = GpxTracePoints[i].Longitude, Latitude = GpxTracePoints[i].Latitude });
            //        }
            //    }
            //}
            //r.DataContext = reportViewModel;
            //OLD-----------------------------------------------------------------------------------------


            //NEW [SBAGLIATO]-----------------------------------------------------------------------------
            //Limito velocità massima a 150 km/h
            //try
            //{
            //    if (reportViewModel.CurrentViewModel != null)
            //    {
            //        var mainRoute = reportViewModel.CurrentViewModel.MainRoute;
            //        var iteractions = mainRoute.Count;
            //        for (int i = 0; i < iteractions - 1; i++)
            //        {
            //            if (mainRoute[i].Speed > 150)
            //            {
            //                mainRoute[i].Speed = 150;
            //            }

            //            var timeSpan = (mainRoute[i].Start - mainRoute[i + 1].Start);

            //            double speed = mainRoute[i].Speed;
            //            DateTime start1 = mainRoute[i].Start;
            //            double lon = mainRoute[i].Longitude;
            //            double lat = mainRoute[i].Latitude;

            //            if (timeSpan > new TimeSpan(0, 5, 0))
            //            {

            //                DateTime start2 = mainRoute[i + 1].Start;

            //                if (GpxReader.CalcoloDistanza(mainRoute[i], mainRoute[i + 1]) < 100)
            //                {
            //                    //"Finish" viene modificata per iterare la lista in cerca di subroutes più velocemente in seguito
            //                    mainRoute[i] = new GpxPoint() {  Speed = 0, Start = start1.AddSeconds(-1), Longitude = lon, Latitude = lat, Finish = new DateTime(1) };
            //                    mainRoute[i + 1] = new GpxPoint() { Speed = 0, Start = start2.AddSeconds(1), Longitude = lon, Latitude = lat, Finish = new DateTime(1) };
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception e) { Console.WriteLine(e.Message); }
            //reportViewModel.GetSubroutes();
            //NEW [SBAGLIATO]-----------------------------------------------------------------------------

            r.ShowDialog();
        }
    }
}