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
    public partial class ViewAttendanceDetailPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        //public List<StudentAttendanceGraph> StudentAttendanceGraph { get; set; } = new List<StudentAttendanceGraph>();
        public ViewAttendanceDetailPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            //StudentAttendanceGraph = studentAttendanceGraphs;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            displayInfo();
        }

        async void displayInfo()
        {
            var res = await ApiService.RetrieveStudentAttendanceGraph(App.User.MatricNo, App.User.ClassID);
            if (res != null)
            {
                subjectPicker.ItemsSource = res;
                subjectPicker.ItemDisplayBinding = new Binding("SubjectId");
            }
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        private void TapTodayButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new ViewAttendancePage());
        }

        private void subjectPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subjectPicker.SelectedItem is StudentAttendanceGraph studentAttendanceGraph)
            {
                subjectName.Text = studentAttendanceGraph.SubjectName;
                attendanceRateLabel.Text = studentAttendanceGraph.attendanceRate.ToString() + '%';

                getAttendanceDetail(studentAttendanceGraph.SubjectId, studentAttendanceGraph.SemesterId);
            }
        }

        async void getAttendanceDetail(string subjectId, int semesterId)
        {
            var res = await ApiService.RetrieveStudentAttendanceDetail(App.User.MatricNo, App.User.ClassID, subjectId, semesterId);
            if (res != null)
            {
                attendanceDetailListView.ItemsSource = res;
            }
            else
            {
                attendanceDetailListView.ItemsSource = null;
            }
        }
    }
}