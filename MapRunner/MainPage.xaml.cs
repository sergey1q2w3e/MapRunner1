﻿using System;
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
        private MapPolyline _lastRunRoute;
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
            MyMapVM = new MyMapViewModel();
            DataContext = MyMapVM;
            // TODO Почему то Fade-эффект не работает если явно указать Visibility="Collapsed" в XAML, поэтому пришлось убирать Grid здесь
            FadeOutRunningGrid.Begin();
        }

        private async void ShowRouteOnMap()
        {
            
            // Start at Microsoft in Redmond, Washington.
            BasicGeoposition startLocation = new BasicGeoposition() { Latitude = 56.31, Longitude = 44.00 };

            // End at the city of Seattle, Washington.
            BasicGeoposition endLocation = new BasicGeoposition() { Latitude = 56.00, Longitude = 44.00 };
            

            // Get the route between the points.
            MapRouteFinderResult routeResult =
                  await MapRouteFinder.GetDrivingRouteAsync(
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
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 20, ReportInterval = 2000};

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    // _geolocator.StatusChanged += OnStatusChanged;
                    geolocator.PositionChanged += OnPositionChanged;
                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    
                    Debug.WriteLine("Location updated.");
                    DrawMyPosition(pos.Coordinate.Point);
                    myMap.Center = pos.Coordinate.Point;
                    await myMap.TrySetViewAsync(pos.Coordinate.Point, 16, myMap.Heading, myMap.DesiredPitch, MapAnimationKind.Linear);

                    break;
                case GeolocationAccessStatus.Denied:
                    Debug.WriteLine("Access to location is denied.");
                    break;
                case GeolocationAccessStatus.Unspecified:
                    Debug.WriteLine("Unspecified error.");
                    break;
            }
        }

        #region Buttons
        private void HamdurgerButton_Click(object sender, RoutedEventArgs e)
        {
            SplitPanelRefresh(MainSplitView.IsPaneOpen);
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void btnDarkLightMode_Click(object sender, RoutedEventArgs e)
        {
            if (myMap.ColorScheme == MapColorScheme.Light)
            {
                myMap.ColorScheme = MapColorScheme.Dark;
                btnDarkLightMode.Style = (Style)Resources.FirstOrDefault(dt => String.Equals(dt.Key, "btnLight")).Value;
            }
            else if (myMap.ColorScheme == MapColorScheme.Dark)
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

        private void btnNewRoute_Click(object sender, RoutedEventArgs e)
        {
            MyMapVM.IsRouting = !MyMapVM.IsRouting;
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;
            FadeInRunningGrid.Begin();
            MyMapVM.StartRunning();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStart.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Collapsed;
            FadeOutRunningGrid.Begin();
            MyMapVM.StopRunning();
        }

        private void StopRunningButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyMapVM.IsRunning)
            {
                MyMapVM.PauseRunning();
            }
            else
            {
                MyMapVM.StartRunning();
            }
        }
        
        #endregion

        #region Event Handlers

        private void MainSplitView_PaneClosed(SplitView sender, object args)
        {
            SplitPanelRefresh(true);
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

        private async void OnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Debug.WriteLine(String.Format(args.Position.Coordinate.Point.Position.Latitude + " " + args.Position.Coordinate.Point.Position.Longitude));
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Geocoordinate coord = args.Position.Coordinate;
                if (!double.IsNaN((double)coord.Speed))
                    MyMapVM.CurrentSpeed = (double)coord.Speed;
                else
                    MyMapVM.CurrentSpeed = 0.0;
                MyMapVM.RunPointList.Add(coord.Point.Position);
                DrawRunRoute();
                DrawMyPosition(coord.Point);
            });
        }

        #endregion

        private void SplitPanelRefresh(bool isOpen)
        {
            if (isOpen)
            {
                 btnDarkLightMode.Width = btnMapMode.Width = btnGetLocation.Width = btnNewRoute.Width = btnDeleteRoute.Width = btnStart.Width = btnStop.Width = 40;
            }
            else
            {
                 btnDarkLightMode.Width = btnMapMode.Width = btnGetLocation.Width = btnNewRoute.Width = btnDeleteRoute.Width = btnStart.Width = btnStop.Width = 150;
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

        private void DrawRunRoute()
        {
            MapPolyline route = new MapPolyline();
            route.StrokeColor = Colors.RoyalBlue;
            route.StrokeThickness = 3;
            route.Path = new Geopath(MyMapVM.RunPointList);
            myMap.MapElements.Add(route);
            if (myMap.MapElements.Contains(_lastRunRoute))
            {
                myMap.MapElements.Remove(_lastRunRoute);
            }
            _lastRunRoute = route;
        }

        private void DrawMyPosition(Geopoint position)
        {
            MapIcon myPosition = new MapIcon();
            myPosition.Location = position;
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
        }
    }
}
