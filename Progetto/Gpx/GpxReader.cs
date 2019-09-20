using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Gpx
{
    public static class GpxReader
    {
        public static async Task<ObservableCollection<GpxPoint>> ReadFromXml(string filePath)
        {

            ObservableCollection<GpxPoint> points = new ObservableCollection<GpxPoint>();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true
            };

            using (XmlReader reader = XmlReader.Create(new FileStream(filePath, FileMode.Open), settings))
            {
                DateTime? time = null;
                double? latitude = null, longitude = null;
                double latTot = 0, lonTot = 0;
                GpxPoint p0 = new GpxPoint();
                bool Esegui = true;
                int cont = 1;
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
                        GpxPoint p1 = new GpxPoint()
                        {
                            Latitude = Convert.ToDouble(latitude),
                            Longitude = Convert.ToDouble(longitude),
                            Start = Convert.ToDateTime(time),
                            Finish = Convert.ToDateTime(time)
                        };

                        if (points.Count == 0)
                        {
                            points.Add(p1);
                            p0 = points[points.Count - 1];
                            latTot = p1.Latitude;
                            lonTot = p1.Longitude;
                        }
                        else
                        {
                            TimeSpan t = p0.Start - p1.Start;

                            /* Converte i gradi in radianti e calcola l'angolo compreso fi */
                            double lat_alfa = Math.PI * p0.Latitude / 180;
                            double lat_beta = Math.PI * p1.Latitude / 180;
                            double lon_alfa = Math.PI * p0.Longitude / 180;
                            double lon_beta = Math.PI * p1.Longitude / 180;
                            double fi = Math.Abs(lon_alfa - lon_beta);
                            /* Calcola il terzo lato del triangolo sferico */
                            double p = Math.Acos(Math.Sin(lat_beta) * Math.Sin(lat_alfa) +
                              Math.Cos(lat_beta) * Math.Cos(lat_alfa) * Math.Cos(fi));

                            p1.Speed = p * 6371 / t.TotalHours;

                            latTot += p1.Latitude;
                            lonTot += p1.Longitude;
                            cont++;

                            if (p1.Speed > 3)
                            {
                                p1.Latitude = Esegui ? p1.Latitude : (latTot + p1.Latitude) / (cont + 1);
                                p1.Longitude = Esegui ? p1.Longitude : (lonTot + p1.Longitude) / (cont + 1);
                                p1.Finish = points[points.Count - 1].Start;
                                points.Add(p1);
                                cont = 1;
                                latTot = p1.Latitude;
                                lonTot = p1.Longitude;
                                p0 = points[points.Count - 1];
                                Esegui = true;
                            }
                            else
                            {
                                latTot += p1.Latitude;
                                lonTot += p1.Longitude;
                                cont++;
                                p0 = p1;
                                Esegui = false;
                            }
                        }
                        time = null;
                        latitude = null;
                    }
                }
                reader.Close();
            }
            return points;

        }



        //public static async Task<ObservableCollection<GpxPoint>> ReadFromXml(string filePath)
        //{
        //    ObservableCollection<GpxPoint> points = new ObservableCollection<GpxPoint>();
        //    XmlReaderSettings settings = new XmlReaderSettings();
        //    settings.Async = true;

        //    using (XmlReader reader = XmlReader.Create(new FileStream(filePath, FileMode.Open), settings))
        //    {
        //        DateTime? time = null;
        //        double? latitude = null;
        //        double? longitude = null;
        //        while (await reader.ReadAsync())
        //        {
        //            switch (reader.NodeType)
        //            {
        //                case XmlNodeType.Element:
        //                    if (reader.Name == "wpt")
        //                    {
        //                        latitude = Convert.ToDouble(reader.GetAttribute("lat").Replace("°", ""), CultureInfo.InvariantCulture);
        //                        longitude = Convert.ToDouble(reader.GetAttribute("lon").Replace("°", ""), CultureInfo.InvariantCulture);
        //                    }
        //                    else if (reader.Name == "time")
        //                    {
        //                        time = DateTime.ParseExact((await reader.ReadInnerXmlAsync()).Replace("T", " ").Replace("Z", ""), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        //                    }
        //                    break;

        //                default:
        //                    break;
        //            }
        //            if ((latitude != null) && (longitude != null) && time != null)
        //            {
        //                GpxPoint point = new GpxPoint
        //                {
        //                    Latitude = Convert.ToDouble(latitude),
        //                    Longitude = Convert.ToDouble(longitude),
        //                    Time = Convert.ToDateTime(time)
        //                };

        //                if (points.Count == 0)
        //                {
        //                    points.Add(point);
        //                    time = null;
        //                    latitude = null;
        //                }
        //                else /*if (Tolleranza(point, points[points.Count - 1], 0.04))*/
        //                {
        //                    points.Add(point);
        //                    time = null;
        //                    latitude = null;
        //                }

        //                time = null;
        //                latitude = null;
        //            }
        //        }
        //    }
        //    return points;
        //}


        public static double CalcoloDistanza(GpxPoint p1, GpxPoint p2)
        {
            /* Definisce le costanti e le variabili */
            const double R = 6371;
            double lat_alfa, lat_beta;
            double lon_alfa, lon_beta;
            double fi;
            double p, d;
            /* Converte i gradi in radianti */
            lat_alfa = Math.PI * p1.Latitude / 180;
            lat_beta = Math.PI * p2.Latitude / 180;
            lon_alfa = Math.PI * p1.Longitude / 180;
            lon_beta = Math.PI * p2.Longitude / 180;
            /* Calcola l'angolo compreso fi */
            fi = Math.Abs(lon_alfa - lon_beta);
            /* Calcola il terzo lato del triangolo sferico */
            p = Math.Acos(Math.Sin(lat_beta) * Math.Sin(lat_alfa) +
              Math.Cos(lat_beta) * Math.Cos(lat_alfa) * Math.Cos(fi));
            /* Calcola la distanza sulla superficie 
            terrestre R = ~6371 km */
            d = p * R;
            return (d);
        }

        //private static bool Tolleranza(GpxPoint p1, GpxPoint p2, double tolleranza)
        //{
        //    if (CalcoloDistanza(p1, p2) > tolleranza)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
