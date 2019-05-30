using DevExpress.Mvvm;
using DevExpress.Xpf.Map;
using Gpx;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Progetto
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            //ImportCommand = new RelayCommand(Import);
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


        //private XDocument GetGpxDoc(string sFile)
        //{
        //    XDocument gpxDoc = XDocument.Load(sFile);
        //    return gpxDoc;
        //}

        //private XNamespace GetGpxNameSpace()
        //{
        //    XNamespace gpx = XNamespace.Get("http://www.topografix.com/GPX/1/1");
        //    return gpx;
        //}


        public async Task<ObservableCollection<GpxPoint>> ReadFromXml(string filePath)
        {
            ObservableCollection<GpxPoint> points = new ObservableCollection<GpxPoint>();
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;

                using (XmlReader reader = XmlReader.Create(new FileStream(filePath, FileMode.Open), settings))
                {
                    DateTime? time = null;
                    double? latitude = null;
                    double? longitude = null;
                    while (await reader.ReadAsync())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "wpt")
                                {
                                    latitude = Convert.ToDouble(reader.GetAttribute("lat").Replace("°", ""), CultureInfo.InvariantCulture);
                                    longitude = Convert.ToDouble(reader.GetAttribute("lon").Replace("°", ""), CultureInfo.InvariantCulture);
                                }
                                else if (reader.Name == "time")
                                {
                                    time = DateTime.ParseExact((await reader.ReadInnerXmlAsync()).Replace("T", " ").Replace("Z", ""), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                                break;

                            default:
                                break;
                        }
                        if ((latitude != null) && (longitude != null) && time != null)
                        {
                            points.Add(new GpxPoint()
                            {
                                Latitude = Convert.ToDouble(latitude),
                                Longitude = Convert.ToDouble(longitude),
                                Time = time,
                                Data = $"Time {time} - Lat: {latitude} - Lon: {longitude}"
                            });
                            time = null;
                            latitude = null;
                        }
                    }
                }
            }
            catch { }
            return points;
        }


        //private string LoadGPXWaypoints(string sFile)
        //{
        //    XNamespace gpx = GetGpxNameSpace();
        //    XDocument gpxDoc = GetGpxDoc(sFile);

        //    var waypoints = from waypoint in gpxDoc.
        //                    select new
        //                    {
        //                        Latitudine = gpxDoc.GetElementsByTagName("lat"),
        //                        Longitude = gpxDoc.GetElementsByTagName("lon"),
        //                        Dt = gpxDoc.GetElementsByTagName(gpx + "time") != null ?
        //                        gpxDoc.GetElementsByTagName(gpx + "time").Value : null,
        //                    };
        //    XDocument gpxDoc = GetGpxDoc(sFile);
        //    XNamespace gpx = GetGpxNameSpace();

        //    var waypoints = from waypoint in gpxDoc.Descendants(gpx + "wpt")
        //                    select new
        //                    {
        //                        Latitude = waypoint.Attribute("lat").Value,
        //                        Longitude = waypoint.Attribute("lon").Value,
        //                        Dt = waypoint.Element(gpx + "time") != null ?
        //                          waypoint.Element(gpx + "time").Value : null,
        //                    };


        //    StringBuilder sb = new StringBuilder();
        //    foreach (var wpt in waypoints)
        //    {
        //        This is where we'd instantiate data
        //         containers for the information retrieved.
        //        sb.Append(
        //          string.Format("{0},{1},{2}\n",
        //          wpt.Latitude, wpt.Longitude, wpt.Dt));
        //    }

        //    return sb.ToString();
        //}




        //private void FillList(string file)
        //{
        //    using (StringReader reader = new StringReader(file))
        //    {
        //        string line = "";

        //        while ((line = reader.ReadLine()) != null)
        //        {
        //            string[] splittedLine = line.Split(',');
        //            string latitude = splittedLine[0].Replace("°", "");
        //            string longitude = splittedLine[1].Replace("°", "");
        //            string time = splittedLine[2];
        //            time = time.Replace('T', ' ');
        //            time = time.Remove(time.IndexOf('Z'));

        //            GpxPointsCollection.Add(new GpxPoint
        //            {
        //                Latitude = Convert.ToDouble(latitude, CultureInfo.InvariantCulture),
        //                Longitude = Convert.ToDouble(longitude, CultureInfo.InvariantCulture),
        //                Time = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        //                Data = $"Time {time} - Lat: {latitude} - Lon: {longitude}"
        //            });
        //        }
        //    }
        //}


        private void CreatePolylines(ObservableCollection<GpxPoint> points)
        {
            MapPolyline pl = new MapPolyline();
            foreach (GpxPoint px in points)
            {
                pl.Points.Add(new GeoPoint(px.Latitude, px.Longitude));
            }
            PolylineCollection.Add(pl);
        }

        //public ICommand ImportCommand { get; set; }
        //private void Import(object obj)
        //{
        //    OpenFileDialog open = new OpenFileDialog
        //    {
        //        Filter = "Xml files (*.xml)|*.xml",
        //        Title = "Importa file"
        //    };
        //    if ((bool)open.ShowDialog())
        //    {
        //        string gpxPoints = LoadGPXWaypoints(open.FileName);
        //        FillList(gpxPoints);
        //    }
        //}

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
                GpxPointsCollection = await ReadFromXml(open.FileName);
                CreatePolylines(GpxPointsCollection);
            }
        }
    }
}
