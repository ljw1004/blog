using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Devices;
using Windows.System.UserProfile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DemoUWP_CS
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void buttonMobile_Click(object sender, RoutedEventArgs e)
        {
            await StatusBar.GetForCurrentView().HideAsync();
        }

        private async void buttonMobileDesktop_Click(object sender, RoutedEventArgs e)
        {
            label1.Text = $"Name: {await UserInformation.GetDisplayNameAsync()}";

            var imageFile = UserInformation.GetAccountPicture(AccountPictureKind.LargeImage);
            if (imageFile != null)
            {
                using (var stream = await imageFile.OpenReadAsync())
                {
                    var bi = new BitmapImage();
                    await bi.SetSourceAsync(stream);
                    image1.Source = bi;
                }
            }
        }

        private void buttonDesktop_Click(object sender, RoutedEventArgs e)
        {
            label1.Text = $"Ringer: {CallControl.GetDefault()?.HasRinger}";
        }

        private async void button10586_Click(object sender, RoutedEventArgs e)
        {
            var holo = Windows.Graphics.Holographic.HolographicSpace.CreateForCoreWindow(Windows.UI.Core.CoreWindow.GetForCurrentThread());
            var gesture = new Windows.UI.Input.Spatial.SpatialGestureRecognizer(Windows.UI.Input.Spatial.SpatialGestureSettings.Tap);
            var space = await Windows.Perception.Spatial.SpatialAnchorManager.RequestStoreAsync();
            var folder = await Windows.Storage.DownloadsFolder.CreateFileForUserAsync(null, "download.png");
            var telemetry = Windows.System.Profile.PlatformDiagnosticsAndUsageDataSettings.CanCollectDiagnostics(Windows.System.Profile.PlatformDataCollectionLevel.Enhanced);
            var hook = Windows.UI.Input.KeyboardDeliveryInterceptor.GetForCurrentView();
            var jumps = Windows.UI.StartScreen.JumpList.IsSupported();
            Windows.ApplicationModel.Store.Preview.StoreConfiguration.PurchasePromptingPolicy = null;
            var size = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().DiagonalSizeInInches;
        }
    }
}


