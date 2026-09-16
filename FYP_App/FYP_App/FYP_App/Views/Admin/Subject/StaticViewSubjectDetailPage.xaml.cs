using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App.Views.Subject
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StaticViewSubjectDetailPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
        public StaticViewSubjectDetailPage(string subjectId, string subjectName)
        {
            InitializeComponent();

            ApiService = new ApiService();

            SubjectName = subjectName;

            SubjectId = subjectId;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new ManageSubjectPage());
        }

        async void displayInfo()
        {
            subjectLabel.Text = SubjectId;
            subjectNameLabel.Text = SubjectName;

            var res = await ApiService.RetrieveSubjectDetail(SubjectId);
            if (res != null)
            {
                facultyLabel.Text = res.FacultyName;
                courseLabel.Text = res.CourseId;
                studentYearLabel.Text = res.StudentYear.ToString();
                activeSemesterLabel.Text = res.SemesterId.ToString();
            }

            var res2 = await ApiService.RetrieveClassList(res.CourseId, res.StudentYear, SubjectId);
            if (res2 != null)
            {
                classListView.ItemsSource = res2;
            }

            var teachinglist = res.teachingList;

            var lecturerList = teachinglist.GroupBy(x => x.LecturerName).Select(x => x.First().LecturerName).ToList();

            var lecturerNum = teachinglist.GroupBy(x => x.LecturerName).Count();

            if (lecturerNum == 1)
            {
                singleLecturerRB.IsChecked = true;
                singleLecturer.IsVisible = true;
                multipleLecturer.IsVisible = false;
                lecturerNameLabel.Text = lecturerList[0];

                var classList = res2.Where(x => x.IsSelected).Select(x => x.ClassName).ToList();
                string classNameList = string.Join(",", classList);
                singleLecturerClassLabel.Text = classNameList;
            }
            else
            {
                multipleLecturerRB.IsChecked = true;
                singleLecturer.IsVisible = false;
                multipleLecturer.IsVisible = true;

                lecturerNumberLabel.Text = lecturerNum.ToString();

                lecturerNameLabel1.Text = lecturerList[0];
                lecturerNameLabel2.Text = lecturerList[1];

                if (lecturerNum == 2)
                {
                    labelLecturerNameEntry3.IsVisible = false;
                    lecturerNameLabel3.IsVisible = false;
                }
                else
                {
                    labelLecturerNameEntry3.IsVisible = true;
                    lecturerNameLabel3.IsVisible = true;
                    lecturerNameLabel3.Text = lecturerList[2];
                }

                var classSessionAssignment = new List<Class_SessionAssignment>();

                var classGroup = teachinglist.GroupBy(x => x.ClassName);

                foreach (var classes in classGroup)
                {
                    var classItems = classes.ToList();
                    var firstItem = classItems.First();

                    var assignment = new Class_SessionAssignment
                    {
                        ClassId = firstItem.ClassId,
                        ClassName = firstItem.ClassName
                    };

                    if (classItems.Count == 1)
                    {
                        var teacher = classItems[0];

                        assignment.LecturerName1 = teacher.LecturerName;
                        assignment.isVisiblelayoutSession2 = false;

                        assignment.isCheckedBoth = true;
                    }
                    else
                    {
                        assignment.isVisiblelayoutSession2 = true;

                        var lectureTeacher = classItems.First(x => x.LectureStatus == 1);

                        assignment.LecturerName1 = lectureTeacher.LecturerName;
                        assignment.isCheckedLecture = true;

                        var labTeacher = classItems.First(x => x.LabStatus == 1);

                        assignment.LecturerName2 = labTeacher.LecturerName;
                        assignment.isCheckedLab2 = true;
                    }
                    classSessionAssignment.Add(assignment);
                }

                classSessionAssignmentListView.ItemsSource = classSessionAssignment;
                classSessionAssignmentListView.HeightRequest = classSessionAssignment.Count() * 200;
            }
        }

        private void TapEditSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new EditSubjectPage(SubjectId, SubjectName));
        }

        private async void TapDeleteSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Alert", $"Are you sure you want to DELETE Subject: {SubjectId} {SubjectName} ? \nIt will delete also attendance record for this subject.", "Yes", "No");
        
            if (confirm)
            {
                var deleteSubjectList = new DeleteSubjectList();

                var subjectList = new List<string> { SubjectId };

                deleteSubjectList.SubjectIdList = subjectList;
                var res = await ApiService.DeleteSubject(deleteSubjectList);

                if (res.Status == "success")
                {
                    await DisplayAlert("Success", $"Success delete Subject: {SubjectId} {SubjectName}", "Ok");
                    return;
                }
                else
                {
                    await DisplayAlert("Alert", res.Message, "Ok");
                    return;
                }
            }
        }
    }
}