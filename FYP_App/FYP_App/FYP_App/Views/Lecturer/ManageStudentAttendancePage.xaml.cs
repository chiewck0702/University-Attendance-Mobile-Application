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
    public partial class ManageStudentAttendancePage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public List<TeachSubjectList> teachSubjectsList { get; set; } = new List<TeachSubjectList>();
        public string SubjectId { get; set; }
        public ManageStudentAttendancePage(string subjectId)
        {
            InitializeComponent();

            ApiService = new ApiService();

            SubjectId = subjectId;

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            dateIcon.Source = ImageSource.FromResource("FYP_App.Image.Date icon.png");

            todayDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            arrowUpIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow up.png");

            displaySubjectList();

            qrIcon.Source = ImageSource.FromResource("FYP_App.Image.QR code icon.png");

            attendedStudentList.IsVisible = false;
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

        async void displaySubjectList()
        {
            teachSubjectsList = await ApiService.TeachingSubjectList(App.User.StaffID);
            subjectPicker.ItemsSource = teachSubjectsList;
            subjectPicker.ItemDisplayBinding = new Binding("SubjectId");  
            
            if (!string.IsNullOrEmpty(SubjectId))
            {
                int index = teachSubjectsList.FindIndex(x => x.SubjectId == SubjectId);
                subjectPicker.SelectedIndex = index;
            }
        }

        public List<CurrentAttendedStudentList> currentAttendedStudents { get; set; } = new List<CurrentAttendedStudentList>();

        private async void subjectPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subjectPicker.SelectedItem is TeachSubjectList selectedSubject)
            {
                subjectName.Text = selectedSubject.SubjectName;

                selectedSubjectId = selectedSubject.SubjectId;
                selectedSubjectName = selectedSubject.SubjectName;
                retrieveAttendedStudentList();
            }
        }

        public string selectedSubjectId { get; set; }

        public string selectedSubjectName { get; set; }

        public CurrentActiveSession currentActiveSession { get; set; } = new CurrentActiveSession();

        async void retrieveAttendedStudentList()
        {
            currentActiveSession = await ApiService.CurrentActiveSession(App.User.StaffID, selectedSubjectId);

            if (currentActiveSession != null)
            {
                currentAttendedStudents = currentActiveSession.currentAttendedStudentLists;
                QrSessionId = currentActiveSession.QrSessionId;

                activeSessionFrame.BackgroundColor = Color.FromHex("#9BE89F");
                activeSessionLabel.Text = $"Active Session \n{currentActiveSession.CreatedTime} to {currentActiveSession.ExpiryTime}";
                generateQrButtonLabel.Text = "Generate New Qr Code / \nView QR code";

                attendedStudentList.IsVisible = true;
                
                addIcon.Source = ImageSource.FromResource("FYP_App.Image.Add icon.png");
                refreshIcon.Source = ImageSource.FromResource("FYP_App.Image.Refresh icon.png");
                peopleIcon.Source = ImageSource.FromResource("FYP_App.Image.People icon.png");
                studentNum.Text = currentActiveSession.StudentCount.ToString();

                int counter = 1;
                foreach (var item in currentAttendedStudents)
                {
                    item.Counter = counter;
                    counter++;
                }

                studentListView.ItemsSource = null;
                studentListView.ItemsSource = currentAttendedStudents;
                //editStudentAttendValidality.Source = ImageSource.FromResource("FYP_App.Image.Edit.png");
            }
            else
            {
                activeSessionFrame.BackgroundColor = Color.LightGray;
                activeSessionLabel.Text = "No Active Session";
                generateQrButtonLabel.Text = "Generate QR Code";
                attendedStudentList.IsVisible = false;
                QrSessionId = 0;
            }
        }

        public int QrSessionId { get; set; }

        private async void TapEditStudentAttendValid_Tapped(object sender, EventArgs e)
        {
            var Sender = (Image)sender;

            if (Sender.BindingContext is CurrentAttendedStudentList student)
            {
                string studentId = student.StudentId;
                string studentName = student.StudentName;
                int isValid = student.IsValid;

                bool confirm = false;

                if (isValid == 1)
                {
                    confirm = await DisplayAlert("Edit Attendance", $"Do you want to change this student attendance to INVALID ? \nStudent: {studentName} ({studentId})", "Yes", "No");
                }
                else
                {
                    confirm = await DisplayAlert("Edit Attendance", $"Do you want to change this student attendance to VALID ? \nStudent: {studentName} ({studentId})", "Yes", "No");
                }

                if (confirm)
                {
                    var updateStudentValidationInfo = new UpdateStudentAttendanceValidation
                    {
                        QrSessionId = QrSessionId,
                        MatricNo = studentId,
                        IsValid = isValid
                    };

                    var res = await ApiService.UpdateStudentAttendanceValidility(updateStudentValidationInfo);

                    if (res)
                    {
                        if (Sender.Parent is Grid grid) // the Grid inside the ViewCell
                        {
                            if (isValid == 1) // going to change to invalid 
                            {
                                grid.BackgroundColor = Color.FromHex("#FFA49F");
                                student.IsValid = 0;
                            }
                            else
                            {
                                grid.BackgroundColor = Color.White;
                                student.IsValid = 1;
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Alert", "Update failed because attendance record is not found", "Ok");
                    }
                }
            }
        }

        private void TapRefreshButton_Tapped(object sender, EventArgs e)
        {
            retrieveAttendedStudentList();
            App.Current.MainPage = new NavigationPage(new ManageStudentAttendancePage(selectedSubjectId));
        }

        private async void TapGenerateQrCodeButton_Tapped(object sender, EventArgs e)
        {
            if (QrSessionId == 0) // dont have active session one 
            {
                await Navigation.PushAsync(new GenerateQrCodePage(selectedSubjectId, teachSubjectsList)); 
            }
            else
            {
                var option = await DisplayActionSheet(
                    "Do you want to CREATE a new QR code or VIEW existing QR code ?",
                    "Cancel",
                    null,
                    "Create new QR Code",
                    "View existing QR Code");
                if (option == "Create new QR Code")
                {
                    var confirm = await DisplayAlert("Alert", "There is existing QR code. \nAre you sure you want to create a new QR code ?", "Yes", "No");

                    if (confirm)
                    {
                        await Navigation.PushAsync(new GenerateQrCodePage(selectedSubjectId, teachSubjectsList));
                    }
                    return;
                }
                else if (option == "View existing QR Code")
                {
                    await Navigation.PushAsync(new DisplayQRCodePage(null, QrSessionId));
                }
            }
        }

        private async void TapAddButton_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManualCheckinStudentPage(currentActiveSession, selectedSubjectId, selectedSubjectName));
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }
    }
}