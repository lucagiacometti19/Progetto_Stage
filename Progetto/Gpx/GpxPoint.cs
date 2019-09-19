using System;

namespace Gpx
{
    public class GpxPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}
