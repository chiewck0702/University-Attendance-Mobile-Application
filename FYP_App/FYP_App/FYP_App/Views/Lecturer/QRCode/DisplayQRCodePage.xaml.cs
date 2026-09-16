using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DisplayQRCodePage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public GenerateQRCodeInfo GenerateQRCodeInfo { get; set; }
        public int QrSessionId { get; set; }
        public DisplayQRCodePage(GenerateQRCodeInfo GenerateQRCodeInfo, int qrSessionId)
        {
            InitializeComponent();

            ApiService = new ApiService();

            this.GenerateQRCodeInfo = GenerateQRCodeInfo;

            QrSessionId = qrSessionId;

            dateIcon.Source = ImageSource.FromResource("FYP_App.Image.Date icon.png");

            todayDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            displayInfo();
        }

        async void displayInfo()
        {
            if (QrSessionId != 0)
            {
                var res = await ApiService.RetrieveExistingQrCode(QrSessionId);
                if (res != null) 
                {
                    GenerateQRCodeInfo = res;
                }
            }
            subjectCodeLabel.Text = GenerateQRCodeInfo.SubjectId;
            subjectNameLabel.Text = GenerateQRCodeInfo.SubjectName;
            sessionLabel.Text = GenerateQRCodeInfo.SessionType;
            classLabel.Text = GenerateQRCodeInfo.ClassName;
            startTimeLabel.Text = GenerateQRCodeInfo.CreatedTime.ToString("h.mm tt");
            endTimeLabel.Text = GenerateQRCodeInfo.ExpiryTime.ToString("h.mm tt");

            ZXingQrImage.BarcodeValue = GenerateQRCodeInfo.QRCode;
        }

        private async void ButtonDownloadPNG_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Generate QR code bitmap from the BarcodeValue
                string qrCodeValue = ZXingQrImage.BarcodeValue;

                // Check Android version
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 10)
                {
                    // Android 9 and below: request StorageWrite permission
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Denied",
                            "Cannot save file without storage permission.", "OK");
                        return;
                    }
                }

                // Save to Pictures folder
                string fileName = $"QRCode_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var qrSaver = DependencyService.Get<ISaveFileInPhone>();

                bool success = await qrSaver.SaveQRCode(qrCodeValue, fileName);

                if (success)
                {
                    await DisplayAlert("Success", $"QR Code saved to Pictures/{fileName}", "Ok");
                }
                else
                {
                    await DisplayAlert("Alert", "Failed to save QR Code", "Ok");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", $"Failed to save: {ex.Message}", "Ok");
            }
        }

        private void TapViewAttendedStudent_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new ManageStudentAttendancePage(GenerateQRCodeInfo.SubjectId));
        }
    }
}