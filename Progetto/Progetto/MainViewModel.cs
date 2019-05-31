using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Gpx;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace Progetto
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            GpxPointsCollection = new ObservableCollection<GpxPoint>();
            PolylineCollection = new ObservableCollection<MapPolyline>();
        }

        private ObservableCollection<GpxPoint> gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return gpxPointsCollection; }
            set { gpxPointsCollection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<MapPolyline> polylineCollection;
        public ObservableCollection<MapPolyline> PolylineCollection
        {
            get { return polylineCollection; }
            set { polylineCollection = value; RaisePropertyChanged(); }
        }


        private void CreatePolylines(ObservableCollection<GpxPoint> points)
        {
            MapPolyline pl = new MapPolyline();
            pl.Stroke = new SolidColorBrush(Color.FromRgb(0, 17, 255));    //))/*(Color.FromRgb(Convert.ToByte(new Random(DateTime.Now.Millisecond).Next() % 256), Convert.ToByte(new Random(DateTime.Now.Millisecond).Next() % 256), Convert.ToByte(new Random(DateTime.Now.Millisecond).Next() % 256)));*/
            //pl.Fill = new SolidColorBrush(Color.FromRgb(Convert.ToByte(new Random(DateTime.Now.Millisecond).Next() % 256), Convert.ToByte(new Random(DateTime.Now.Millisecond).Next() % 256), Convert.ToByte(new Random(DateTime.Now.Millisecond).Next() % 256)));
            foreach (GpxPoint px in points)
            {
                pl.Points.Add(new GeoPoint(px.Latitude, px.Longitude));
            }
            PolylineCollection.Add(pl);
        }


        private AsyncCommand importCommand;
        public AsyncCommand ImportCommand
        {
            get { return importCommand ?? (importCommand = new AsyncCommand(Import)); }
        }
        private async Task Import()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Importa file"
            };
            if ((bool)open.ShowDialog())
            {
                GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);
                CreatePolylines(GpxPointsCollection);
            }
        }
    }
}
