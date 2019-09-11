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

        private ObservableCollection<GpxPoint> puntiStazionamento;
        public ObservableCollection<GpxPoint> PuntiStazionamento
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
    }
}