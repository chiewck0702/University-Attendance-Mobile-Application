using FYP_App.Models;
using FYP_App.Service;
using FYP_App.Views.Lecturer.AbsentProof;
using FYP_App.Views.StudentList;
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

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UploadStudentLeaveProofPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public UploadStudentLeaveProofPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            searchIcon.Source = ImageSource.FromResource("FYP_App.Image.Search icon.png");

            arrowUpIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow up.png");

            displayInfo();
        }


        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        async void displayInfo()
        {
            var res = await ApiService.TeachingSubjectList(App.User.StaffID);
            if (res != null)
            {
                subjectPicker.ItemsSource = res;
                subjectPicker.ItemDisplayBinding = new Binding("SubjectId");
            }
        }

        public string SelectedSubjectId { get; set; }
        public string SelectedSubjectName { get; set; }

        private async void subjectPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subjectPicker.SelectedItem is TeachSubjectList teachSubjectList)
            {
                SelectedSubjectId = teachSubjectList.SubjectId;
                SelectedSubjectName = teachSubjectList.SubjectName;
                subjectName.Text = teachSubjectList.SubjectName;

                var res = await ApiService.RetriveTeachingSubjectInfo(App.User.StaffID, teachSubjectList.SubjectId);

                var sessionList = new List<string>();
                
                if (res != null)
                {
                    if (res.LectureStatus)
                    {
                        sessionList.Add("Lecture");
                    }

                    if (res.LabStatus)
                    {
                        sessionList.Add("Lab");
                    }
                }

                sessionPicker.ItemsSource = sessionList;
                sessionPicker.IsEnabled = true;
                weekPicker.IsEnabled = false;
                datePicker.IsEnabled = false;
            }
        }

        public List<TeachingQRSessionInfo> teachingQRSessionInfos {  get; set; }

        private async void sessionPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sessionPicker.SelectedIndex != -1)
            {
                var res = await ApiService.RetriveTeachingQRSessionInfo(App.User.StaffID, SelectedSubjectId, sessionPicker.SelectedItem.ToString());

                if (res != null)
                {
                    teachingQRSessionInfos = res;

                    var weekList = teachingQRSessionInfos.GroupBy(x => x.Week).Select(x => x.First().Week).OrderBy(x => x).ToList();
                    weekPicker.ItemsSource = weekList;
                    //weekPicker.ItemDisplayBinding = new Binding("Week");
                    weekPicker.IsEnabled = true;
                }
                
                datePicker.IsEnabled = false;
            }
        }

        public int selectedWeek {  get; set; }

        private void weekPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (weekPicker.SelectedIndex != -1)
            {
                selectedWeek = (int)weekPicker.SelectedItem;
                var dateList = teachingQRSessionInfos.Where(x => x.Week == selectedWeek).GroupBy(x => x.Date).Select(x => x.First().Date).ToList();
                datePicker.ItemsSource = dateList;
                //datePicker.ItemDisplayBinding = new Binding("Date");
                datePicker.IsEnabled = true;
            }
        }

        public List<AttendanceListInfo> notAttendedStudentList { get; set; }
        public List<int> QrSessionId { get; set; }

        private async void datePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (datePicker.SelectedIndex != -1)
            {
                var selectedDate = datePicker.SelectedItem.ToString();
                QrSessionId = teachingQRSessionInfos.Where(x => x.Week == selectedWeek && x.Date == selectedDate).Select(x => x.QrSessionId).ToList();

                var notAttendStudentRequest = new NotAttendStudentRequest
                {
                    StaffId = App.User.StaffID,
                    SubjectId = SelectedSubjectId,
                    QrSessionId = QrSessionId
                };

                var res = await ApiService.RetrieveNotAttendedStudentList(notAttendStudentRequest);

                if (res != null)
                {
                    notAttendedStudentList = res;
                    nameList.ItemsSource = notAttendedStudentList;
                    nameList.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Info", $"There is no registered student that not attend class at {selectedDate}", "Ok");
                    return;
                }
            }    
        }

        private void searchEntry_Completed(object sender, EventArgs e)
        {
            nameList.ItemsSource = notAttendedStudentList.Where(x => x.StudentId.ToLower().Contains(searchEntry.Text.ToLower()) || x.StudentName.ToLower().Contains(searchEntry.Text.ToLower())).ToList();
        }

        private async void TapUploadStudentList_Tapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            var student = frame.BindingContext as AttendanceListInfo;

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

                var studentAbsentProofInfo = new StudentAbsentProofInfo
                {
                    SubjectId = SelectedSubjectId,
                    SubjectName = SelectedSubjectName,
                    SessionType = sessionPicker.SelectedItem.ToString(),
                    Date = datePicker.SelectedItem.ToString(),
                    Week = weekPicker.SelectedItem.ToString(),
                    StudentName = student.StudentName,
                    StudentId = student.StudentId,
                    FileName = fileName,
                    FilePath = filePath,
                    QrSessionId = QrSessionId.First()
                };

                App.Current.MainPage = new NavigationPage(new DisplayStudentAbsentProof(studentAbsentProofInfo));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
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

        private async void TapArrowUp_Tapped(object sender, EventArgs e)
        {
            await contentScrollview.ScrollToAsync(0, 0, false);
        }

        
    }
}