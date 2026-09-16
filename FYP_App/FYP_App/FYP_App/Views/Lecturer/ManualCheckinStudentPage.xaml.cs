using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualCheckinStudentPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public CurrentActiveSession CurrentActiveSession { get; set; }
        public string SelectedSubjectId { get; set; }
        public string SelectedSubjectName { get; set; }
        public ManualCheckinStudentPage(CurrentActiveSession currentActiveSession, string selectedSubjectId, string selectedSubjectName)
        {
            InitializeComponent();

            ApiService = new ApiService();

            CurrentActiveSession = currentActiveSession;
            SelectedSubjectId = selectedSubjectId;
            SelectedSubjectName = selectedSubjectName;

            dateIcon.Source = ImageSource.FromResource("FYP_App.Image.Date icon.png");

            todayDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            displayInfo();

            buttonConfirm.IsEnabled = false;
        }

        void displayInfo()
        {
            subjectCodeLabel.Text = SelectedSubjectId;
            subjectNameLabel.Text = SelectedSubjectName;
            sessionLabel.Text = CurrentActiveSession.SessionType;
            startTimeLabel.Text = CurrentActiveSession.CreatedTime;
            endTimeLabel.Text = CurrentActiveSession.ExpiryTime;
        }

        public string studentId { get; set; }
        public string studentName { get; set; }
        public int studentClassId { get; set; }

        private async void ButtonSearch_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(studentIdEntry.Text))
            {
                await DisplayAlert("Alert", "Student ID field cannot be empty.", "Ok");
                return;
            }

            if (studentIdEntry.Text.Length != 10)
            {
                await DisplayAlert("Alert", "Student ID length is incorrect. Please check again.", "Ok");
                return;
            }

            var studentInfo = await ApiService.RetrieveStudentName(studentIdEntry.Text);

            if (studentInfo.Status == "success")
            {
                studentId = studentIdEntry.Text;
                studentName = studentInfo.Name;
                studentClassId = studentInfo.ClassId;
                studentNameLabel.Text = studentInfo.Name;
                buttonConfirm.IsEnabled = true;
            }
            else
            {
                await DisplayAlert("Alert", studentInfo.Message, "Ok");
                return;
            }
        }

        private async void ButtonConfirm_Clicked(object sender, EventArgs e)
        {
            var addAttendanceRecord = new AddAttendanceRecord
            {
                SubjectId = SelectedSubjectId,
                MatricNo = studentId,
                Date = DateTime.Now,
                Location = "Manual check in", 
                ClassId = studentClassId,
                QRSessionID = CurrentActiveSession.QrSessionId
            };

            var status = await ApiService.AddAttendance(addAttendanceRecord);

            if (status.Status == "success")
            {
                await DisplayAlert("Success", $"Successful Manual check in student! \nStudent name: {studentName}", "Ok");
                App.Current.MainPage = new NavigationPage(new ManageStudentAttendancePage(SelectedSubjectId));
            }
            else
            {
                await DisplayAlert("Alert", status.Message, "Ok");
                return;
            }
        }
    }
}