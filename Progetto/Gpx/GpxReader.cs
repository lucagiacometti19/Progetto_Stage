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
                                Time = Convert.ToDateTime(time)
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
    }
}
