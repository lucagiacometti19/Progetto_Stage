using DevExpress.Mvvm;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.NavBar;
using Gpx;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Progetto
{
    public class RouteViewModel : ViewModelBase
    {
        public RouteViewModel(ObservableCollection<GpxPoint> mainRoute)
        {
            _segmentsCollection = new ObservableCollection<RouteViewModel>();
            _mainRoute = mainRoute;
            _puntiStazionamento = new ObservableCollection<string>();
            _velocitaMedia = 0;
            _velocitaMassima = 0;
            _velocitaMinima = 0;
            _lunghezzaPercorso = 0;
            _oraFine = "";
            _oraInizio = "";
            _selectedItem = new NavBarItem();
            _selectedSegment = MainRoute;
        }

        private ObservableCollection<RouteViewModel> _segmentsCollection;

        public ObservableCollection<RouteViewModel> SegmentsCollection
        {
            get { return _segmentsCollection; }
            set { _segmentsCollection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<GpxPoint> _mainRoute;

        public ObservableCollection<GpxPoint> MainRoute
        {
            get { return _mainRoute; }
            set { _mainRoute = value; RaisePropertyChanged(); }
        }

        private NavBarItem _selectedItem;
        public NavBarItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SelectedSegment = SegmentsCollection.FirstOrDefault(x => x.Nome == value.Content.ToString()).MainRoute;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<GpxPoint> _selectedSegment;
        public ObservableCollection<GpxPoint> SelectedSegment
        {
            get { return _selectedSegment; }
            set { _selectedSegment = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<string> _puntiStazionamento;
        public ObservableCollection<string> PuntiStazionamento
        {
            get { return _puntiStazionamento; }
            set { _puntiStazionamento = value; RaisePropertyChanged(); }
        }

        private double _velocitaMedia;
        public double VelocitaMedia
        {
            get { return _velocitaMedia; }
            set { _velocitaMedia = value; RaisePropertyChanged(); }
        }

        private double _velocitaMassima;
        public double VelocitaMassima
        {
            get { return _velocitaMassima; }
            set { _velocitaMassima = value; RaisePropertyChanged(); }
        }

        private double _velocitaMinima;
        public double VelocitaMinima
        {
            get { return _velocitaMinima; }
            set { _velocitaMinima = value; RaisePropertyChanged(); }
        }

        private double _lunghezzaPercorso;
        public double LunghezzaPercorso
        {
            get { return _lunghezzaPercorso; }
            set { _lunghezzaPercorso = value; RaisePropertyChanged(); }
        }

        private string _oraInizio;
        public string OraInizio
        {
            get { return _oraInizio; }
            set { _oraInizio = value; RaisePropertyChanged(); }
        }

        private string _oraFine;
        public string OraFine
        {
            get { return _oraFine; }
            set { _oraFine = value; RaisePropertyChanged(); }
        }

        private string _nome;

        public string Nome
        {
            get { return _nome; }
            set { _nome = value; RaisePropertyChanged(); }
        }

        private string _oraTot;

        public string OraTot
        {
            get { return _oraTot; }
            set { _oraTot = value; }
        }


        private DelegateCommand<ChartControl> pdfReport;
        public DelegateCommand<ChartControl> PdfReport
        {
            get { return pdfReport ?? (pdfReport = new DelegateCommand<ChartControl>(Pdf)); }
        }
        
        public async void Pdf(ChartControl chart)
        {
            if (MainRoute != Enumerable.Empty<GpxPoint>() && MainRoute.Count != 0)
            {
                CalculateMaxSpeed();
                CalculateMediumSpeed();
                CalculateMinSpeed();
                CalculateRouteLenght();
                CalculateStart();
                CalculateEnd();
                CalculateTotalTime();
                await GetStationaryPoints();
                PDFCreator pdf = new PDFCreator(Nome, VelocitaMedia, VelocitaMassima, VelocitaMinima, LunghezzaPercorso, OraInizio, OraFine, OraTot, PuntiStazionamento);
                pdf.CreaPDF(chart);
            }
        }


        #region Metodi per il report
        public void CalculateStationaryPoints(RouteViewModel currentViewModel)
        {
            var mainRoute = currentViewModel.MainRoute;
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

        public async Task GetStationaryPoints()
        {
            try
            {
                PuntiStazionamento = new ObservableCollection<string>();
                int index = 0;
                for (int i = 0; i < MainRoute.Count; i++)
                {
                    if (index - i > 0) { continue; }
                    if (MainRoute[i].Speed == 0)
                    {
                        index = i + 1;
                        while (index < MainRoute.Count && MainRoute[index].Speed == 0)
                        {
                            index++;
                        }
                        var geocoderResult = await Gpx.Nominatim.GetAddress(MainRoute[index - 1].Latitude, MainRoute[index - 1].Longitude);

                        string val = "";
                        if(geocoderResult.Address.Village != null)
                        {
                            val = geocoderResult.Address.Village;
                        }
                        else if(geocoderResult.Address.Town != null)
                        {
                            val = geocoderResult.Address.Town;
                        }
                        else
                        {
                            val = geocoderResult.Address.City;
                        }

                        string address = $"{(geocoderResult.Address.Road != null ? $"{geocoderResult.Address.Road}, " : "")}" +
                            $"{val}, " +
                            $"{(geocoderResult.Address.County != null ? $"{geocoderResult.Address.Country}, ": "")}" +
                            $"{(geocoderResult.Address.State != null ? $"{geocoderResult.Address.State}": "")}";
                        TimeSpan span = MainRoute[i].Start - MainRoute[index - 1].Start;
                        if (span > new TimeSpan(0, 0, 10))
                            PuntiStazionamento.Add($"Stazionamento alle: {MainRoute[index - 1].Start} di durata: {span}| {address}");
                        else
                            PuntiStazionamento.Add($"Stazionamento alle: {MainRoute[index - 1].Start}| {address}");
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
            VelocitaMassima = 0;
            foreach (var p in MainRoute)
            {
                if (VelocitaMassima < p.Speed)
                {
                    VelocitaMassima = p.Speed;
                }
            }
        }

        public void CalculateMinSpeed()
        {
            VelocitaMinima = 150;
            foreach (var p in MainRoute)
            {
                if (VelocitaMinima > p.Speed && p.Speed != 0)
                {
                    VelocitaMinima = p.Speed;
                }
            }
        }

        public void CalculateMediumSpeed()
        {
            VelocitaMedia = 0;
            double somma = 0;
            foreach (var p in MainRoute)
            {
                somma += p.Speed;
            }
            VelocitaMedia = somma / MainRoute.Count;
        }

        public void CalculateRouteLenght()
        {
            LunghezzaPercorso = 0;
            for (int i = 0; i < MainRoute.Count - 1; i++)
            {
                double distanza = GpxReader.CalcoloDistanza(MainRoute[i], MainRoute[i + 1]);
                if (distanza != double.NaN && !double.IsNaN(distanza))
                    LunghezzaPercorso += distanza;
            }
        }

        public void CalculateTotalTime()
        {
            TimeSpan tot = MainRoute[0].Start - MainRoute[MainRoute.Count - 1].Start;
            int d = tot.Days;
            string time = $"{tot.Hours.ToString()}:{tot.Minutes.ToString()}:{tot.Seconds.ToString()}";
            OraTot = $"{ d.ToString()} {(d == 1 ? "giorno" : "giorni")} e {time}";
        }
        public void CalculateStart()
        {
            OraInizio = MainRoute[MainRoute.Count - 1].Start.ToString();
        }

        public void CalculateEnd()
        {
            OraFine = MainRoute[0].Start.ToString();
        }

        //Eseguire sempre prima "CalculateStationaryPoints" poi "GetSubroutes"
        public void GetSubroutes(RouteViewModel currentViewModel)
        {
            ObservableCollection<GpxPoint> subRoute = new ObservableCollection<GpxPoint>();
            bool alreadyAdded = true;
            int index = 1;
            foreach (var point in currentViewModel.MainRoute ?? Enumerable.Empty<GpxPoint>())
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
                        currentViewModel.SegmentsCollection.Add(new RouteViewModel(subRoute) { Nome = $"Sotto-route {index}" });
                        index++;
                        subRoute = new ObservableCollection<GpxPoint>();
                        alreadyAdded = true;
                    }
                    else
                    {
                        subRoute = new ObservableCollection<GpxPoint>();
                    }
                }
            }
        }
        #endregion
    }
}
