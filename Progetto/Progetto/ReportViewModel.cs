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
        private ObservableCollection<GpxPoint> points;
        public ObservableCollection<GpxPoint> Points
        {
            get { return points; }
            set { points = value; RaisePropertyChanged(); }
        }

        //public struct ChartPoint
        //{
        //    public TimeSpan Start;
        //    public double Speed;
        //}
    }
}
