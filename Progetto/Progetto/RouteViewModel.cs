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

        public void Pdf(ChartControl chart)
        {
            PDFCreator pdf = new PDFCreator(Nome, VelocitaMedia, VelocitaMassima, VelocitaMinima, LunghezzaPercorso, OraInizio, OraFine, OraTot);
            pdf.CreaPDF(chart);
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

        public async Task GetStationaryPoints(RouteViewModel currentViewModel)
        {
            try
            {
                currentViewModel.PuntiStazionamento = new ObservableCollection<string>();
                int index = 0;
                for (int i = 0; i < currentViewModel.MainRoute.Count; i++)
                {
                    if (index - i > 0) { continue; }
                    if (currentViewModel.MainRoute[i].Speed == 0)
                    {
                        index = i + 1;
                        while (index < currentViewModel.MainRoute.Count && currentViewModel.MainRoute[index].Speed == 0)
                        {
                            index++;
                        }
                        var geocoderResult = await Gpx.Nominatim.GetAddress(currentViewModel.MainRoute[index - 1].Latitude, currentViewModel.MainRoute[index - 1].Longitude);
                        string address = geocoderResult.DisplayName;
                        TimeSpan span = currentViewModel.MainRoute[i].Start - currentViewModel.MainRoute[index - 1].Start;
                        if (span > new TimeSpan(0, 0, 10))
                            currentViewModel.PuntiStazionamento.Add($"Stazionamento alle: {currentViewModel.MainRoute[index - 1].Start}| di durata: {span}| a {address}");
                        else
                            currentViewModel.PuntiStazionamento.Add($"Stazionamento alle: {currentViewModel.MainRoute[index - 1].Start}| a {address}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CalculateMaxSpeed(RouteViewModel currentViewModel)
        {
            currentViewModel.VelocitaMassima = 0;
            foreach (var p in currentViewModel.MainRoute)
            {
                if (currentViewModel.VelocitaMassima < p.Speed)
                {
                    currentViewModel.VelocitaMassima = p.Speed;
                }
            }
        }

        public void CalculateMinSpeed(RouteViewModel currentViewModel)
        {
            currentViewModel.VelocitaMinima = 0;
            foreach (var p in currentViewModel.MainRoute)
            {
                if (currentViewModel.VelocitaMinima > p.Speed && p.Speed != 0)
                {
                    currentViewModel.VelocitaMinima = p.Speed;
                }
            }
        }

        public void CalculateMediumSpeed(RouteViewModel currentViewModel)
        {
            currentViewModel.VelocitaMedia = 0;
            double somma = 0;
            foreach (var p in currentViewModel.MainRoute)
            {
                somma += p.Speed;
            }
            currentViewModel.VelocitaMedia = somma / currentViewModel.MainRoute.Count;
        }

        public void CalculateRouteLenght(RouteViewModel currentViewModel)
        {
            currentViewModel.LunghezzaPercorso = 0;
            for (int i = 0; i < currentViewModel.MainRoute.Count - 1; i++)
            {
                double distanza = GpxReader.CalcoloDistanza(currentViewModel.MainRoute[i], currentViewModel.MainRoute[i + 1]);
                if (distanza != Double.NaN && !double.IsNaN(distanza))
                    currentViewModel.LunghezzaPercorso += distanza;
            }
        }

        public void CalculateTotalTime(RouteViewModel currentViewModel)
        {
            TimeSpan tot = currentViewModel.MainRoute[0].Start - currentViewModel.MainRoute[currentViewModel.MainRoute.Count - 1].Start;
            int d = tot.Days;
            string time = $"{tot.Hours.ToString()}:{tot.Minutes.ToString()}:{tot.Seconds.ToString()}";
            currentViewModel.OraTot = $"{ d.ToString()} {(d == 1 ? "giorno" : "giorni")} e {time}";
        }
        public void CalculateStart(RouteViewModel currentViewModel)
        {
            currentViewModel.OraInizio = currentViewModel.MainRoute[currentViewModel.MainRoute.Count - 1].Start.ToString();
        }

        public void CalculateEnd(RouteViewModel currentViewModel)
        {
            currentViewModel.OraFine = currentViewModel.MainRoute[0].Start.ToString();
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
