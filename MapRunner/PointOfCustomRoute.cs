using System;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace MapRunner
{
    public class PointOfCustomRoute : INotifyPropertyChanged
    {
        private const int RADIUS_EARTH = 6372795; //радиус Земли в метрах
        private static int _count;
        private double _currentLenght;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Count { get; private set; }
        public Geopoint Location { get; set; }

        public double CurrentLenght
        {
            get { return _currentLenght; }
            set
            {
                if (!(Math.Abs(value - _currentLenght) > 0.00001)) return;
                _currentLenght = value;
                NotifyPropertyChanged("CurrentLenght");
            }
        }
        public Point NormalizedAnchorPoint { get; set; }

        public PointOfCustomRoute()
        {
            Count = ++_count;
        }

        public static double GetDistance(PointOfCustomRoute p1, PointOfCustomRoute p2)
        {
            //Y=atan{sqrt(a+(b-c)^2)/(d+e)}
            double lat1 = p1.Location.Position.Latitude * Math.PI / 180;
            double long1 = p1.Location.Position.Longitude * Math.PI / 180;
            double lat2 = p2.Location.Position.Latitude * Math.PI / 180;
            double long2 = p2.Location.Position.Longitude * Math.PI / 180;
            double a = Math.Pow(Math.Cos(lat2) * Math.Sin(long2 - long1), 2);
            double b = Math.Cos(lat1) * Math.Sin(lat2);
            double c = Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(long2 - long1);
            double d = Math.Sin(lat1) * Math.Sin(lat2);
            double e = Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(long2 - long1);
            double Y = Math.Atan(Math.Sqrt(a + Math.Pow(b - c, 2)) / (d + e));

            return RADIUS_EARTH * Y / 1000;
        }

        protected virtual void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
