using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

namespace MapRunner
{
    public class MyMapViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<PointOfCustomRoute> PointList { get; set; }
        public ObservableCollection<BasicGeoposition> RunPointList { get; set; }
        private bool _isRouting;
        private bool _isRunning;
        private double _runningLenght;
        private TimeSpan _runningTime;
        private DispatcherTimer _timer;
        private double _currentSpeed;

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

        public TimeSpan RunningTime
        {
            get
            {
                return _runningTime;
            }
            set
            {
                _runningTime = value;
                NotifyPropertyChanged("RunningTime");
            }
        }

        public double CurrentSpeed
        {
            get { return _currentSpeed; }
            set
            {
                _currentSpeed = value;
                NotifyPropertyChanged("CurrentSpeed");
            }
        }

        public MyMapViewModel()
        {
            PointList = new ObservableCollection<PointOfCustomRoute>();
            RunPointList = new ObservableCollection<BasicGeoposition>();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_tick;
        }

        private void Timer_tick(object sender, object o)
        {
            RunningTime += TimeSpan.FromSeconds(1);
        }

        public void StartRunning()
        {
            IsRunning = true;
            _timer.Start();
        }

        public void StopRunning()
        {
            _timer.Stop();
            IsRunning = false;
            RunningTime = TimeSpan.Zero;
        }

        public void PauseRunning()
        {
            IsRunning = false;
            _timer.Stop();
        }

        protected virtual void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
