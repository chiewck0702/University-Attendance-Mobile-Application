using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace FYP_App.Views.Lecturer.AbsentProof
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DisplayStudentAbsentProof : ContentPage
    {
        public ApiService ApiService { get; set; }
        public StudentAbsentProofInfo StudentAbsentProofInfo { get; set; }
        public DisplayStudentAbsentProof(StudentAbsentProofInfo studentAbsentProofInfo)
        {
            InitializeComponent();

            ApiService = new ApiService();

            StudentAbsentProofInfo = studentAbsentProofInfo;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UploadStudentLeaveProofPage());
        }

        void displayInfo()
        {
            subjectLabel.Text = StudentAbsentProofInfo.SubjectId;
            subjectName.Text = StudentAbsentProofInfo.SubjectName;
            sessionLabel.Text = StudentAbsentProofInfo.SessionType;
            weekLabel.Text = StudentAbsentProofInfo.Week;
            dateLabel.Text = StudentAbsentProofInfo.Date;
            studentNameLabel.Text = StudentAbsentProofInfo.StudentName;
            studentIdLabel.Text = StudentAbsentProofInfo.StudentId;
            fileNameLabel.Text = StudentAbsentProofInfo.FileName;
        }

        private async void TapChooseAnotherFile_Tapped(object sender, EventArgs e)
        {
            var chooseNewFile = await DisplayAlert("Alert", "You already choose one file. Are you sure you want to choose a new file to replace ?", "Yes", "No");
            
            if (!chooseNewFile)
            {
                return;
            }

            var confirm = await DisplayAlert("Info", "Only .pdf file is accepted. ", "Ok", "Cancel");

            if (!confirm)
            {
                return;
            }

            try
            {
                var filePicker = DependencyService.Get<IAndroidFilePicker>();
                var filePath = await filePicker.PickCsvFileAsync();

                if (string.IsNullOrEmpty(filePath))
                    return;  // user cancelled

                // Optional: check extension
                if (!filePath.EndsWith(".pdf"))
                {
                    await DisplayAlert("Error", "Please select a PDF file", "OK");
                    return;
                }

                string fileName = Path.GetFileName(filePath);

                StudentAbsentProofInfo.FileName = fileName;
                StudentAbsentProofInfo.FilePath = filePath;

                fileNameLabel.Text = StudentAbsentProofInfo.FileName;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void TapConfirm_Tapped(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(reasonEntry.Text))
            {
                await DisplayAlert("Alert", "Reason field cannot be empty", "Ok");
                return;
            }    

            try
            {
                var originalPath = StudentAbsentProofInfo.FilePath;

                // New file name
                var newFileName =
                    $"AbsentProof_{StudentAbsentProofInfo.StudentId}_{StudentAbsentProofInfo.QrSessionId}.pdf";

                // New path in cache
                var newPath = Path.Combine(
                    Xamarin.Essentials.FileSystem.CacheDirectory,
                    newFileName);

                // Copy (rename) file
                File.Copy(originalPath, newPath, true);

                StudentAbsentProofInfo.FilePath = newPath;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
                return;
            }

            var res = await ApiService.UploadPDFFile(StudentAbsentProofInfo.FilePath);

            if (res.Status != "success")
            {
                await DisplayAlert("Alert", res.Message, "Ok");
                return;
            }

            var res2 = await ApiService.EditStudentAttendanceStatus(StudentAbsentProofInfo.StudentId, StudentAbsentProofInfo.QrSessionId, reasonEntry.Text);
            if (res2.Status == "success")
            {
                await DisplayAlert("Success", $"Success update Student : {StudentAbsentProofInfo.StudentName} ({StudentAbsentProofInfo.StudentId}) attendance to 'Absent with Reason'", "Ok");
                App.Current.MainPage = new NavigationPage(new UploadStudentLeaveProofPage());
            }
            else
            {
                await DisplayAlert("Alert", res2.Message, "Ok");
                return;
            }
        }
    }
}