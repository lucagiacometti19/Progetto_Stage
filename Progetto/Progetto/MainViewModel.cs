using Gpx;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System.Xml;
using DevExpress.Xpf.Map;

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

        //Metodi per il salvataggio con XDocument
        private XDocument GetGpxDoc(string sFile)
        {
            XDocument gpxDoc = XDocument.Load(sFile);
            return gpxDoc;
        }

        private XNamespace GetGpxNameSpace()
        {
            XNamespace gpx = XNamespace.Get("http://www.topografix.com/GPX/1/1");
            return gpx;
        }

        private string LoadGPXWaypoints(string sFile)
        {
            XDocument gpxDoc = GetGpxDoc(sFile);
            XNamespace gpx = GetGpxNameSpace();

            var waypoints = from waypoint in gpxDoc.Descendants(gpx + "wpt")
                            select new
                            {
                                Latitude = waypoint.Attribute("lat").Value,
                                Longitude = waypoint.Attribute("lon").Value,
                                Dt = waypoint.Element(gpx + "time") != null ?
                                  waypoint.Element(gpx + "time").Value : null,
                            };


            StringBuilder sb = new StringBuilder();
            foreach (var wpt in waypoints)
            {
                sb.Append(
                  string.Format("{0},{1},{2}\n",
                  wpt.Latitude, wpt.Longitude, wpt.Dt));
            }

            return sb.ToString();
        }
        
        //Crea la lista di punti
        private void FillList(string file)
        {
            using (StringReader reader = new StringReader(file))
            {
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    string[] splittedLine = line.Split(',');
                    string latitude = splittedLine[0].Replace("°", "");
                    string longitude = splittedLine[1].Replace("°", "");
                    string time = splittedLine[2];
                    time = time.Replace('T', ' ');
                    time = time.Remove(time.IndexOf('Z'));

                    GpxPointsCollection.Add(new GpxPoint
                    {
                        Latitude = Convert.ToDouble(latitude, CultureInfo.InvariantCulture),
                        Longitude = Convert.ToDouble(longitude, CultureInfo.InvariantCulture),
                        Time = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Data = $"Time {time} - Lat: {latitude} - Lon: {longitude}"
                    });
                }
            }
        }

        //Crea la lista di Polyline
        private void CreatePolylines(ObservableCollection<GpxPoint> points)
        {
            MapPolyline pl = new MapPolyline();
            foreach(GpxPoint px in points)
            {
                pl.Points.Add(new GeoPoint(px.Latitude, px.Longitude));
            }
            PolylineCollection.Add(pl);
        }

        //Comando per importare i dati da file Xml
        private DelegateCommand importCommand;
        public DelegateCommand ImportCommand
        {
            get { return importCommand ?? (importCommand = new DelegateCommand(Import)); }
        }
        private void Import()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Importa file"
            };
            if ((bool)open.ShowDialog())
            {
                string gpxPoints = LoadGPXWaypoints(open.FileName);
                FillList(gpxPoints);
                CreatePolylines(GpxPointsCollection);
            }
        }
    }
}
