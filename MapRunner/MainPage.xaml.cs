using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace MapRunner
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            // Check if Streetside is supported.
            //Show();
            //ShowRouteOnMap();
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

        private void HamdurgerButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainSplitView.IsPaneOpen)
            {
                MainSplitView.IsPaneOpen = false;
                btnDarkLightMode.Width = 40;
                btnDarkLightMode.HorizontalAlignment = HorizontalAlignment.Left;
                btnMapMode.Width = 40;
                btnMapMode.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                MainSplitView.IsPaneOpen = true;
                btnDarkLightMode.Width = 150;
                btnDarkLightMode.HorizontalAlignment = HorizontalAlignment.Stretch;
                btnMapMode.Width = 150;
                btnMapMode.HorizontalAlignment = HorizontalAlignment.Stretch;
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
                    myMap.Style = MapStyle.Aerial;
                    break;
            }
        }
    }
}
