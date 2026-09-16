using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App.Views.Report
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FullAttendancePage : ContentPage
	{
        public ApiService ApiService { get; set; }
        public string SubjectId { get; set; }
        public string SubjectName { get; set; }
        public FullAttendancePage (string subjectId, string subjectName)
		{
			InitializeComponent ();

            ApiService = new ApiService();

            SubjectId = subjectId;
            SubjectName = subjectName;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            subjectLabel.Text = SubjectId + "\n" + subjectName.ToUpper();

            searchIcon.Source = ImageSource.FromResource("FYP_App.Image.Search icon.png");

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new ReportPage());
        }

        public bool isLecture = true;

        public List<TeachSubjectClassList> lectureList = new List<TeachSubjectClassList>();
        public List<TeachSubjectClassList> labList = new List<TeachSubjectClassList>();

        async void displayInfo()
        {
            var res = await ApiService.TeachSubjectLectureClassList(App.User.StaffID, App.User.Role, SubjectId);
            if (res != null)
            {
                lectureList = res;
                if (lectureList.Count > 1)
                {
                    string combineClass = "";

                    foreach (var item in lectureList)
                    {
                        combineClass += item.ClassName + ", ";
                    }

                    combineClass = combineClass.Remove(combineClass.Length - 2);

                    lectureList.Insert(0, new TeachSubjectClassList
                    {
                        ClassId = 0,
                        ClassName = combineClass
                    });
                }
                classPicker.ItemsSource = lectureList;
                classPicker.ItemDisplayBinding = new Binding("ClassName");
                
                List<string> weekNumList = new List<string>();
                for (int i = 1; i <= 15; i++)
                {
                    weekNumList.Add("Week " + i.ToString());
                }

                weekNumHeader.ItemsSource = weekNumList;
            }
        }

        public List<AttendanceListInfo> fullAttendanceListInfo = new List<AttendanceListInfo>();

        private async void classPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (classPicker.SelectedItem is TeachSubjectClassList teachSubjectClassList)
            {
                var classList = new List<TeachSubjectClassList>();
                if (classPicker.SelectedIndex == 0)
                {
                    if (isLecture)
                    {
                        classList = lectureList.GetRange(1, lectureList.Count - 1).ToList();
                    }
                }

                if (classList.Count == 0)
                {
                    if (isLecture)
                    {
                        classList.Add(lectureList[classPicker.SelectedIndex]);
                    }
                    else
                    {
                        classList.Add(labList[classPicker.SelectedIndex]);
                    }
                }

                var subjectInfo = new SubjectInfo
                {
                    SubjectId = SubjectId,
                    isLecture = isLecture,
                    ClassInfo = classList
                };

                var res = await ApiService.RetrieveFullAttendanceList(subjectInfo);
                if (res != null)
                {
                    fullAttendanceListInfo = res;
                    nameList.ItemsSource = fullAttendanceListInfo;
                }
            }
        }

        private void searchEntry_Completed(object sender, EventArgs e)
        {
            nameList.ItemsSource = fullAttendanceListInfo.Where(x => x.StudentId.ToLower().Contains(searchEntry.Text.ToLower()) || x.StudentName.ToLower().Contains(searchEntry.Text.ToLower())).ToList();
        }

        private void lectureButton_Clicked(object sender, EventArgs e)
        {
            classPicker.ItemsSource = lectureList;
            classPicker.ItemDisplayBinding = new Binding("ClassName");
            isLecture = true;
            lectureButton.TextColor = Color.Purple;
            labButton.TextColor = Color.Black;
            nameList.ItemsSource = null;
        }

        private async void labButton_Clicked(object sender, EventArgs e)
        {
            if (labList.Count == 0)
            {
                var res = await ApiService.TeachSubjectLabClassList(App.User.StaffID, App.User.Role, SubjectId);
                if (res != null)
                {
                    labList = res;
                }
            }
            classPicker.ItemsSource = labList;
            classPicker.ItemDisplayBinding = new Binding("ClassName");
            isLecture = false;
            lectureButton.TextColor = Color.Black;
            labButton.TextColor = Color.Purple;
            nameList.ItemsSource = null;
        }

        private async void ButtonDownloadNow_Clicked(object sender, EventArgs e)
        {
            if (classPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select class first.", "Ok");
                return;
            }

            var reportInfo = new DownloadFullReportInfo
            {
                SubjectId = SubjectId,
                SubjectName = SubjectName,
                SessionType = isLecture ? "Lecture" : "Lab",
                fullAttendanceListInfos = nameList.ItemsSource as List<AttendanceListInfo>
            };

            var res = await ApiService.DownloadCsv_FullAttendance_Current(reportInfo);
            if (res != null)
            {
                await DisplayAlert("Success", $"Success save the file at path: Download/{res}", "Ok");
            }
            else
            {
                await DisplayAlert("Alert", "Download report is not success", "Ok");
            }
        }

        async void downloadFunction(int weekNum)
        {
            var classList = new List<int>();
            if (classPicker.SelectedIndex == 0)
            {
                if (isLecture)
                {
                    classList = lectureList.GetRange(1, lectureList.Count - 1).Select(x => x.ClassId).ToList();
                }
            }

            if (classList.Count == 0)
            {
                if (isLecture)
                {
                    classList.Add(lectureList[classPicker.SelectedIndex].ClassId);
                }
                else
                {
                    classList.Add(labList[classPicker.SelectedIndex].ClassId);
                }
            }

            var reportInfo = new DownloadScheduleReportInfo
            {
                SubjectId = SubjectId,
                SubjectName = SubjectName,
                SessionType = isLecture ? "Lecture" : "Lab",
                ClassId = classList,
                WeekNum = weekNum
            };

            var res = await ApiService.DownloadCsv_FullAttendance_Schedule(reportInfo);
            if (res != null)
            {
                await DisplayAlert("Success", $"Success save the file at path: Download/{res}", "Ok");
            }
            else
            {
                await DisplayAlert("Alert", "Download report is not success", "Ok");
            }
        }

        private async void ButtonDownloadWeek6_Clicked_1(object sender, EventArgs e)
        {
            if (classPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select class first.", "Ok");
                return;
            }

            downloadFunction(6);
        }

        private async void ButtonDownloadWeek11_Clicked_1(object sender, EventArgs e)
        {
            if (classPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select class first.", "Ok");
                return;
            }

            downloadFunction(11);
        }
    }
}