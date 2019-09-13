using DevExpress.Mvvm;
using Gpx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto
{
    public class RouteViewModel : ViewModelBase
    {
        public RouteViewModel()
        {
            segmentsCollection = new ObservableCollection<ObservableCollection<GpxPoint>>();
            mainRoute = new ObservableCollection<GpxPoint>();
            puntiStazionamento = new ObservableCollection<string>();
            velocitaMedia = 0;
            velocitaMassima = 0;
            velocitaMinima = 0;
            lunghezzaPercorso = 0;
            oraFine = "";
            oraInizio = "";
        }

        private ObservableCollection<ObservableCollection<GpxPoint>> segmentsCollection;

        public ObservableCollection<ObservableCollection<GpxPoint>> SegmentsCollection
        {
            get { return segmentsCollection; }
            set { segmentsCollection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<GpxPoint> mainRoute;

        public ObservableCollection<GpxPoint> MainRoute
        {
            get { return mainRoute; }
            set { mainRoute = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<string> puntiStazionamento;
        public ObservableCollection<string> PuntiStazionamento
        {
            get { return puntiStazionamento; }
            set { puntiStazionamento = value; RaisePropertiesChanged(); }
        }

        private double velocitaMedia;
        public double VelocitaMedia
        {
            get { return velocitaMedia; }
            set { velocitaMedia = value; RaisePropertiesChanged(); }
        }

        private double velocitaMassima;
        public double VelocitaMassima
        {
            get { return velocitaMassima; }
            set { velocitaMassima = value; RaisePropertiesChanged(); }
        }

        private double velocitaMinima;
        public double VelocitaMinima
        {
            get { return velocitaMinima; }
            set { velocitaMinima = value; RaisePropertiesChanged(); }
        }

        private double lunghezzaPercorso;
        public double LunghezzaPercorso
        {
            get { return lunghezzaPercorso; }
            set { lunghezzaPercorso = value; RaisePropertiesChanged(); }
        }

        private string oraInizio;
        public string OraInizio
        {
            get { return oraInizio; }
            set { oraInizio = value; RaisePropertiesChanged(); }
        }

        private string oraFine;
        public string OraFine
        {
            get { return oraFine; }
            set { oraFine = value; RaisePropertiesChanged(); }
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
                    string address = geocoderResult.DisplayName;
                    TimeSpan span = MainRoute[i].Start - MainRoute[index - 1].Start;
                    if (span > new TimeSpan(0, 0, 10))
                        PuntiStazionamento.Add($"Stazionamento alle: {MainRoute[index - 1].Start}| di durata: {span}| a {address}");
                    else
                        PuntiStazionamento.Add($"Stazionamento alle: {MainRoute[index - 1].Start}| a {address}");
                }
            }
        }

    }
}
