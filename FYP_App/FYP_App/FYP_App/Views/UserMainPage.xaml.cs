using FYP_App.Models;
using FYP_App.Service;
using Microcharts.Forms;
using SkiaSharp;
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
    public partial class UserMainPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public UserMainPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            utemLogo.Source = ImageSource.FromResource("FYP_App.Image.UTEM logo 1.png");

            profileLogo.Source = ImageSource.FromResource("FYP_App.Image.Profile icon.png");

            arrowUpIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow up.png");

            Menu.IsVisible = false;

            displayUserDetails();
        }

        public bool MenuVisibility = false;
        private void TapProfileLogo_Tapped(object sender, EventArgs e)
        {
            if (MenuVisibility)
            {
                // currently is visible 
                Menu.IsVisible = false;
                MenuVisibility = false;
            }
            else
            {
                // currently is not visible 
                Menu.IsVisible = true;
                MenuVisibility = true;
            }
        }

        private async void ButtonLogout_Clicked(object sender, EventArgs e)
        {
            var confirmLogout = await DisplayAlert("Alert", "Are you sure you want to logout ?", "Yes", "No");

            if (confirmLogout)
            {
                App.Current.MainPage = new NavigationPage(new MainPage());
            }
        }

        //public List<StudentAttendanceGraph> StudentAttendanceGraph { get; set; } = new List<StudentAttendanceGraph>();
        async void displayUserDetails()
        {
            userName.Text = App.User.Name.Trim();
            if (App.User.Role == "Student")
            {
                userId.Text = App.User.MatricNo.Trim();
                userFaculty.Text = App.User.Faculty;
                userDetail.HeightRequest = 200;

                studentButtonMenu.IsVisible = true;
                scanQr.Source = ImageSource.FromResource("FYP_App.Image.Scan QR.png");
                viewAttendance.Source = ImageSource.FromResource("FYP_App.Image.Attendance.png");
                lecturerButtonMenu.IsVisible = false;
                //lecturerButtonMenu.HeightRequest = 0;
                //lecturerButtonMenu.Margin = new Thickness(0);

                adminButtonMenu.IsVisible = false;
                //adminButtonMenu.HeightRequest = 0;
                //adminButtonMenu.Margin = new Thickness(0);

                var attendanceGraph = await ApiService.RetrieveStudentAttendanceGraph(App.User.MatricNo, App.User.ClassID);
                if (attendanceGraph != null) 
                {
                    attendanceList.ItemsSource = attendanceGraph;
                    //StudentAttendanceGraph = attendanceGraph;
                }
            }
            else
            {
                userId.Text = App.User.StaffID.Trim();
                userFaculty.Text = "";

                if (App.User.Role == "Lecturer")
                {
                    //studentButtonMenu.HeightRequest = 0;
                    lecturerButtonMenu.IsVisible = true;
                    lecturerButtonMenu.Margin = new Thickness(40);
                    studentAttendance.Source = ImageSource.FromResource("FYP_App.Image.Student Attendance.png");
                    leaveProof.Source = ImageSource.FromResource("FYP_App.Image.Leave Proof.png");
                    studentList.Source = ImageSource.FromResource("FYP_App.Image.Student list.png");
                    reportIcon.Source = ImageSource.FromResource("FYP_App.Image.Report.png");
                    
                    studentButtonMenu.IsVisible = false;
                    //studentButtonMenu.HeightRequest = 0;
                    //studentButtonMenu.Margin = new Thickness(0);

                    adminButtonMenu.IsVisible = false;
                    //adminButtonMenu.HeightRequest = 0;
                    //adminButtonMenu.Margin = new Thickness(0);
                }
                else
                {
                    //studentButtonMenu.HeightRequest = 0;
                    //lecturerButtonMenu.HeightRequest = 0;
                    adminButtonMenu.IsVisible = true;
                    adminButtonMenu.Margin = new Thickness(40);
                    manageSubject.Source = ImageSource.FromResource("FYP_App.Image.Subject.png");
                    reportIcon2.Source = ImageSource.FromResource("FYP_App.Image.Report.png");
                    studentButtonMenu.IsVisible = false;
                    //studentButtonMenu.HeightRequest = 0;
                    //studentButtonMenu.Margin = new Thickness(0);

                    lecturerButtonMenu.IsVisible = false;
                    //lecturerButtonMenu.HeightRequest = 0;
                    //lecturerButtonMenu.Margin = new Thickness(0);

                    //studentButtonMenu.Parent = null;
                    //lecturerButtonMenu.Parent = null;
                    //var parent = adminButtonMenu.Parent as Grid;
                    //if (parent != null)
                    //{
                    //    parent.Children.Remove(studentButtonMenu);
                    //    parent.Children.Remove(lecturerButtonMenu);
                    //}
                }
            }
        }

        private async void TapScanQrCode_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScanQrPage());
        }
        private async void TapViewAttendance_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ViewAttendancePage());
        }


        private async void TapStudentAttendance_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageStudentAttendancePage(""));
        }

        private async void TapLeaveProof_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UploadStudentLeaveProofPage());
        }

        private async void TapStudentList_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UploadStudentListPage());
        }

        private async void TapReport_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportPage());
        }

        private async void TapManageSubject_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageSubjectPage());
        }

        private void AttendanceList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            var item = e.Item as StudentAttendanceGraph;
            if (item == null) 
                return;

            // Find the cell
            var listView = sender as ListView;
            var cell = listView?.TemplatedItems?.FirstOrDefault(x => x.BindingContext == item) as ViewCell;
            if (cell == null) 
                return;

            var chartView = cell.FindByName<Microcharts.Forms.ChartView>("chartView");
            var percentageLabel = cell.FindByName<Label>("percentageLabel");

            if (chartView == null || percentageLabel == null) 
                return;

            double attendanceRate = item.attendanceRate;
            float percent = (float)attendanceRate;

            percentageLabel.Text = $"{percent:F1}%";

            var entries = new[]
            {
                new Microcharts.ChartEntry(percent)
                {
                    Color = percent >= 80.00 ? SKColor.Parse("#37D13F") : SKColor.Parse("#FF0E00"),
                    ValueLabel = ""
                },
                new Microcharts.ChartEntry(100 - percent) // The remaining unfilled part
                {
                    Color = SKColor.Parse("#E0E0E0"), // Light gray for unfilled portion
                    ValueLabel = ""
                }
            };

            chartView.Chart = new Microcharts.DonutChart()
            {
                Entries = entries,
                HoleRadius = 0.7f,
                BackgroundColor = SKColors.Transparent,
                MaxValue = 100
            };
        }

        private async void TapArrowUp_Tapped(object sender, EventArgs e)
        {
            await contentScrollview.ScrollToAsync(0, 0, false);
        }

        private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (e.ScrollY > 50)
            {
                arrowUpIcon.IsVisible = true;
            }
            else
            {
                arrowUpIcon.IsVisible = false;
            }
        }
    }
}