using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MapRunner
{
    public class LenghtWayStringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var format = parameter as string;
            if (!String.IsNullOrEmpty(format))
                return String.Format(format, value);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        private bool _invert;
        public bool Invert
        {
            get { return _invert; }
            set { _invert = value; }
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool flag = (bool) value;
            if (_invert)
            {
                return flag ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return flag ? Visibility.Visible : Visibility.Collapsed;
            }
            
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var visible = (Visibility) value;
            if (_invert)
            {
                return visible == Visibility.Collapsed;
            }
            else
            {
                return visible != Visibility.Collapsed;
            }
            
        }
    }

    public class BooleanStarStopConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool flag = (bool) value;
            return flag ? "Стоп" : "Старт";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
