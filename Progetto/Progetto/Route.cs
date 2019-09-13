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
    public class Route : ViewModelBase
    {
        public Route()
        {
            segmentsCollection = new ObservableCollection<Route>();
            mainRoute = new ObservableCollection<GpxPoint>();
            puntiStazionamento = new ObservableCollection<string>();
            velocitaMedia = 0;
            velocitaMassima = 0;
            velocitaMinima = 0;
            lunghezzaPercorso = 0;
            oraFine = "";
            oraInizio = "";
        }

        private ObservableCollection<Route> segmentsCollection;

        public ObservableCollection<Route> SegmentsCollection
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
            set { puntiStazionamento = value; RaisePropertyChanged(); }
        }

        private double velocitaMedia;
        public double VelocitaMedia
        {
            get { return velocitaMedia; }
            set { velocitaMedia = value; RaisePropertyChanged(); }
        }

        private double velocitaMassima;
        public double VelocitaMassima
        {
            get { return velocitaMassima; }
            set { velocitaMassima = value; RaisePropertyChanged(); }
        }

        private double velocitaMinima;
        public double VelocitaMinima
        {
            get { return velocitaMinima; }
            set { velocitaMinima = value; RaisePropertyChanged(); }
        }

        private double lunghezzaPercorso;
        public double LunghezzaPercorso
        {
            get { return lunghezzaPercorso; }
            set { lunghezzaPercorso = value; RaisePropertyChanged(); }
        }

        private string oraInizio;
        public string OraInizio
        {
            get { return oraInizio; }
            set { oraInizio = value; RaisePropertyChanged(); }
        }

        private string oraFine;
        public string OraFine
        {
            get { return oraFine; }
            set { oraFine = value; RaisePropertyChanged(); }
        }

        private string nome;

        public string Nome
        {
            get { return nome; }
            set { nome = value; RaisePropertyChanged(); }
        }
    }
}
