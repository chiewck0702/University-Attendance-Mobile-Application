using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App.Views.Subject
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddSubjectPage : ContentPage
	{
        public ApiService ApiService { get; set; }
		public AddSubjectPage ()
		{
			InitializeComponent ();

            ApiService = new ApiService ();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            displayInfo();

            multipleLecturer.IsVisible = false;
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

        public List<LecturerList> lecturerLists = new List<LecturerList> ();

        async void displayInfo()
        {
            var res = await ApiService.RetrieveFacultyList();
            if (res != null)
            {
                facultyPicker.ItemsSource = res;
                coursePicker.IsEnabled = false;
                studentYearPicker.IsEnabled = false;
            }

            var result = await ApiService.RetrieveLecturerList();
            if (result != null)
            {
                lecturerLists = result;
                potentialLecturerNameList.ItemsSource = lecturerLists;
                potentialLecturerNameList.IsVisible = true;
                //lecturerNamePicker.ItemsSource = lecturerLists;
                //lecturerNamePicker.ItemDisplayBinding = new Binding("LecturerName");
            }

            var res2 = await ApiService.RetrieveSemesterList();
            if (res2 != null)
            {
                activeSemesterPicker.ItemsSource = res2;
            }
        }

        public bool subjectVerify = false;

        bool verifySubjectIDName(string inputValue)
        {
            return Regex.IsMatch(inputValue, @"[^a-zA-Z0-9 ]"); // ^ means not // which means return true if the string NOT ONLY contains a-z A-Z 0-9 and space
        }

        private async void ButtonVerify_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(subjectEntry.Text))
            {
                await DisplayAlert("Alert", "Subject ID cannot be empty.", "Ok");
                subjectVerify = false;
                checkIcon.IsVisible = false;
                return;
            }
            else if (subjectEntry.Text.Length != 8)
            {
                await DisplayAlert("Alert", "Subject ID must be 8 character.", "Ok");
                subjectVerify = false;
                checkIcon.IsVisible = false;
                return;
            }

            var invalidId = verifySubjectIDName(subjectEntry.Text);
            if (invalidId)
            {
                await DisplayAlert("Alert", "Subject ID cannot contains any special character.", "Ok");
                return;
            }

            var res = await ApiService.SubjectIdChecking(subjectEntry.Text);
            if (res.Status == "valid")
            {
                await DisplayAlert("Info", $"This subject ID {subjectEntry.Text} is valid.", "Ok");
                subjectVerify = true;
                checkIcon.Source = ImageSource.FromResource("FYP_App.Image.done_icon.png");
                checkIcon.IsVisible = true;
                //subjectEntry.IsReadOnly = true;
            }
            else
            {
                await DisplayAlert("Alert", res.Message, "Ok");
                subjectVerify = false;
                checkIcon.IsVisible = false;
            }
        }

        private void subjectEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (subjectVerify)
            {
                subjectVerify = false;
                checkIcon.IsVisible = false;
            }
        }

        private async void facultyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (facultyPicker.SelectedIndex != -1)
            {
                string facultyName = facultyPicker.SelectedItem as string;
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
            if (coursePicker.SelectedIndex != -1)
            {
                CourseId = coursePicker.SelectedItem.ToString();
                var res = await ApiService.RetrieveStudentYearList(CourseId);
                if (res != null)
                {
                    studentYearPicker.ItemsSource = res;
                    studentYearPicker.IsEnabled = true;
                }
            }
        }

        public List<TeachSubjectClassList> classLists = new List<TeachSubjectClassList>();

        private async void studentYearPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (studentYearPicker.SelectedIndex != -1)
            {
                var res = await ApiService.RetrieveClassList(CourseId, (int)studentYearPicker.SelectedItem, "");
                if (res != null)
                {
                    classLists = res;
                    classListView.ItemsSource = classLists;
                }
            }
        }

        bool anyCheckboxClassChecked = false;

        private void CheckBoxClass_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            
            if (checkbox.BindingContext is TeachSubjectClassList teachSubjectClass)
            {
                teachSubjectClass.IsSelected = e.Value;

                anyCheckboxClassChecked = classLists.Any(c => c.IsSelected);

                if (selectedSingleLecturerOption)
                {
                    var selectedClassList = classLists.Where(c => c.IsSelected).Select(x => x.ClassName).ToList();

                    string classNameList = string.Join(", ", selectedClassList);

                    singleLecturerClassLabel.Text = classNameList;
                }
                else
                {
                    if (!string.IsNullOrEmpty(lecturerNameEntry1.Text) && !string.IsNullOrEmpty(lecturerNameEntry2.Text))
                    {
                        if (selectedLecturerNum == 2)
                        {
                            displayClassSessionAssignment();
                        }
                        else
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

        List<int> lecturerNumberList = new List<int> { 2, 3 };

        bool selectedSingleLecturerOption = true;

        private void RadioButtonLecturerOption_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
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
            if (selectingLecturer)
            {
                return;
            }

            string inputText = lecturerNameEntry.Text.ToLower();

            var list = new List<LecturerList>();
            list = lecturerLists.Where(x => x.LecturerId.ToLower().Contains(inputText) || x.LecturerName.ToLower().Contains(inputText)).ToList();

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
            if (lecturerNumberPicker.SelectedIndex == -1)
            {
                return;
            }

            if (lecturerNumberPicker.SelectedIndex == 0)
            {
                lecturerNameEntry1.Text = "";
                lecturerNameEntry2.Text = "";
                labelLecturerNamePicker3.IsVisible = false;
                lecturerNameEntry3.IsVisible= false;
                lecturerNameEntry2.IsEnabled = false;
                potentialLecturerNameList2.IsVisible = false;
                selectedLecturerNum = 2;
            }
            else
            {
                if (!string.IsNullOrEmpty(lecturerNameEntry1.Text))
                {
                    if (!(lecturerLists.FirstOrDefault(x => x.LecturerName == lecturerNameEntry1.Text) == null))
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
                    lecturerNameEntry1.Text = "";
                    lecturerNameEntry2.IsEnabled = false;
                }

                if (lecturerNameEntry2.IsEnabled)
                {
                    if (!(lecturerLists.FirstOrDefault(x => x.LecturerName == lecturerNameEntry2.Text) == null))
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
                labelLecturerNamePicker3.IsVisible = true;
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
            if (selectingLecturer1)
            {
                lecturerNameEntry2.Text = "";
                lecturerNameEntry2.IsEnabled = true;
                lecturerNameEntry3.IsEnabled = false;
                potentialLecturerNameList2.ItemsSource = lecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text).ToList();
                return;
            }

            string inputText = lecturerNameEntry1.Text.ToLower();

            var list = new List<LecturerList>();
            list = lecturerLists.Where(x => x.LecturerId.ToLower().Contains(inputText) || x.LecturerName.ToLower().Contains(inputText)).ToList();

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
                potentialLecturerNameList2.IsVisible = true;
            }

            selectingLecturer1 = false; // setting it back to false so that if user change the text again will trigger lecturerNameEntry_TextChanged()
        }

        bool selectingLecturer2 = false;

        private async void lecturerNameEntry2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectingLecturer2)
            {
                if (selectedLecturerNum == 3)
                {
                    lecturerNameEntry3.Text = "";
                    lecturerNameEntry3.IsEnabled = true;
                    potentialLecturerNameList3.ItemsSource = lecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text && x.LecturerName != lecturerNameEntry2.Text).ToList();
                }
                else
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

            var list = lecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text).ToList();
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

                if (selectedLecturerNum == 3)
                {
                    potentialLecturerNameList3.IsVisible = true;
                }
            }

            selectingLecturer2 = false; // setting it back to false so that if user change the text again will trigger lecturerNameEntry_TextChanged()
        }

        bool selectingLecturer3 = false;

        private async void lecturerNameEntry3_TextChanged(object sender, TextChangedEventArgs e)
        {
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

            var list = lecturerLists.Where(x => x.LecturerName != lecturerNameEntry1.Text && x.LecturerName != lecturerNameEntry2.Text).ToList();
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
            var class_sessionAssignmentList = new List<Class_SessionAssignment>();

            var selectedLecturerList = new List<LecturerList>();

            var lecturer1 = lecturerLists.First(x => x.LecturerName == lecturerNameEntry1.Text);
            var lecturer2 = lecturerLists.First(x => x.LecturerName == lecturerNameEntry2.Text);

            selectedLecturerList.Add(lecturer1);
            selectedLecturerList.Add(lecturer2);

            if (selectedLecturerNum == 3 && !string.IsNullOrEmpty(lecturerNameEntry3.Text))
            {
                var lecturer3 = lecturerLists.First(x => x.LecturerName == lecturerNameEntry3.Text);
                selectedLecturerList.Add(lecturer3);
            }

            foreach (var selectedClass in classLists)
            {
                if (selectedClass.IsSelected)
                {
                    class_sessionAssignmentList.Add(new Class_SessionAssignment
                    {
                        ClassId = selectedClass.ClassId,
                        ClassName = selectedClass.ClassName,
                        LecturerNameList1 = selectedLecturerList
                    });
                }
            }

            classSessionAssignmentListView.IsVisible = true;
            classSessionAssignmentListView.ItemsSource = class_sessionAssignmentList;
            classSessionAssignmentListView.HeightRequest = class_sessionAssignmentList.Count * 200;
        }

        private void lecturerNamePicker4_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            var selectedLecturer = picker.SelectedItem as LecturerList;

            if (picker.SelectedIndex == -1)
            {
                return;
            }

            var item = picker.BindingContext as Class_SessionAssignment;
            item.SelectedLecturer1 = selectedLecturer;

            if (selectedLecturerNum == 2)
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
            if (!e.Value)
                return;

            var radioButton = sender as RadioButton;
            string selectedOption = radioButton.Content.ToString();

            // Get the row's BindingContext
            var item = radioButton.BindingContext as Class_SessionAssignment;
            item.SelectedSessionTypeLecturer1 = selectedOption;

            if (selectedOption == "Both")
            {
                item.isVisiblelayoutSession2 = false;
                classSessionAssignmentListView.HeightRequest -= 150;
            }
            else
            {
                item.isVisiblelayoutSession2 = true;

                if (selectedLecturerNum == 2)
                {
                    item.isVisibleLecturerLabel = true;
                    item.isVisibleLecturerPicker = false;
                }
                else
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
                classSessionAssignmentListView.HeightRequest += 150;
            }
        }

        private async void TapAddSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (!subjectVerify)
            {
                await DisplayAlert("Alert", "Please click button verify for Subject ID first.", "Ok");
                return;
            }

            var subjectId = subjectEntry.Text;

            if (string.IsNullOrEmpty(subjectNameEntry.Text))
            {
                await DisplayAlert("Alert", "Subject Name field cannot be empty.", "Ok");
                return;
            }

            var subjectName = subjectNameEntry.Text;
            var invalidSubjectName = verifySubjectIDName(subjectName);
            if (invalidSubjectName)
            {
                await DisplayAlert("Alert", "Subject Name cannot contains Special character.", "Ok");
                return;
            }

            if (facultyPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Faculty first.", "Ok");
                return;
            }

            if (coursePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Course first.", "Ok");
                return;
            }

            if (studentYearPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Year of Student first.", "Ok");
                return;
            }

            if (activeSemesterPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Active Semester first.", "Ok");
                return;
            }

            if (!anyCheckboxClassChecked)
            {
                await DisplayAlert("Alert", "Please select at least 1 class first.", "Ok");
                return;
            }

            var teachingList = new List<TeachSubjectList>();

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

                if (confirmationPage)
                {
                    var lecturerInfo = lecturerLists.First(x => x.LecturerName == lecturerNameEntry.Text);

                    foreach (var classInfo in classLists)
                    {
                        if (classInfo.IsSelected)
                        {
                            teachingList.Add(new TeachSubjectList()
                            {
                                StaffId = lecturerInfo.LecturerId,
                                //SubjectId = subjectId,
                                ClassId = classInfo.ClassId,
                                LectureStatus = 1,
                                LabStatus = 1
                            });
                        }
                    }
                }                       
            }
            else // multiple lecturer choose
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

                if (selectedLecturerNum == 3)
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

                var itemsSource = classSessionAssignmentListView.ItemsSource;

                var classSessionAssignmentListViewData = itemsSource as List<Class_SessionAssignment>;

                foreach (var item in classSessionAssignmentListViewData)
                {
                    if (item.SelectedLecturer1 == null)
                    {
                        await DisplayAlert("Alert", $"Please select Lecturer name for Class {item.ClassName} first.", "Ok");
                        return;
                    }

                    if (item.SelectedSessionTypeLecturer1 != "Both" && selectedLecturerNum == 3)
                    {
                        if (item.SelectedLecturer2 == null)
                        {
                            await DisplayAlert("Alert", $"Please select Lecturer 2 name for Class {item.ClassName} first.", "Ok");
                            return;
                        }
                    }
                }

                //var teachingList = new List<TeachSubjectList>();

                if (confirmationPage)
                {
                    foreach (var item in classSessionAssignmentListViewData)
                    {
                        if (item.SelectedSessionTypeLecturer1 == "Both")
                        {
                            teachingList.Add(new TeachSubjectList()
                            {
                                StaffId = item.SelectedLecturer1.LecturerId,
                                //SubjectId = subjectId,
                                ClassId = item.ClassId,
                                LectureStatus = 1,
                                LabStatus = 1
                            });
                        }
                        else
                        {
                            bool lectureStatus = item.SelectedSessionTypeLecturer1 == "Lecture";

                            teachingList.Add(new TeachSubjectList()
                            {
                                StaffId = item.SelectedLecturer1.LecturerId,
                                //SubjectId = subjectId,
                                ClassId = item.ClassId,
                                LectureStatus = lectureStatus ? 1 : 0,
                                LabStatus = lectureStatus ? 0 : 1
                            });

                            teachingList.Add(new TeachSubjectList()
                            {
                                StaffId = item.SelectedLecturer2.LecturerId,
                                //SubjectId = subjectId,
                                ClassId = item.ClassId,
                                LectureStatus = lectureStatus ? 0 : 1,
                                LabStatus = lectureStatus ? 1 : 0
                            });
                        }
                    }
                }                              
            }

            if (confirmationPage)
            {
                var addSubjectDetails = new SubjectDetail
                {
                    SubjectId = subjectId,
                    SubjectName = subjectName,
                    CourseId = coursePicker.SelectedItem.ToString(),
                    StudentYear = (int)studentYearPicker.SelectedItem,
                    SemesterId = (int)activeSemesterPicker.SelectedItem,
                    teachingList = teachingList
                };

                var res = await ApiService.AddSubject(addSubjectDetails);
                if (res.Status == "success")
                {
                    await DisplayAlert("Success", $"Success ADD new Subject: {subjectId} {subjectName}", "Ok");
                    App.Current.MainPage = new NavigationPage(new ManageSubjectPage());
                    return;
                }
                else
                {
                    Console.WriteLine(addSubjectDetails.SubjectId);
                    Console.WriteLine(addSubjectDetails.SubjectName);
                    Console.WriteLine(addSubjectDetails.CourseId);
                    Console.WriteLine(addSubjectDetails.StudentYear);
                    Console.WriteLine(addSubjectDetails.SemesterId);

                    foreach (var item in addSubjectDetails.teachingList)
                    {
                        Console.WriteLine(item.StaffId);
                        Console.WriteLine(item.ClassId);
                        Console.WriteLine(item.LectureStatus);
                        Console.WriteLine(item.LabStatus);
                    }

                    await DisplayAlert("Alert", res.Message, "Ok");
                    return;
                }
            }

            confirmationPage = true;

            if (confirmationPage)
            {
                await DisplayAlert("Info", "Please confirm your choice before submit.", "Ok");
                enablePage(false);
                await contentScrollview.ScrollToAsync(0, 0, false);
            }
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

                if (selectedLecturerNum == 3)
                {
                    lecturerNameEntry3.IsEnabled = enable;
                }

                classSessionAssignmentListView.IsEnabled = enable;
            }
        }
    }
}