using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Gpx;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
            Console.WriteLine("Creando la polilinea..");

            MapPolyline pl = new MapPolyline();
            pl.Stroke = new SolidColorBrush(Color.FromRgb(0, 17, 255));
            foreach (GeoPoint px in points)
            {
                pl.Points.Add(px);
            }
            PolylineCollection.Add(pl);
            Console.WriteLine("Polilinea creata con successo");

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
                for (int c = 0; c < GpxPointsCollection.Count; c = c + 35)
                {
                    List<GeoPoint> g = new List<GeoPoint>();
                    for (int i = 0; (i < 35) && (i + c) < GpxPointsCollection.Count; i++)
                    {
                        GeoPoint p = null;
                        if (c < 35)
                        {
                            p = new GeoPoint() { Latitude = GpxPointsCollection[i].Latitude, Longitude = GpxPointsCollection[i].Longitude };
                        }
                        else if (c >= 35)
                        {
                            p = new GeoPoint() { Latitude = GpxPointsCollection[i + c].Latitude, Longitude = GpxPointsCollection[i + c].Longitude };
                        }
                        g.Add(p);
                    }
                    HttpMessage.RequestAssembler(g);
                    Console.WriteLine($"Request {c} ok");
                }

                int c2 = 0;
                foreach (string s in HttpMessage.Requests)
                {
                    await HttpMessage.RunAsync(s);
                    Console.WriteLine($"RunAsync {c2} ok");
                    c2++;
                }

                int c3 = 0;
                foreach (string s in HttpMessage.Results)
                {
                    HttpMessage.ConvertFromJson(s);
                    Console.WriteLine($"RunAsync {c3} ok");
                    c3++;
                }

                GeoPointsCollection = HttpMessage.Point;
                Console.WriteLine(GeoPointsCollection.Count);
                CreatePolylines(GeoPointsCollection);
            }
        }
    }
}