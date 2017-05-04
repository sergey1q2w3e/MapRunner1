using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapRunner
{
    public class MyMapViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<PointOfCustomRoute> PointList { get; set; }
        private bool _isRouting ;

        public bool IsRouting
        {
            get { return _isRouting; }
            set
            {
                _isRouting = value;
                NotifyPropertyChanged("IsRouting");
            }
        }

        public MyMapViewModel()
        {
            PointList = new ObservableCollection<PointOfCustomRoute>();
        }

        protected virtual void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
