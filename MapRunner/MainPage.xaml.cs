using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace MapRunner
{
    public sealed partial class MainPage : Page
    {
        private RandomAccessStreamReference _mapIconStreamReference;
        private MapPolyline _lastPolyline;
        private MapIcon _lastPosition;
        public MyMapViewModel MyMapVM;
        public MainPage()
        {
            this.InitializeComponent();
            _mapIconStreamReference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/pin48.png"));
            // Check if Streetside is supported.
            //Show();
            //ShowRouteOnMap();
            GetLocation();
            //PointList = new ObservableCollection<PointOfCustomRoute>();
            MyMapVM = new MyMapViewModel();
            DataContext = MyMapVM;

        }

        private async void ShowRouteOnMap()
        {
            
            // Start at Microsoft in Redmond, Washington.
            BasicGeoposition startLocation = new BasicGeoposition() { Latitude = 47.643, Longitude = -122.131 };

            // End at the city of Seattle, Washington.
            BasicGeoposition endLocation = new BasicGeoposition() { Latitude = 47.604, Longitude = -122.429 };
            

            // Get the route between the points.
            MapRouteFinderResult routeResult =
                  await MapRouteFinder.GetWalkingRouteAsync(
                  new Geopoint(startLocation),
                  new Geopoint(endLocation) );

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                // Use the route to initialize a MapRouteView.
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.
                myMap.Routes.Add(viewOfRoute);

                // Fit the MapControl to the route.
                await myMap.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null,
                      Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
            }
        }
        private async void Show()
        {
            if (myMap.IsStreetsideSupported)
            {
                // Find a panorama near Avenue Gustave Eiffel.
                BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 48.858, Longitude = 2.295 };
                Geopoint cityCenter = new Geopoint(cityPosition);
                StreetsidePanorama panoramaNearCity = await StreetsidePanorama.FindNearbyAsync(cityCenter);

                // Set the Streetside view if a panorama exists.
                if (panoramaNearCity != null)
                {
                    // Create the Streetside view.
                    StreetsideExperience ssView = new StreetsideExperience(panoramaNearCity);
                    ssView.OverviewMapVisible = true;
                    myMap.CustomExperience = ssView;
                }
            }
            else
            {
                // If Streetside is not supported
                ContentDialog viewNotSupportedDialog = new ContentDialog()
                {
                    Title = "Streetside is not supported",
                    Content = "\nStreetside views are not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await viewNotSupportedDialog.ShowAsync();
            }
        }

        private async void GetLocation()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    Debug.WriteLine("Waiting for update...");

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 20 };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                   // _geolocator.StatusChanged += OnStatusChanged;

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    
                    Debug.WriteLine("Location updated.");
                    myMap.Center = pos.Coordinate.Point;
                    await myMap.TrySetViewAsync(pos.Coordinate.Point, 16, myMap.Heading, myMap.DesiredPitch, MapAnimationKind.Linear);
                    MapIcon myPosition = new MapIcon();
                    myPosition.Location = myMap.Center;
                    myPosition.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    myPosition.Title = "Я здесь";
                    myPosition.Image = _mapIconStreamReference;
                    myPosition.ZIndex = 0;
                    
                    if (myMap.MapElements.Contains(_lastPosition))
                    {
                        myMap.MapElements.Remove(_lastPosition);
                    }
                    myMap.MapElements.Add(myPosition);
                    _lastPosition = myPosition;

                    break;
                case GeolocationAccessStatus.Denied:
                    Debug.WriteLine("Access to location is denied.");
                    break;
                case GeolocationAccessStatus.Unspecified:
                    Debug.WriteLine("Unspecified error.");
                    break;
            }
        }


        private void HamdurgerButton_Click(object sender, RoutedEventArgs e)
        {
            SplitPanelRefresh(MainSplitView.IsPaneOpen);
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void SplitPanelRefresh(bool isOpen)
        {
            if (isOpen)
            {
                btnDarkLightMode.Width = btnMapMode.Width = btnGetLocation.Width = btnNewRoute.Width = btnDeleteRoute.Width = 40;
                btnDarkLightMode.HorizontalAlignment =
                    btnMapMode.HorizontalAlignment = btnGetLocation.HorizontalAlignment = btnNewRoute.HorizontalAlignment = btnDeleteRoute.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                btnDarkLightMode.Width = btnMapMode.Width = btnGetLocation.Width = btnNewRoute.Width = btnDeleteRoute.Width = 150;
                btnDarkLightMode.HorizontalAlignment =
                    btnMapMode.HorizontalAlignment = btnGetLocation.HorizontalAlignment = btnNewRoute.HorizontalAlignment = btnDeleteRoute.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
        private void btnDarkLightMode_Click(object sender, RoutedEventArgs e)
        {
            if (myMap.ColorScheme == MapColorScheme.Light)
            {
                myMap.ColorScheme = MapColorScheme.Dark;
                btnDarkLightMode.Style = (Style)Resources.FirstOrDefault(dt => String.Equals(dt.Key, "btnLight")).Value;
            } else if (myMap.ColorScheme == MapColorScheme.Dark)
            {
                myMap.ColorScheme = MapColorScheme.Light;
                btnDarkLightMode.Style = (Style)Resources.FirstOrDefault(dt => String.Equals(dt.Key, "btnDark")).Value;
            }
        }

        private void btnMapMode_Click(object sender, RoutedEventArgs e)
        {
            switch (myMap.Style)
            {
                case MapStyle.Aerial:
                    myMap.Style = MapStyle.Road;
                    break;
                case MapStyle.Road:
                    myMap.Style = MapStyle.Terrain;
                    break;
                case MapStyle.Terrain:
                    myMap.Style = MapStyle.AerialWithRoads;
                    break;
                case MapStyle.AerialWithRoads:
                    myMap.Style = MapStyle.Aerial;
                    break;
            }
        }

        private void btnGetLocation_Click(object sender, RoutedEventArgs e)
        {
            GetLocation();
        }

        private void MainSplitView_PaneClosed(SplitView sender, object args)
        {
            SplitPanelRefresh(true);
        }
        
        private void btnNewRoute_Click(object sender, RoutedEventArgs e)
        {
            //PointList = new ObservableCollection<PointOfCustomRoute>();
            MyMapVM.IsRouting = !MyMapVM.IsRouting;
        }



        private void myMap_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            if (MyMapVM.IsRouting)
            {
                Debug.WriteLine(String.Format(args.Location.Position.Latitude + " " + args.Location.Position.Longitude));
                PointOfCustomRoute currPoint = new PointOfCustomRoute()
                {
                    Location = args.Location,
                    NormalizedAnchorPoint = new Point(0.5, 0.93)
                };
                if (MyMapVM.PointList.Count != 0)
                {
                    currPoint.CurrentLenght = MyMapVM.PointList.Last().CurrentLenght +
                                              PointOfCustomRoute.GetDistance(MyMapVM.PointList.Last(), currPoint);
                }
                MyMapVM.PointList.Add(currPoint);

                DrawPolyline();
            }
        }

        private void DrawPolyline()
        {
            MapPolyline polyline = new MapPolyline();
            polyline.StrokeColor = Colors.Navy;
            polyline.StrokeThickness = 3;
            List<BasicGeoposition> geopositions = new List<BasicGeoposition>();
            foreach (PointOfCustomRoute pointOfCustomRoute in MyMapVM.PointList)
            {
                geopositions.Add(pointOfCustomRoute.Location.Position);
            }

            polyline.Path = new Geopath(geopositions);
            myMap.MapElements.Add(polyline);
            if (myMap.MapElements.Contains(_lastPolyline))
            {
                myMap.MapElements.Remove(_lastPolyline);
            }
            
            _lastPolyline = polyline;
            
        }

        private void mapItemButton_Click(object sender, RoutedEventArgs e)
        {
            var mapItemButton = sender as Button;
            PointOfCustomRoute currMapItem = mapItemButton?.DataContext as PointOfCustomRoute;
            if (currMapItem != null)
            {
                int ind = MyMapVM.PointList.IndexOf(currMapItem);
                MyMapVM.PointList.RemoveAt(ind);
                if (ind == 0)
                {
                    myMap.MapElements.Remove(_lastPolyline);
                    MyMapVM.PointList.Clear();
                    MyMapVM.IsRouting = false;
                    return;
                }
                DrawPolyline();
                if (ind != MyMapVM.PointList.Count)
                {
                    double delta = MyMapVM.PointList[ind].CurrentLenght - MyMapVM.PointList[ind - 1].CurrentLenght -
                                   PointOfCustomRoute.GetDistance(MyMapVM.PointList[ind], MyMapVM.PointList[ind - 1]);
                    for (int i = ind; i < MyMapVM.PointList.Count; i++)
                    {
                        MyMapVM.PointList[i].CurrentLenght -= delta;
                    }
                }
            }
        }

        private void btnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            myMap.MapElements.Remove(_lastPolyline);
            MyMapVM.PointList.Clear();
            MyMapVM.IsRouting = false;
        }

        
    }
}
