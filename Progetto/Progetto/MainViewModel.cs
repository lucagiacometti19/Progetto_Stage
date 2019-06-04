using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Microsoft.Win32;
using System;
using Gpx;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private ObservableCollection<GeoPoint> geoPointsCollection;
        public ObservableCollection<GeoPoint> GeoPointsCollection
        {
            get { return geoPointsCollection; }
            set { geoPointsCollection = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<MapPolyline> polylineCollection;
        public ObservableCollection<MapPolyline> PolylineCollection
        {
            get { return polylineCollection; }
            set { polylineCollection = value; RaisePropertyChanged(); }
        }

        private void CreatePolylines(ObservableCollection<GeoPoint> points)
        {
            MapPolyline pl = new MapPolyline();
            pl.Stroke = new SolidColorBrush(Color.FromRgb(0, 17, 255));
            foreach (GeoPoint px in points)
            {
                pl.Points.Add(px);
            }
            PolylineCollection.Add(pl);
        }


        private DelegateCommand importCommand;
        public DelegateCommand ImportCommand
        {
            get { return importCommand ?? (importCommand = new DelegateCommand(Import)); }
        }

        private async void Import()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Importa file"
            };
            if ((bool)open.ShowDialog())
            {
                GpxPointsCollection = await GpxReader.ReadFromXml(open.FileName);
                GeoPoint p1 = new GeoPoint() { Latitude = 43.264206, Longitude = 11.550751, };
                GeoPoint p2 = new GeoPoint() { Latitude = 43.110007,  Longitude = 11.535645, };
                string request = HttpMessage.RequestAssembler(p1, p2);
                await HttpMessage.RunAsync(request);
                GeoPointsCollection = HttpMessage.Point;
                CreatePolylines(GeoPointsCollection);
            }
        }
    }
}