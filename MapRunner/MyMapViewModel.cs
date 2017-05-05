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
        private bool _isRouting;
        private bool _isRunning;
        private double _runningLenght;

        public bool IsRouting
        {
            get { return _isRouting; }
            set
            {
                _isRouting = value;
                NotifyPropertyChanged("IsRouting");
            }
        }
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                NotifyPropertyChanged("IsRunning");
            }
        }
        public double RunningLenght
        {
            get { return _runningLenght; }
            set
            {
                _runningLenght = value;
                NotifyPropertyChanged("RunningLenght");
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
