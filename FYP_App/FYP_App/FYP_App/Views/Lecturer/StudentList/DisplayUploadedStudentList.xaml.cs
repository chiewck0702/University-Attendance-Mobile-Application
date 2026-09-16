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
using System.IO;

namespace FYP_App.Views.StudentList
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DisplayUploadedStudentList : ContentPage
    {
        public ApiService ApiService { get; set; }
        public Report_SubjectList SubjectInfo { get; set; }
        public List<StudentCheckResult> PrimaryUploadedStudentList { get; set; }
        public DisplayUploadedStudentList(Report_SubjectList subjectInfo, List<StudentCheckResult> studentList)
        {
            InitializeComponent();

            ApiService = new ApiService();

            SubjectInfo = subjectInfo;
            PrimaryUploadedStudentList = studentList;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            arrowUpIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow up.png");

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UploadStudentListPage());
        }

        void displayInfo()
        {
            subjectIdLabel.Text = SubjectInfo.SubjectId;
            subjectNameLabel.Text = SubjectInfo.SubjectName;

            studentListView.ItemsSource = PrimaryUploadedStudentList;
        }

        private async void TapChooseAnotherFile_Tapped(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "A student list already exists. Choose an action: ",   // title
                "Cancel",             
                null,
                "Append (Add new students)",
                "Replace (Overwrite existing list)");

            if (action == "Cancel")
            {
                return;
            }

            //var confirm = await DisplayAlert("Info", "Only .csv file is accepted. \nReminder: Make sure the csv file only contains 1 column with header name: StudentId", "Ok", "Cancel");
            var confirm = await DisplayAlert("Info", "Only .csv file is accepted. \nReminder: Make sure the csv file only contains 3 column with header name: 'StudentId', 'StudentName', 'Class'", "Ok", "Cancel");

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
                if (!filePath.EndsWith(".csv"))
                {
                    await DisplayAlert("Error", "Please select a CSV file", "OK");
                    return;
                }

                // Call API upload
                var res = await ApiService.UploadCSVFile(filePath);

                if (res != null)
                {
                    if (action == "Replace (Overwrite existing list)")
                    {
                        PrimaryUploadedStudentList = res;
                    }
                    else
                    {
                        PrimaryUploadedStudentList.AddRange(res);
                    }
                }

                updateStudentListView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void OneTapDeleteInvalidStudent_Tapped(object sender, EventArgs e)
        {
            PrimaryUploadedStudentList.RemoveAll(x => x.IsExist == false);
            updateStudentListView();
        }

        private async void TapDeleteStudent_Tapped(object sender, EventArgs e)
        {
            var label = sender as Frame;
            var student = label.BindingContext as StudentCheckResult;

            if (student.IsExist)
            {
                var confirm = await DisplayAlert("Alert", "This student is exist in the system. Are you sure you want to remove this student ?", "Yes", "No");

                if (!confirm)
                {
                    return;
                }
            }

            PrimaryUploadedStudentList.Remove(student);

            updateStudentListView();
        }

        void updateStudentListView()
        {
            for (int i = 0; i < PrimaryUploadedStudentList.Count; i++)
            {
                PrimaryUploadedStudentList[i].Counter = i + 1;
            }

            studentListView.ItemsSource = null;
            studentListView.ItemsSource = PrimaryUploadedStudentList;
        }

        private async void TapConfirm_Tapped(object sender, EventArgs e)
        {
            if (PrimaryUploadedStudentList.Any(x => x.IsExist == false))
            {
                await DisplayAlert("Alert", "Please remove student that is not exist in the system first.", "Ok");
                return;
            }

            var enrolmentInfo = new EnrolmentInfo
            {
                SubjectId = SubjectInfo.SubjectId,
                StudentIdList = PrimaryUploadedStudentList.Select(x => x.StudentId).ToList()
            };

            var res = await ApiService.AddEnrolment(enrolmentInfo);

            if (res.Status == "success")
            {
                await DisplayAlert("Success", "Success upload student list", "Ok");

                App.Current.MainPage = new NavigationPage(new UploadStudentListPage());
            }
            else
            {
                await DisplayAlert("Alert", res.Message, "Ok");
                return;
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