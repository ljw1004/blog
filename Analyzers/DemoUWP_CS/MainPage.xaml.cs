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
    }
}

