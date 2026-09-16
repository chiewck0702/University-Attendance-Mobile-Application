using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App.Views.Subject
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EditSubjectPage : ContentPage
	{
		public ApiService ApiService { get; set; }
		public string SubjectId { get; set; }
		public string SubjectName { get; set; }
        public EditSubjectPage (string subjectId, string subjectName)
		{
			InitializeComponent ();

			ApiService = new ApiService ();

			SubjectId = subjectId;
            SubjectName = subjectName;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            displayInfo();
        }

        bool confirmationPage = false;

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            if (confirmationPage)
            {
                enablePage(true);
                confirmationPage = false;
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new ManageSubjectPage());
            }
        }

        public SubjectDetail OriginalSubjectDetail = new SubjectDetail();
        public SubjectDetail EditedSubjectDetail = new SubjectDetail();
        public List<TeachSubjectClassList> OriginalClassList = new List<TeachSubjectClassList>();
        public List<TeachSubjectClassList> EditedClassList = new List<TeachSubjectClassList>();
        bool selectedSingleLecturerOption = true;
        async Task getOriginalData()
        {
            var res = await ApiService.RetrieveSubjectDetail(SubjectId);
            if (res != null)
            {
                OriginalSubjectDetail = res;
            }
            OriginalSubjectDetail.SubjectName = SubjectName;

            EditedSubjectDetail = new SubjectDetail()
            {
                SubjectId = SubjectId,
                SubjectName = OriginalSubjectDetail.SubjectName,
                CourseId = OriginalSubjectDetail.CourseId,
                FacultyName = OriginalSubjectDetail.FacultyName,
                StudentYear = OriginalSubjectDetail.StudentYear,
                SemesterId = OriginalSubjectDetail.SemesterId,
                teachingList = new List<TeachSubjectList>()
            };
        }

        public List<LecturerList> LecturerLists = new List<LecturerList>();

        List<int> lecturerNumberList = new List<int> { 2, 3 };

        public int OriginalSelectedLecturerNum;

        private bool _isInitializing = false;

        void displayLoading(bool enable)
        {
            loadingIndicator.IsVisible = enable;
            loadingIndicator.IsRunning = enable;
        }

        async void displayInfo()
        {
            _isInitializing = true;
            displayLoading(true);
            enablePage(false);

            await getOriginalData();

            subjectEntry.Text = SubjectId;

            subjectNameEntry.Text = SubjectName;

            var res = await ApiService.RetrieveFacultyList();
            if (res != null)
            {
                facultyPicker.ItemsSource = res;
                facultyPicker.SelectedIndex = res.IndexOf(OriginalSubjectDetail.FacultyName);
            }

            var res2 = await ApiService.RetrieveCourseList(OriginalSubjectDetail.FacultyName);
            if (res2 != null)
            {
                coursePicker.ItemsSource = res2;
                coursePicker.SelectedIndex = res2.IndexOf(OriginalSubjectDetail.CourseId);
            }

            var res3 = await ApiService.RetrieveStudentYearList(OriginalSubjectDetail.CourseId);
            if (res3 != null)
            {
                studentYearPicker.ItemsSource = res3;
                studentYearPicker.SelectedIndex = res3.IndexOf(OriginalSubjectDetail.StudentYear);
            }

            var res4 = await ApiService.RetrieveSemesterList();
            if (res4 != null)
            {
                activeSemesterPicker.ItemsSource = res4;
                activeSemesterPicker.SelectedIndex = res4.IndexOf(OriginalSubjectDetail.SemesterId);
            }

            var res5 = await ApiService.RetrieveClassList(OriginalSubjectDetail.CourseId, OriginalSubjectDetail.StudentYear, SubjectId);
            if (res5 != null)
            {
                OriginalClassList = res5;
                EditedClassList = res5.Select(x => new TeachSubjectClassList
                {
                    ClassId = x.ClassId,
                    ClassName = x.ClassName,
                    IsSelected = x.IsSelected
                }).ToList();
                classListView.ItemsSource = EditedClassList;
            }

            var result = await ApiService.RetrieveLecturerList();
            if (result != null)
            {
                LecturerLists = result;
            }

            var teachinglist = OriginalSubjectDetail.teachingList;

            var lecturerList = teachinglist.GroupBy(x => x.LecturerName).Select(g => g.First()).ToList();

            OriginalSelectedLecturerNum = teachinglist.GroupBy(x => x.LecturerName).Count();

            if (OriginalSelectedLecturerNum == 1)
            {
                selectedSingleLecturerOption = true;
                singleLecturerRB.IsChecked = true;
                singleLecturer.IsVisible = true;
                multipleLecturer.IsVisible = false;
                lecturerNameEntry.Text = lecturerList[0].LecturerName;
                potentialLecturerNameList.ItemsSource = LecturerLists;
                potentialLecturerNameList.IsVisible = false;

                var classList = OriginalClassList.Where(x => x.IsSelected).Select(x => x.ClassName).ToList();
                string classNameList = string.Join(",", classList);
                singleLecturerClassLabel.Text = classNameList;
            }
            else
            {
                selectedSingleLecturerOption = false;
                multipleLecturerRB.IsChecked = true;
                singleLecturer.IsVisible = false;
                multipleLecturer.IsVisible = true;

                lecturerNumberPicker.ItemsSource = lecturerNumberList;
                lecturerNumberPicker.SelectedIndex = lecturerNumberList.IndexOf(OriginalSelectedLecturerNum);

                lecturerNameEntry1.Text = lecturerList[0].LecturerName;
                potentialLecturerNameList1.ItemsSource = LecturerLists;
                potentialLecturerNameList1.IsVisible = false;
                lecturerNameEntry2.Text = lecturerList[1].LecturerName;
                lecturerNameEntry2.IsEnabled = true;
                potentialLecturerNameList2.ItemsSource = LecturerLists.Where(x => x.LecturerId != lecturerList[0].StaffId).ToList();
                potentialLecturerNameList2.IsVisible = false;

                if (OriginalSelectedLecturerNum == 2)
                {
                    labelLecturerNameEntry3.IsVisible = false;
                    lecturerNameEntry3.IsVisible = false;
                }
                else
                {
                    labelLecturerNameEntry3.IsVisible = true;
                    lecturerNameEntry3.IsVisible = true;
                    lecturerNameEntry3.IsEnabled = true;
                    lecturerNameEntry3.Text = lecturerList[2].LecturerName;
                    potentialLecturerNameList3.ItemsSource = LecturerLists.Where(x => x.LecturerId != lecturerList[0].StaffId && x.LecturerId != lecturerList[1].StaffId).ToList();
                    potentialLecturerNameList3.IsVisible = false;
                }

                var classSessionAssignment = new List<Class_SessionAssignment>();

                var selectedLecturerList = new List<LecturerList>();

                var lecturer1 = LecturerLists.First(x => x.LecturerName == lecturerNameEntry1.Text);
                var lecturer2 = LecturerLists.First(x => x.LecturerName == lecturerNameEntry2.Text);

                selectedLecturerList.Add(lecturer1);
                selectedLecturerList.Add(lecturer2);

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

                        assignment.LecturerNameList1 = selectedLecturerList;
                        assignment.SelectedLecturer1 = selectedLecturerList.First(x => x.LecturerId == classItems[0].StaffId);
                        assignment.SelectedLecturerIndex1 = selectedLecturerList.FindIndex(x => x.LecturerId == classItems[0].StaffId);
                        assignment.isVisiblelayoutSession2 = false;

                        assignment.isCheckedBoth = true;
                    }
                    else // original got 2 lecturer teaching 
                    {
                        assignment.isCheckedBoth = false;
                        assignment.isVisiblelayoutSession2 = true;

                        var lectureTeacher = classItems.First(x => x.LectureStatus == 1);

                        assignment.LecturerNameList1 = selectedLecturerList;
                        assignment.SelectedLecturer1 = selectedLecturerList.First(x => x.LecturerId == lectureTeacher.StaffId);
                        assignment.SelectedLecturerIndex1 = selectedLecturerList.FindIndex(x => x.LecturerId == lectureTeacher.StaffId);
                        assignment.isCheckedLecture = true;

                        var labTeacher = classItems.First(x => x.LabStatus == 1);
                        assignment.SelectedLecturer2 = selectedLecturerList.First(x => x.LecturerId == labTeacher.StaffId);

                        if (OriginalSelectedLecturerNum == 2)
                        {
                            assignment.LecturerName2 = labTeacher.LecturerName;
                            assignment.isVisibleLecturerLabel = true;
                            assignment.isVisibleLecturerPicker = false;
                        }
                        else
                        {
                            assignment.LecturerNameList2 = selectedLecturerList;
                            assignment.SelectedLecturerIndex2 = selectedLecturerList.FindIndex(x => x.LecturerId == labTeacher.StaffId);
                            assignment.isVisibleLecturerLabel = false;
                            assignment.isVisibleLecturerPicker = true;
                        }
                        assignment.isCheckedLab2 = true;
                    }

                    classSessionAssignment.Add(assignment);
                }

                classSessionAssignmentListView.ItemsSource = classSessionAssignment;
                classSessionAssignmentListView.HeightRequest = classSessionAssignment.Count() * 200;
                classSessionAssignmentListView.IsVisible = true;
                //lecturerNumberPickerGrid.ForceLayout();
            }

            _isInitializing = false;
            displayLoading(false);
            enablePage(true);
        }

        void enablePage(bool enable)
        {
            subjectEntry.IsEnabled = enable;
            subjectNameEntry.IsEnabled = enable;
            facultyPicker.IsEnabled = enable;
            coursePicker.IsEnabled = enable;
            studentYearPicker.IsEnabled = enable;
            activeSemesterPicker.IsEnabled = enable;

            classListView.IsEnabled = enable;
            lecturerOptionRadioButtonGroup.IsEnabled = enable;

            if (selectedSingleLecturerOption)
            {
                lecturerNameEntry.IsEnabled = enable;
            }
            else
            {
                multipleLecturer.IsEnabled = enable;
                lecturerNumberPicker.IsEnabled = enable;
                lecturerNameEntry1.IsEnabled = enable;
                lecturerNameEntry2.IsEnabled = enable;

                if (GetEffectiveLecturerNum() == 3)
                {
                    lecturerNameEntry3.IsEnabled = enable;
                }

                var listView = classSessionAssignmentListView.ItemsSource as List<Class_SessionAssignment>;
                classSessionAssignmentListView.HeightRequest = listView.Count() * 200;

                foreach (var item in listView)
                {
                    if (!item.isCheckedBoth)
                    {
                        classSessionAssignmentListView.HeightRequest += 150;
                    }
                }

                classSessionAssignmentListView.IsEnabled = enable;
            }
        }

        private async void facultyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (facultyPicker.SelectedIndex != -1)
            {
                string facultyName = facultyPicker.SelectedItem as string;
                EditedSubjectDetail.FacultyName = facultyName;
                var res = await ApiService.RetrieveCourseList(facultyName);
                if (res != null)
                {
                    coursePicker.ItemsSource = res;
                    coursePicker.IsEnabled = true;
                }
            }
        }

        public string CourseId { get; set; }

        private async void coursePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (coursePicker.SelectedIndex != -1)
            {
                CourseId = coursePicker.SelectedItem.ToString();
                EditedSubjectDetail.CourseId = CourseId;
                var res = await ApiService.RetrieveStudentYearList(CourseId);
                if (res != null)
                {
                    studentYearPicker.ItemsSource = res;
                    studentYearPicker.IsEnabled = true;
                }
            }
        }

        //public List<TeachSubjectClassList> classLists = new List<TeachSubjectClassList>();

        private async void studentYearPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (studentYearPicker.SelectedIndex != -1)
            {
                EditedSubjectDetail.StudentYear = (int)studentYearPicker.SelectedItem;
                var res = await ApiService.RetrieveClassList(CourseId, EditedSubjectDetail.StudentYear, "");
                if (res != null)
                {
                    EditedClassList = res;
                    classListView.ItemsSource = EditedClassList;
                }
            }
        }

        bool anyCheckboxClassChecked = true;

        private void CheckBoxClass_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            var checkbox = (CheckBox)sender;

            if (checkbox.BindingContext is TeachSubjectClassList teachSubjectClass)
            {
                teachSubjectClass.IsSelected = e.Value;

                anyCheckboxClassChecked = EditedClassList.Any(c => c.IsSelected);

                if (selectedSingleLecturerOption)
                {
                    var selectedClassList = EditedClassList.Where(c => c.IsSelected).Select(x => x.ClassName).ToList();

                    string classNameList = string.Join(", ", selectedClassList);

                    singleLecturerClassLabel.Text = classNameList;
                }
                else
                {
                    if (!string.IsNullOrEmpty(lecturerNameEntry1.Text) && !string.IsNullOrEmpty(lecturerNameEntry2.Text))
                    {
                        if (GetEffectiveLecturerNum() == 2)
                        {
                            displayClassSessionAssignment();
                        }
                        else if (GetEffectiveLecturerNum() == 3)
                        {
                            if (!string.IsNullOrEmpty(lecturerNameEntry3.Text))
                            {
                                displayClassSessionAssignment();
                            }
                        }
                    }
                }
            }
        }

        private void RadioButtonLecturerOption_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (e.Value)
            {
                var rb = (RadioButton)sender;
                string selectedRB = rb.Content.ToString();
                if (selectedRB == "Single")
                {
                    singleLecturer.IsVisible = true;
                    multipleLecturer.IsVisible = false;
                    selectedSingleLecturerOption = true;
                }
                else
                {
                    singleLecturer.IsVisible = false;
                    multipleLecturer.IsVisible = true;
                    lecturerNumberPicker.ItemsSource = lecturerNumberList;
                    selectedSingleLecturerOption = false;
                }
            }
        }

        bool selectingLecturer = false;

        private void lecturerNameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (selectingLecturer)
            {
                return;
            }

            string inputText = lecturerNameEntry.Text.ToLower();

            var list = new List<LecturerList>();
            list = LecturerLists.Where(x => x.LecturerId.ToLower().Contains(inputText) || x.LecturerName.ToLower().Contains(inputText)).ToList();

            potentialLecturerNameList.ItemsSource = list;
            potentialLecturerNameList.IsVisible = true;
        }

        private void TapLecturerNameGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            selectingLecturer = true;

            var label = sender as Label;
            var lecturer = label.BindingContext as LecturerList;

            if (lecturer != null)
            {
                lecturerNameEntry.Text = lecturer.LecturerName; // set selectingLecturer = true; so that this assignment wont trigger lecturerNameEntry_TextChanged() again 
                potentialLecturerNameList.IsVisible = false;
            }

            selectingLecturer = false; // setting it back to false so that if user change the text again will trigger lecturerNameEntry_TextChanged()
        }

        int selectedLecturerNum = 0;

        private void lecturerNumberPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (lecturerNumberPicker.SelectedIndex == -1)
            {
                return;
            }

            if (lecturerNumberPicker.SelectedIndex == 0)
            {
                lecturerNameEntry1.Text = "";
                lecturerNameEntry2.Text = "";
                labelLecturerNameEntry3.IsVisible = false;
                lecturerNameEntry3.IsVisible = false;
                lecturerNameEntry2.IsEnabled = false;
                selectedLecturerNum = 2;
            }
            else
            {
                if (!string.IsNullOrEmpty(lecturerNameEntry1.Text))
                {
                    if (!(LecturerLists.FirstOrDefault(x => x.LecturerName == lecturerNameEntry1.Text) == null))
                    {
                        lecturerNameEntry2.IsEnabled = true;
                    }
                    else
                    {
                        lecturerNameEntry1.Text = "";
                    }
                }
                else
                {
                    lecturerNameEntry2.IsEnabled = false;
                }

                if (lecturerNameEntry2.IsEnabled)
                {
                    if (!(LecturerLists.FirstOrDefault(x => x.LecturerName == lecturerNameEntry2.Text) == null))
                    {
                        lecturerNameEntry3.IsEnabled = true;
                    }
                    else
                    {
                        lecturerNameEntry2.Text = "";
                    }
                }
                else
                {
                    lecturerNameEntry3.IsEnabled = false;

                }
                labelLecturerNameEntry3.IsVisible = true;
                lecturerNameEntry3.IsVisible = true;
                selectedLecturerNum = 3;
            }
            //lecturerNamePicker1.ItemsSource = lecturerLists;
            //lecturerNamePicker1.ItemDisplayBinding = new Binding("LecturerName");
            lecturerNumberPickerGrid.IsVisible = true;
            classSessionAssignmentListView.IsVisible = false;
            //classSessionAssignmentListView.HeightRequest = 0;
        }

        bool selectingLecturer1 = false;

        private void lecturerNameEntry1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (selectingLecturer1)
            {
                lecturerNameEntry2.Text = "";
                lecturerNameEntry2.IsEnabled = true;
                lecturerNameEntry3.IsEnabled = false;
                potentialLecturerNameList2.ItemsSource = LecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text).ToList();
                return;
            }

            string inputText = lecturerNameEntry1.Text.ToLower();

            var list = new List<LecturerList>();
            list = LecturerLists.Where(x => x.LecturerId.ToLower().Contains(inputText) || x.LecturerName.ToLower().Contains(inputText)).ToList();

            potentialLecturerNameList1.ItemsSource = list;
            potentialLecturerNameList1.IsVisible = true;
            classSessionAssignmentListView.IsVisible = false;
        }

        private void TapLecturerNameGestureRecognizer1_Tapped(object sender, EventArgs e)
        {
            selectingLecturer1 = true;

            var label = sender as Label;
            var lecturer = label.BindingContext as LecturerList;

            if (lecturer != null)
            {
                lecturerNameEntry1.Text = lecturer.LecturerName; // set selectingLecturer = true; so that this assignment wont trigger lecturerNameEntry_TextChanged() again 
                potentialLecturerNameList1.IsVisible = false;
            }

            selectingLecturer1 = false; // setting it back to false so that if user change the text again will trigger lecturerNameEntry_TextChanged()
        }

        bool selectingLecturer2 = false;

        private async void lecturerNameEntry2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (selectingLecturer2)
            {
                if (GetEffectiveLecturerNum() == 3)
                {
                    lecturerNameEntry3.Text = "";
                    lecturerNameEntry3.IsEnabled = true;
                    potentialLecturerNameList3.ItemsSource = LecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text && x.LecturerName != lecturerNameEntry2.Text).ToList();
                }
                else if (GetEffectiveLecturerNum() == 2)
                {
                    if (string.IsNullOrEmpty(lecturerNameEntry1.Text))
                    {
                        return;
                    }

                    if (!anyCheckboxClassChecked)
                    {
                        await DisplayAlert("Info", "Remember to choose at least one class to proceed with lecturer assignment to classes.", "Ok");
                        return;
                    }

                    if (!string.IsNullOrEmpty(lecturerNameEntry2.Text))
                    {
                        displayClassSessionAssignment();
                    }
                }
                return;
            }

            string inputText = lecturerNameEntry2.Text.ToLower();

            var list = LecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text).ToList();
            var potentialList = list.Where(x => x.LecturerId.ToLower().Contains(inputText) || x.LecturerName.ToLower().Contains(inputText)).ToList();
            potentialLecturerNameList2.ItemsSource = potentialList;
            potentialLecturerNameList2.IsVisible = true;
            classSessionAssignmentListView.IsVisible = false;
        }

        private void TapLecturerNameGestureRecognizer2_Tapped(object sender, EventArgs e)
        {
            selectingLecturer2 = true;

            var label = sender as Label;
            var lecturer = label.BindingContext as LecturerList;

            if (lecturer != null)
            {
                lecturerNameEntry2.Text = lecturer.LecturerName; // set selectingLecturer = true; so that this assignment wont trigger lecturerNameEntry_TextChanged() again 
                potentialLecturerNameList2.IsVisible = false;
            }

            selectingLecturer2 = false; // setting it back to false so that if user change the text again will trigger lecturerNameEntry_TextChanged()
        }

        bool selectingLecturer3 = false;

        private async void lecturerNameEntry3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (selectingLecturer3)
            {
                if (string.IsNullOrEmpty(lecturerNameEntry1.Text) || string.IsNullOrEmpty(lecturerNameEntry2.Text))
                {
                    return;
                }

                if (!anyCheckboxClassChecked)
                {
                    await DisplayAlert("Info", "Remember to choose at least one class to proceed with lecturer assignment to classes.", "Ok");
                    return;
                }

                displayClassSessionAssignment();

                return;
            }

            string inputText = lecturerNameEntry3.Text.ToLower();

            var list = LecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text && x.LecturerName != lecturerNameEntry2.Text).ToList();
            var potentialList = list.Where(x => x.LecturerId.ToLower().Contains(inputText) || x.LecturerName.ToLower().Contains(inputText)).ToList();
            potentialLecturerNameList3.ItemsSource = potentialList;
            potentialLecturerNameList3.IsVisible = true;
            classSessionAssignmentListView.IsVisible = false;
        }

        private void TapLecturerNameGestureRecognizer3_Tapped(object sender, EventArgs e)
        {
            selectingLecturer3 = true;

            var label = sender as Label;
            var lecturer = label.BindingContext as LecturerList;

            if (lecturer != null)
            {
                lecturerNameEntry3.Text = lecturer.LecturerName; // set selectingLecturer = true; so that this assignment wont trigger lecturerNameEntry_TextChanged() again 
                potentialLecturerNameList3.IsVisible = false;
            }

            selectingLecturer3 = false; // setting it back to false so that if user change the text again will trigger lecturerNameEntry_TextChanged()
        }

        void displayClassSessionAssignment()
        {
            var existingAssignments = classSessionAssignmentListView.ItemsSource as List<Class_SessionAssignment>;
            var class_sessionAssignmentList = new List<Class_SessionAssignment>();

            var selectedLecturerList = new List<LecturerList>();

            var lecturer1 = LecturerLists.First(x => x.LecturerName == lecturerNameEntry1.Text);
            var lecturer2 = LecturerLists.First(x => x.LecturerName == lecturerNameEntry2.Text);

            selectedLecturerList.Add(lecturer1);
            selectedLecturerList.Add(lecturer2);

            if (GetEffectiveLecturerNum() == 3 && !string.IsNullOrEmpty(lecturerNameEntry3.Text))
            {
                var lecturer3 = LecturerLists.First(x => x.LecturerName == lecturerNameEntry3.Text);
                selectedLecturerList.Add(lecturer3);
            }

            foreach (var selectedClass in EditedClassList)
            {
                if (selectedClass.IsSelected)
                {
                    // existingAssignments? because it might be null if original lecturer number = 1
                    var existing = existingAssignments?.FirstOrDefault(x => x.ClassId == selectedClass.ClassId);

                    if (existing != null)
                    {
                        existing.LecturerNameList1 = selectedLecturerList;
                        existing.LecturerNameList2 = selectedLecturerList.Where(x => x.LecturerId != existing.SelectedLecturer1?.LecturerId).ToList();
                        class_sessionAssignmentList.Add(existing);
                    }
                    else
                    {
                        class_sessionAssignmentList.Add(new Class_SessionAssignment
                        {
                            ClassId = selectedClass.ClassId,
                            ClassName = selectedClass.ClassName,
                            LecturerNameList1 = selectedLecturerList,
                            SelectedLecturer1 = selectedLecturerList[0],
                            SelectedLecturerIndex1 = 0
                        });
                    }                        
                }
            }

            classSessionAssignmentListView.ItemsSource = class_sessionAssignmentList;
            classSessionAssignmentListView.HeightRequest = class_sessionAssignmentList.Count * 200;
            classSessionAssignmentListView.IsVisible = true;
        }

        private void lecturerNamePicker4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            var picker = sender as Picker;
            var selectedLecturer = picker.SelectedItem as LecturerList;

            if (picker.SelectedIndex == -1)
            {
                return;
            }

            var item = picker.BindingContext as Class_SessionAssignment;
            item.SelectedLecturer1 = selectedLecturer;

            if (GetEffectiveLecturerNum() == 2)
            {
                if (picker.SelectedIndex == 0)
                {
                    item.LecturerName2 = item.LecturerNameList1[1].LecturerName;
                    item.SelectedLecturer2 = item.LecturerNameList1[1];
                }
                else
                {
                    item.LecturerName2 = item.LecturerNameList1[0].LecturerName;
                    item.SelectedLecturer2 = item.LecturerNameList1[0];
                }
            }

            item.LecturerNameList2 = item.LecturerNameList1.Where(x => x.LecturerId != selectedLecturer.LecturerId).ToList();

            //foreach (var item2 in item.LecturerNameList2)
            //{
            //    Console.WriteLine(item2.LecturerName);
            //}

            item.isEnableSessionOptionRBGroup1 = true;
        }

        private void lecturerNamePicker5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            var picker = sender as Picker;
            var selectedLecturer = picker.SelectedItem as LecturerList;

            if (picker.SelectedIndex == -1)
            {
                return;
            }

            var item = picker.BindingContext as Class_SessionAssignment;
            item.SelectedLecturer2 = selectedLecturer;
        }

        private void RadioButtonSessionOption_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            if (!e.Value)
                return;

            var radioButton = sender as RadioButton;
            string selectedOption = radioButton.Content.ToString();

            // Get the row's BindingContext
            var item = radioButton.BindingContext as Class_SessionAssignment;
            string previousValue = item.SelectedSessionTypeLecturer1;

            item.SelectedSessionTypeLecturer1 = selectedOption;

            if (selectedOption == "Both")
            {
                item.isVisiblelayoutSession2 = false;
                classSessionAssignmentListView.HeightRequest -= 150;
            }
            else
            {
                item.isVisiblelayoutSession2 = true;

                //if (selectedLecturerNum == 0)
                //{
                //    if (OriginalSelectedLecturerNum == 2)
                //    {
                //        item.isVisibleLecturerLabel = true;
                //        item.isVisibleLecturerPicker = false;
                //    }
                //    else
                //    {
                //        item.isVisibleLecturerLabel = false;
                //        item.isVisibleLecturerPicker = true;
                //    }
                //}
                //else
                //{
                //    if (selectedLecturerNum == 2)
                //    {
                //        item.isVisibleLecturerLabel = true;
                //        item.isVisibleLecturerPicker = false;
                //    }
                //    else
                //    {
                //        item.isVisibleLecturerLabel = false;
                //        item.isVisibleLecturerPicker = true;
                //    }
                //}

                if (GetEffectiveLecturerNum() == 2)
                {
                    item.isVisibleLecturerLabel = true;
                    item.isVisibleLecturerPicker = false;
                }
                else if (GetEffectiveLecturerNum() == 3)
                {
                    item.isVisibleLecturerLabel = false;
                    item.isVisibleLecturerPicker = true;
                }

                if (selectedOption == "Lecture")
                {
                    item.isCheckedLecture2 = false;
                    item.isCheckedLab2 = true;
                    item.SelectedSessionTypeLecturer2 = "Lab";
                }
                else
                {
                    item.isCheckedLecture2 = true;
                    item.isCheckedLab2 = false;
                    item.SelectedSessionTypeLecturer2 = "Lecture";
                }

                if (previousValue == "Both")
                {
                    classSessionAssignmentListView.HeightRequest += 150;
                }
            }
        }

        int GetEffectiveLecturerNum()
        {
            return selectedLecturerNum == 0
                ? OriginalSelectedLecturerNum
                : selectedLecturerNum;
        }


        private async void TapEditSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(subjectNameEntry.Text))
            {
                await DisplayAlert("Alert", "Subject Name field cannot be empty.", "Ok");
                return;
            }

            var subjectName = subjectNameEntry.Text;

            EditedSubjectDetail.SubjectName = subjectNameEntry.Text;

            bool subjectNameChanged = OriginalSubjectDetail.SubjectName != EditedSubjectDetail.SubjectName;
            if (subjectNameChanged)
            {
                var invalidSubjectName = Regex.IsMatch(subjectName, @"[^a-zA-Z0-9 ]"); ;
                if (invalidSubjectName)
                {
                    await DisplayAlert("Alert", "Subject Name cannot contains Special character.", "Ok");
                    return;
                }
            }  

            bool courseIdChanged = OriginalSubjectDetail.CourseId != EditedSubjectDetail.CourseId;
            bool studentYearChanged = OriginalSubjectDetail.StudentYear != EditedSubjectDetail.StudentYear;
            bool semesterIdChanged = OriginalSubjectDetail.SemesterId != EditedSubjectDetail.SemesterId;

            //string elementChanged = "";

            //if (courseIdChanged)
            //{
            //    elementChanged += "Course Id, ";
            //}

            //if (facultyNameChanged)
            //{
            //    elementChanged += "Faculty name,";
            //}

            //if (studentYearChanged)
            //{
            //    elementChanged += "Student year,";
            //}

            //if (semesterIdChanged)
            //{
            //    elementChanged += "Semester ID,";
            //}

            //if (!string.IsNullOrEmpty(elementChanged))
            //{
            //    await DisplayAlert("Alert", $"Following element have changed. \n{elementChanged}", "Ok");
            //}
            //else
            //{
            //    await DisplayAlert("Info", "No element have changed.", "Ok");                
            //}

            if (!confirmationPage)
            {
                if (selectedSingleLecturerOption)
                {
                    if (string.IsNullOrEmpty(lecturerNameEntry.Text))
                    {
                        await DisplayAlert("Alert", "Please select Lecturer name first.", "Ok");
                        return;
                    }

                    if (potentialLecturerNameList.IsVisible)
                    {
                        await DisplayAlert("Alert", "Please select Lecturer name first.", "Ok");
                        return;
                    }
                }
                else
                {
                    if (lecturerNumberPicker.SelectedIndex == -1)
                    {
                        await DisplayAlert("Alert", "Please select Lecturer number first.", "Ok");
                        return;
                    }

                    if (string.IsNullOrEmpty(lecturerNameEntry1.Text))
                    {
                        await DisplayAlert("Alert", "Please select Lecturer 1 name first.", "Ok");
                        return;
                    }

                    if (potentialLecturerNameList1.IsVisible)
                    {
                        await DisplayAlert("Alert", "Please select Lecturer 1 name first.", "Ok");
                        return;
                    }

                    if (string.IsNullOrEmpty(lecturerNameEntry2.Text))
                    {
                        await DisplayAlert("Alert", "Please select Lecturer 2 name first.", "Ok");
                        return;
                    }

                    if (potentialLecturerNameList2.IsVisible)
                    {
                        await DisplayAlert("Alert", "Please select Lecturer 2 name first.", "Ok");
                        return;
                    }

                    if (GetEffectiveLecturerNum() == 3)
                    {
                        if (string.IsNullOrEmpty(lecturerNameEntry3.Text))
                        {
                            await DisplayAlert("Alert", "Please select Lecturer 3 name first.", "Ok");
                            return;
                        }

                        if (potentialLecturerNameList3.IsVisible)
                        {
                            await DisplayAlert("Alert", "Please select Lecturer 3 name first.", "Ok");
                            return;
                        }
                    }
                }
            }

            bool teachingListChanged = false;

            if (!selectedSingleLecturerOption)
            {
                var itemsSource = classSessionAssignmentListView.ItemsSource;

                var classSessionAssignmentListViewData = itemsSource as List<Class_SessionAssignment>;

                foreach (var item in classSessionAssignmentListViewData)
                {
                    if (item.SelectedSessionTypeLecturer1 != "Both" && GetEffectiveLecturerNum() == 3)
                    {
                        if (item.SelectedLecturer2 == null)
                        {
                            await DisplayAlert("Alert", $"Please select Lecturer 2 name for Class {item.ClassName} first.", "Ok");
                            return;
                        }
                    }
                }

                EditedSubjectDetail.teachingList = new List<TeachSubjectList>();

                foreach (var item in classSessionAssignmentListViewData)
                {
                    if (item.SelectedSessionTypeLecturer1 == "Both")
                    {
                        EditedSubjectDetail.teachingList.Add(new TeachSubjectList()
                        {
                            StaffId = item.SelectedLecturer1.LecturerId,
                            ClassId = item.ClassId,
                            LectureStatus = 1,
                            LabStatus = 1
                        });
                    }
                    else
                    {
                        bool lectureStatus = item.SelectedSessionTypeLecturer1 == "Lecture";

                        EditedSubjectDetail.teachingList.Add(new TeachSubjectList()
                        {
                            StaffId = item.SelectedLecturer1.LecturerId,
                            ClassId = item.ClassId,
                            LectureStatus = lectureStatus ? 1 : 0,
                            LabStatus = lectureStatus ? 0 : 1
                        });

                        EditedSubjectDetail.teachingList.Add(new TeachSubjectList()
                        {
                            StaffId = item.SelectedLecturer2.LecturerId,
                            ClassId = item.ClassId,
                            LectureStatus = lectureStatus ? 0 : 1,
                            LabStatus = lectureStatus ? 1 : 0
                        });
                    }
                }

                var originalClassIdList = OriginalSubjectDetail.teachingList.GroupBy(x => x.ClassId).Select(x => x.First().ClassId).ToList();
                var editedClassIdList = EditedSubjectDetail.teachingList.GroupBy(x => x.ClassId).Select(x => x.First().ClassId).ToList();

                var addedClasses = editedClassIdList.Except(originalClassIdList).ToList();

                if (addedClasses.Any())
                {
                    teachingListChanged = true;
                }

                var removedClasses = originalClassIdList.Except(editedClassIdList).ToList();

                if (removedClasses.Any())
                {
                    teachingListChanged = true;
                }

                var commonClasses = originalClassIdList.Intersect(editedClassIdList).ToList();

                foreach (var classId in commonClasses)
                {
                    var originalLecturers = OriginalSubjectDetail.teachingList.Where(x => x.ClassId == classId).OrderBy(x => x.StaffId).ToList();

                    var editedLecturers = EditedSubjectDetail.teachingList.Where(x => x.ClassId == classId).OrderBy(x => x.StaffId).ToList();

                    if (originalLecturers.Count != editedLecturers.Count)
                    {
                        teachingListChanged = true;
                        break;
                    }
                    else
                    {
                        if (originalLecturers.Count == 1) // 1 to 1 comparison
                        {
                            var original = originalLecturers.First();
                            var edited = editedLecturers.First();

                            if (original.StaffId != edited.StaffId) // Different lecturer 
                            {
                                teachingListChanged = true;
                            }
                            else if (original.LectureStatus != edited.LectureStatus || original.LabStatus != edited.LabStatus)
                            {
                                teachingListChanged = true;
                            }
                        }
                        else
                        {
                            foreach (var lecturer in originalLecturers)
                            {
                                var matchingOriginal = editedLecturers.FirstOrDefault(x => x.StaffId == lecturer.StaffId);
                                if (matchingOriginal != null)
                                {
                                    if (lecturer.LectureStatus != matchingOriginal.LectureStatus || lecturer.LabStatus != matchingOriginal.LabStatus)
                                    {
                                        teachingListChanged = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    teachingListChanged = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (teachingListChanged)
                    {
                        break;
                    }
                }
            }
            

            if (confirmationPage)
            {
                bool successApi = false;
                if (subjectNameChanged || courseIdChanged || studentYearChanged || semesterIdChanged)
                {
                    var res = await ApiService.EditSubject(EditedSubjectDetail);

                    if (res.Status == "success")
                    {
                        await DisplayAlert("Success", "Success edit Subject Detail.", "OK");
                        successApi = true;                        
                    }
                    else
                    {
                        await DisplayAlert("Alert", res.Message, "OK");
                        successApi = false;
                    }
                }

                if (teachingListChanged)
                {
                    //Console.WriteLine(EditedSubjectDetail.SubjectId);
                    //Console.WriteLine(EditedSubjectDetail.SubjectName);
                    //Console.WriteLine(EditedSubjectDetail.CourseId);
                    //Console.WriteLine(EditedSubjectDetail.StudentYear);
                    //Console.WriteLine(EditedSubjectDetail.SemesterId);
                    //Console.WriteLine(EditedSubjectDetail.FacultyName);
                    //foreach (var item in EditedSubjectDetail.teachingList)
                    //{
                    //    Console.WriteLine(item.StaffId);
                    //    Console.WriteLine(item.ClassId);
                    //    Console.WriteLine(item.LectureStatus);
                    //    Console.WriteLine(item.LabStatus);
                    //}
                    var res = await ApiService.EditSubjectTeachingList(EditedSubjectDetail);

                    if (res.Status == "success")
                    {
                        await DisplayAlert("Success", "Success edit Subject - Class assignment.", "OK");
                        successApi = true;
                    }
                    else
                    {
                        await DisplayAlert("Alert", res.Message, "OK");
                        successApi = false;
                    }
                }

                if (successApi)
                {
                    App.Current.MainPage = new NavigationPage(new ManageSubjectPage());
                }
                else
                {
                    return;
                }
            }

            if (!confirmationPage)
            {
                if (subjectNameChanged || courseIdChanged || studentYearChanged || semesterIdChanged)
                {
                    await DisplayAlert("Info", "Subject detail changed", "OK");

                    confirmationPage = true;
                }

                if (teachingListChanged)
                {
                    await DisplayAlert("Info", "Class session assignment changed. ", "OK");

                    confirmationPage = true;
                }

                if (confirmationPage)
                {
                    await DisplayAlert("Info", "Please confirm your choice before submit.", "Ok");
                    enablePage(false);
                    await contentScrollview.ScrollToAsync(0, 0, false);
                }
                else
                {
                    await DisplayAlert("Info", "No info edited. Returning back to Manage Subject Page", "Ok");
                    App.Current.MainPage = new NavigationPage(new ManageSubjectPage());
                }
            }  
        }
    }
}