using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Gpx;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Progetto
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            gpxPointsCollection = new ObservableCollection<GpxPoint>();
            gpxTracePoints = new ObservableCollection<GpxPoint>();
            mapItems = new ObservableCollection<MapItem>();
            routes = new ObservableCollection<MapItem>();
            routeViewModels = new ObservableCollection<RouteViewModel>();
            currentViewModel = new RouteViewModel(new ObservableCollection<GpxPoint>());
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
                        //creo la route da mostrate su osm
                        CreateRoute(gpxPointsCollection, false);

                        var newRoute = new RouteViewModel(GpxTracePoints) { Nome = name };
                        RouteViewModels.Add(newRoute);
                        CurrentViewModel = newRoute;
                        //calcolo le proprietà della route
                        newRoute.CalculateStationaryPoints(CurrentViewModel);
                        newRoute.GetSubroutes(CurrentViewModel);
                        //newRoute.CalculateMaxSpeed(CurrentViewModel);
                        //newRoute.CalculateMediumSpeed(CurrentViewModel);
                        //newRoute.CalculateMinSpeed(CurrentViewModel);
                        //newRoute.CalculateRouteLenght(CurrentViewModel);
                        //newRoute.CalculateStart(CurrentViewModel);
                        //newRoute.CalculateEnd(CurrentViewModel);
                        //newRoute.CalculateTotalTime(CurrentViewModel);
                        //await newRoute.GetStationaryPoints(CurrentViewModel);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                GpxPointsCollection = new ObservableCollection<GpxPoint>();
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
            RouteViewModels = new ObservableCollection<RouteViewModel>();
            CurrentViewModel = new RouteViewModel(new ObservableCollection<GpxPoint>());
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
            r.DataContext = this;
            r.ShowDialog();
        }
    }
}