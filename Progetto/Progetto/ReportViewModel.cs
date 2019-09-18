using DevExpress.Mvvm;
using Gpx;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Progetto
{
    public class ReportViewModel : ViewModelBase
    {
        public ReportViewModel()
        {
            routeViewModels = new ObservableCollection<RouteViewModel>();
            currentViewModel = new RouteViewModel();
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


        //BEHAVIOUR
        private DelegateCommand windowLoad;
        public DelegateCommand WindowLoad
        {
            get { return windowLoad ?? (windowLoad = new DelegateCommand(OnLoad)); }
        }

        public async void OnLoad()
        {
            //ricerca punti stazionari
            //NB: La lista di punti parte dall'ultimo punto di ordine cronologico
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
                CurrentViewModel.LunghezzaPercorso += CalcoloDistanza(CurrentViewModel.MainRoute[i], CurrentViewModel.MainRoute[i + 1]);
            }
        }

        public static double CalcoloDistanza(GpxPoint p1, GpxPoint p2)
        {
            /* Definisce le costanti e le variabili */
            const double R = 6371;
            double lat_alfa, lat_beta;
            double lon_alfa, lon_beta;
            double fi;
            double p, d;
            /* Converte i gradi in radianti */
            lat_alfa = Math.PI * p1.Latitude / 180;
            lat_beta = Math.PI * p2.Latitude / 180;
            lon_alfa = Math.PI * p1.Longitude / 180;
            lon_beta = Math.PI * p2.Longitude / 180;
            /* Calcola l'angolo compreso fi */
            fi = Math.Abs(lon_alfa - lon_beta);
            /* Calcola il terzo lato del triangolo sferico */
            p = Math.Acos(Math.Sin(lat_beta) * Math.Sin(lat_alfa) +
              Math.Cos(lat_beta) * Math.Cos(lat_alfa) * Math.Cos(fi));
            /* Calcola la distanza sulla superficie 
            terrestre R = ~6371 km */
            d = p * R;
            return (d);
        }


        public void CalculateStart()
        {
            CurrentViewModel.OraInizio = CurrentViewModel.MainRoute[0].Start.ToString();
        }

        public void CalculateEnd()
        {
            CurrentViewModel.OraFine = CurrentViewModel.MainRoute[CurrentViewModel.MainRoute.Count - 1].Start.ToString();
        }



        //public PDFCreator pdf = new PDFCreator();
        //private ObservableCollection<GpxPoint> points;
        //public ObservableCollection<GpxPoint> Points
        //{
        //    get { return points; }
        //    set { points = value; RaisePropertyChanged(); }
        //}


        //private DelegateCommand pdfReport;
        //public DelegateCommand PdfReport
        //{
        //    get { return pdfReport ?? (pdfReport = new DelegateCommand(Pdf)); }
        //}

        //public void Pdf()
        //{

        //    pdf.DoSomething();

        //}
    }
}