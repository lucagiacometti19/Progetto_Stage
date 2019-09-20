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
            _gpxPointsCollection = new ObservableCollection<GpxPoint>();
            _gpxTracePoints = new ObservableCollection<GpxPoint>();
            _mapItems = new ObservableCollection<MapItem>();
            _routes = new ObservableCollection<MapItem>();
            _routeViewModels = new ObservableCollection<ReportViewModel>();
            _currentViewModel = new ReportViewModel(new ObservableCollection<GpxPoint>());
        }

        private ObservableCollection<GpxPoint> _gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return _gpxPointsCollection; }
            set { _gpxPointsCollection = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<GpxPoint> _gpxTracePoints;
        public ObservableCollection<GpxPoint> GpxTracePoints
        {
            get { return _gpxTracePoints; }
            set { _gpxTracePoints = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<MapItem> _mapItems;
        public ObservableCollection<MapItem> MapItems
        {
            get { return _mapItems; }
            set { _mapItems = value; RaisePropertyChanged(); }

        }

        private ObservableCollection<MapItem> _routes;
        public ObservableCollection<MapItem> Routes
        {
            get { return _routes; }
            set { _routes = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ReportViewModel> _routeViewModels;

        public ObservableCollection<ReportViewModel> RouteViewModels
        {
            get { return _routeViewModels; }
            set { _routeViewModels = value; RaisePropertyChanged(); }
        }

        private ReportViewModel _currentViewModel;

        public ReportViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set { _currentViewModel = value; RaisePropertyChanged(); }
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

        private DelegateCommand _importCommand;
        public DelegateCommand ImportCommand
        {
            get { return _importCommand ?? (_importCommand = new DelegateCommand(Import)); }
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
                    foreach (var routeViewModel in RouteViewModels ?? Enumerable.Empty<ReportViewModel>())
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
                        CreateRoute(_gpxPointsCollection, false);

                        var newRoute = new ReportViewModel(GpxTracePoints) { Nome = name };
                        RouteViewModels.Add(newRoute);
                        CurrentViewModel = newRoute;
                        //calcolo le proprietà della route
                        newRoute.CalculateStationaryPoints(CurrentViewModel);
                        newRoute.GetSubroutes(CurrentViewModel);
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

        private DelegateCommand _resetAll;
        public DelegateCommand ResetAll
        {
            get { return _resetAll ?? (_resetAll = new DelegateCommand(Reset)); }
        }

        public void Reset()
        {
            GpxPointsCollection = new ObservableCollection<GpxPoint>();
            GpxTracePoints = new ObservableCollection<GpxPoint>();
            Routes = new ObservableCollection<MapItem>();
            MapItems = new ObservableCollection<MapItem>();
            RouteViewModels = new ObservableCollection<ReportViewModel>();
            CurrentViewModel = new ReportViewModel(new ObservableCollection<GpxPoint>());
            HttpMessage.Reset();
        }

        private DelegateCommand _report;
        public DelegateCommand Report
        {
            get { return _report ?? (_report = new DelegateCommand(ShowReport)); }
        }

        public void ShowReport()
        {
            Report r = new Report
            {
                DataContext = this
            };
            r.ShowDialog();
        }
    }
}