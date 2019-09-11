using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using Gpx;

namespace Progetto
{ 
    class ReportViewModel : ViewModelBase
    {
        public PDFCreator pdf = new PDFCreator();
        private ObservableCollection<GpxPoint> points;
        public ObservableCollection<GpxPoint> Points
        {
            get { return points; }
            set { points = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<string> puntiStazionamento;
        public ObservableCollection<string> PuntiStazionamento
        {
            get { return puntiStazionamento; }
            set { puntiStazionamento = value; RaisePropertiesChanged(); }
        }

        private ObservableCollection<GpxPoint> velocitaMedia;
        public ObservableCollection<GpxPoint> VelocitaMedia
        {
            get { return velocitaMedia; }
            set { velocitaMedia = value; RaisePropertiesChanged(); }
        }

        private ObservableCollection<GpxPoint> velocitaMassima;
        public ObservableCollection<GpxPoint> VelocitaMassima
        {
            get { return velocitaMassima; }
            set { velocitaMassima = value; RaisePropertiesChanged(); }
        }

        private ObservableCollection<GpxPoint> velocitaMinima;
        public ObservableCollection<GpxPoint> VelocitaMinima
        {
            get { return velocitaMinima; }
            set { velocitaMinima = value; RaisePropertiesChanged(); }
        }

        private ObservableCollection<GpxPoint> lunghezzaPercorso;
        public ObservableCollection<GpxPoint> LunghezzaPercorso
        {
            get { return lunghezzaPercorso; }
            set { lunghezzaPercorso = value; RaisePropertiesChanged(); }
        }

        private ObservableCollection<GpxPoint> oraInizio;
        public ObservableCollection<GpxPoint> OraInizio
        {
            get { return oraInizio; }
            set { oraInizio = value; RaisePropertiesChanged(); }
        }

        private ObservableCollection<GpxPoint> oraFine;
        public ObservableCollection<GpxPoint> OraFine
        {
            get { return oraFine; }
            set { oraFine = value; RaisePropertiesChanged(); }
        }



        private DelegateCommand pdfReport;
        public DelegateCommand PdfReport
        {
            get { return pdfReport ?? (pdfReport = new DelegateCommand(Pdf)); }
        }

        public void Pdf()
        {
            
            pdf.DoSomething();

        }

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
            for (int i = 0; i < Points.Count; i++)
            {
                if (index - i > 0) { continue; }
                if (Points[i].Speed == 0)
                {
                    index = i + 1;
                    while (index < Points.Count && Points[index].Speed == 0)
                    {
                        index++;
                    }
                    var geocoderResult = await Gpx.Nominatim.GetAddress(Points[index - 1].Latitude, Points[index - 1].Longitude);
                    string address = geocoderResult.DisplayName;
                    TimeSpan span = Points[i].Start - Points[index - 1].Start;
                    if (span > new TimeSpan(0, 0, 10))
                        PuntiStazionamento.Add($"Stazionamento alle: {Points[index - 1].Start}| di durata: {span}| a {address}");
                    else
                        PuntiStazionamento.Add($"Stazionamento alle: {Points[index - 1].Start}| a {address}");
                }
            }
        }
    }
}