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
