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
                //for (int i = 0; i < GpxPointsCollection.Count - 1; i++)
                //{
                //    GeoPoint p1 = new GeoPoint() { Latitude = GpxPointsCollection[i].Latitude, Longitude = GpxPointsCollection[i].Longitude };
                //    GeoPoint p2 = new GeoPoint() { Latitude = GpxPointsCollection[i + 1].Latitude, Longitude = GpxPointsCollection[i + 1].Longitude };
                //    string request = HttpMessage.RequestAssembler(p1, p2);
                //    Console.WriteLine($"Request {i} ok");
                //    await HttpMessage.RunAsync(request);
                //    Console.WriteLine($"RunAsync {i} ok");
                //    Console.WriteLine(GpxPointsCollection.Count - 1);
                //    Console.WriteLine(i);
                //}
                for (int c = 0; c < GpxPointsCollection.Count; c = c + 25)
                {
                    List<GeoPoint> g = new List<GeoPoint>();
                    for (int i = 0; (i < 25) && (i + c) < GpxPointsCollection.Count; i++)
                    {
                        GeoPoint p = null;
                        if (c < 25)
                        {
                            p = new GeoPoint() { Latitude = GpxPointsCollection[i].Latitude, Longitude = GpxPointsCollection[i].Longitude };
                        }
                        else if (c >= 25)
                        {
                            p = new GeoPoint() { Latitude = GpxPointsCollection[i + c].Latitude, Longitude = GpxPointsCollection[i + c].Longitude };
                        }
                        g.Add(p);
                    }
                    string request = HttpMessage.RequestAssembler(g);
                    Console.WriteLine($"Request {c} ok");
                    await HttpMessage.RunAsync(request);
                    Console.WriteLine($"RunAsync {c} ok");
                    Console.WriteLine(GpxPointsCollection.Count - 1);
                    Console.WriteLine(c);
                }

                //for (double i = 0; i < 24; i++)
                //{
                //    double x = i * 0.001;
                //    double la = (45.61472 + x);
                //    double lo = (10.1017983 + x);

                //    g.Add(new GeoPoint() { Latitude = la, Longitude = lo, });
                //}
                //string request = HttpMessage.RequestAssembler(g);
                //await HttpMessage.RunAsync(request);

                GeoPointsCollection = HttpMessage.Point;
                Console.WriteLine(GeoPointsCollection.Count);
                CreatePolylines(GeoPointsCollection);
            }
        }
    }
}