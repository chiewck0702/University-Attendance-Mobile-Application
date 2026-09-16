using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanQrPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public ScanQrPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        //async void restartScanner()
        //{
        //    QRscanner.IsAnalyzing = false;
        //    QRscanner.IsScanning = false;

        //    await Task.Delay(500);

        //    QRscanner.IsAnalyzing = true;
        //    QRscanner.IsScanning = true;
        //}

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    //restartScanner();
        //}

        protected override void OnDisappearing()
        {
            QRscanner.IsScanning = false;
            QRscanner.IsAnalyzing = false;
            base.OnDisappearing();
        }

        void displayLoading(bool enable)
        {
            loadingIndicator.IsVisible = enable;
            loadingIndicator.IsRunning = enable;
        }

        private void QRscanner_OnScanResult(ZXing.Result result)
        {
             Device.BeginInvokeOnMainThread(async () => // this is needed to bring it to main thread because camera is working on background thread
             {
                 QRscanner.IsScanning = false;

                 displayLoading(true);

                 string scannedCode = result.Text;

                 var deviceInfo = DependencyService.Get<IDeviceInfo>();
                 string androidId = deviceInfo.GetDeviceId();
                 string deviceName = deviceInfo.GetDeviceModel();

                 string location = await GetLocationAndNavigateAsync();
                 if (string.IsNullOrEmpty(location))
                 {
                     //restartScanner();
                     //await Task.Delay(500);
                     displayLoading(false);
                     App.Current.MainPage = new NavigationPage(new ScanQrPage());
                     return;
                 }

                 var validDevice = await ApiService.ValidateStudentDevice(App.User.MatricNo, androidId, deviceName);

                 if (validDevice.Status == "Valid")
                 {
                     // verify from server
                     var attendenceRecord = new AddAttendanceRecord
                     {
                         MatricNo = App.User.MatricNo,
                         ClassId = App.User.ClassID,
                         Date = DateTime.Now,
                         Location = location,
                         QRCode = scannedCode
                     };

                     //Console.WriteLine(attendenceRecord.MatricNo);
                     //Console.WriteLine(attendenceRecord.ClassId);
                     //Console.WriteLine(attendenceRecord.Date);
                     //Console.WriteLine(attendenceRecord.Location);
                     //Console.WriteLine(attendenceRecord.QRCode);

                     var res = await ApiService.AddAttendance(attendenceRecord);

                     //if (res.Message == null)
                     //{
                     //    await DisplayAlert("Debug", "Message is NULL!", "Ok");
                     //}
                     //if (res.Status == null)
                     //{
                     //    await DisplayAlert("Debug", "Status also is NULL!", "Ok");
                     //}
                     //else
                     //{
                     //    await DisplayAlert("Debug", res.Status, "Ok");
                     //}


                     if (res.Status == "success")
                     {
                         await DisplayAlert("Success", "Successful check in attendance.", "Ok");
                         displayLoading(false);
                         App.Current.MainPage = new NavigationPage(new ViewAttendancePage());
                     }
                     else
                     {
                         await DisplayAlert("Alert", res.Message, "Ok");
                         //restartScanner();
                         displayLoading(false);
                         App.Current.MainPage = new NavigationPage(new ScanQrPage());
                     }
                 }
                 else
                 {
                     await DisplayAlert("Alert", validDevice.Message, "Ok");
                     //restartScanner();
                     displayLoading(false);
                     App.Current.MainPage = new NavigationPage(new ScanQrPage());
                 }

                 //await Navigation.PopAsync();
             });
        }

        async Task<string> GetLocationAndNavigateAsync()
        {
            try
            {
                // 1️. request permission
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Location permission is required. Please allow location and try again.", "OK");
                    return "";
                }

                // 2️. get current location
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    // 3️. produce address (Reverse Geocoding)
                    var placemarks = await Geocoding.GetPlacemarksAsync(location);
                    var placemark = (placemarks != null) ? placemarks.FirstOrDefault() : null;

                    string fullAddress = "";

                    if (placemark.FeatureName != null)
                    {
                        fullAddress += placemark.FeatureName + ", ";
                    }
                    if (placemark.SubThoroughfare != null)
                    {
                        fullAddress += placemark.SubThoroughfare + ", ";
                    }
                    if (placemark.Thoroughfare != null)
                    {
                        fullAddress += placemark.Thoroughfare + ", ";
                    }
                    if (placemark.PostalCode != null)
                    {
                        fullAddress += placemark.PostalCode + ", ";
                    }
                    if (placemark.SubLocality != null)
                    {
                        fullAddress += placemark.SubLocality + ", ";
                    }
                    if (placemark.Locality != null)
                    {
                        fullAddress += placemark.Locality + ", ";
                    }
                    if (placemark.SubAdminArea != null)
                    {
                        fullAddress += placemark.SubAdminArea + ", ";
                    }
                    if (placemark.AdminArea != null)
                    {
                        fullAddress += placemark.AdminArea + ", ";
                    }
                    if (placemark.CountryName != null)
                    {
                        fullAddress += placemark.CountryName;
                    }

                    return fullAddress;
                }
                else
                {
                    await DisplayAlert("Error", "Unable to detect location.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            return "";
        }
    }
}