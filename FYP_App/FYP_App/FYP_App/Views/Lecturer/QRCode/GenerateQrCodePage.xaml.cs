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
    public partial class GenerateQrCodePage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public string selectedSubjectId { get; set; }
        public List<TeachSubjectList> teachSubjectsList { get; set; }
        public List<int> qrSessionTimeSelection { get; set; } = new List<int> { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
        public GenerateQrCodePage(string subjectId, List<TeachSubjectList> teachSubjectsList)
        {
            InitializeComponent();

            ApiService = new ApiService();

            selectedSubjectId = subjectId;

            this.teachSubjectsList = teachSubjectsList;

            subjectPicker.ItemsSource = teachSubjectsList;
            subjectPicker.ItemDisplayBinding = new Binding("SubjectId");

            if (selectedSubjectId != null)
            {
                var selectedItem = teachSubjectsList.FirstOrDefault(x => x.SubjectId == selectedSubjectId);
                subjectPicker.SelectedItem = selectedItem;
            }
            else
            {
                sessionPicker.IsEnabled = false;
            }

            dateIcon.Source = ImageSource.FromResource("FYP_App.Image.Date icon.png");

            todayDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");

            startTimePicker.ItemsSource = qrSessionTimeSelection;
            endTimePicker.IsEnabled = false;

            qrIcon.Source = ImageSource.FromResource("FYP_App.Image.QR code icon.png");

            classPicker.IsEnabled = false;
        }

        private async void subjectPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subjectPicker.SelectedItem is TeachSubjectList teachSubjectList)
            {
                selectedSubjectId = teachSubjectList.SubjectId;
                subjectName.Text = teachSubjectList.SubjectName;

                var sessionTypeList = new List<string>();

                if (teachSubjectList.LectureStatus == 2) // means not yet check / retrieve TeachSubjectLectureClassList
                {
                    if (teachSubjectList.LectureClassList == null)
                    {
                        var lecturerClassList = await ApiService.TeachSubjectLectureClassList(App.User.StaffID, App.User.Role, teachSubjectList.SubjectId);

                        if (lecturerClassList != null)
                        {
                            teachSubjectList.LectureStatus = 1;
                            teachSubjectList.LectureClassList = lecturerClassList;

                            if (lecturerClassList.Count > 1)
                            {
                                string combineClass = "";

                                foreach (var item in lecturerClassList)
                                {
                                    combineClass += item.ClassName + ", ";
                                }

                                combineClass = combineClass.Remove(combineClass.Length - 2);

                                teachSubjectList.LectureClassList.Insert(0, new TeachSubjectClassList 
                                { 
                                    ClassId = 0, 
                                    ClassName = combineClass
                                });
                            }

                            sessionTypeList.Add("Lecture");
                        }
                        else
                        {
                            teachSubjectList.LectureStatus = 0;
                        }
                    }
                }
                else if (teachSubjectList.LectureStatus == 1)
                {
                    sessionTypeList.Add("Lecture");
                }


                if (teachSubjectList.LabStatus == 2) // means not yet check / retrieve
                {
                    if (teachSubjectList.LabClassList == null)
                    {
                        var labClassList = await ApiService.TeachSubjectLabClassList(App.User.StaffID, App.User.Role, teachSubjectList.SubjectId);

                        if (labClassList != null)
                        {
                            teachSubjectList.LabStatus = 1;
                            teachSubjectList.LabClassList = labClassList;

                            sessionTypeList.Add("Lab");
                        }
                        else
                        {
                            teachSubjectList.LabStatus = 0;
                        }
                    }
                }
                else if (teachSubjectList.LabStatus == 1)
                {
                    sessionTypeList.Add("Lab");
                }

                sessionPicker.Items.Clear();
                sessionPicker.ItemsSource = sessionTypeList;
                sessionPicker.IsEnabled = true;
            }
        }

        private void startTimePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startTimePicker.SelectedIndex != -1)
            {
                var endTimeList = qrSessionTimeSelection.Skip(startTimePicker.SelectedIndex + 1).ToList();
                endTimePicker.ItemsSource = endTimeList;
                endTimePicker.IsEnabled = true;
                endTimePicker.SelectedIndex = -1;
            }
        }

        private void sessionPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sessionPicker.SelectedItem == "Lecture")
            {
                var classList = teachSubjectsList.First(x => x.SubjectId == selectedSubjectId).LectureClassList;

                var firstClass = classList.Take(1).ToList();

                classPicker.ItemsSource = firstClass;
                classPicker.ItemDisplayBinding = new Binding("ClassName");
                classPicker.IsEnabled = true;
                classPicker.SelectedIndex = 0;
            }
            else if (sessionPicker.SelectedItem == "Lab")
            {
                var classList = teachSubjectsList.First(x => x.SubjectId == selectedSubjectId).LabClassList;

                classPicker.ItemsSource = classList;
                classPicker.ItemDisplayBinding = new Binding("ClassName");
                classPicker.IsEnabled = true;
            }
        }

        public bool firstTimeGenerateQR = true;

        public int? qrSessionId;

        private async void TapGenerateQrCodeButton_Tapped(object sender, EventArgs e)
        {
            if (subjectPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Subject Code first.", "Ok");
                return; 
            }
            else if (sessionPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Session first.", "Ok");
                return;
            }
            else if (classPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select Class first.", "Ok");
                return;
            }
            else if (startTimePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select session Start Time first.", "Ok");
                return;
            }
            else if (endTimePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Alert", "Please select session Start Time first.", "Ok");
                return;
            }
            else
            {
                var sessionType = (string)sessionPicker.SelectedItem;

                var classList = new List<int>();
                
                var selectedClassName = (TeachSubjectClassList)classPicker.SelectedItem;
                string className = selectedClassName.ClassName;

                if (sessionType == "Lecture")
                {
                    if (className.Length > 4)
                    {
                        var classIDList = teachSubjectsList.First(x => x.SubjectId == selectedSubjectId).LectureClassList;

                        for (int i = 1; i < classIDList.Count; i++)
                        {
                            classList.Add(classIDList[i].ClassId);
                        }
                    }
                }
                else
                {
                    var classId = teachSubjectsList.First(x => x.SubjectId == selectedSubjectId).LabClassList.First(x => x.ClassName == className).ClassId;
                    
                    classList.Add(classId);
                }
                var qrCodeInfo = new GenerateQRCodeInfo()
                {
                    SubjectId = selectedSubjectId,
                    SubjectName = teachSubjectsList.First(x => x.SubjectId == selectedSubjectId).SubjectName,
                    SessionType = sessionType,
                    ClassName = className,
                    QRCode = Guid.NewGuid().ToString(),
                    CreatedTime = new DateTime(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day,
                        (int)startTimePicker.SelectedItem,
                        0,
                        0),
                    ExpiryTime = new DateTime(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day,
                        (int)endTimePicker.SelectedItem,
                        0,
                        0),
                    ClassId = classList
                };                

                if (firstTimeGenerateQR)
                {
                    qrSessionId = await ApiService.GenerateQrCode(qrCodeInfo);

                    if (qrSessionId != null)
                    {
                        await DisplayAlert("Success", "Successful generate QR code.", "Ok");
                        await Navigation.PushAsync(new DisplayQRCodePage(qrCodeInfo, 0));
                        firstTimeGenerateQR = false;
                    }
                    else
                    {
                        await DisplayAlert("Alert", "Failed to generate QR code. \nPlease try again.", "Ok");
                    }
                }
                else
                {
                    var option = await DisplayActionSheet(
                    "You already generate QR Code recently. \nDo you want to CREATE a new QR code or VIEW existing QR code ?",
                    "Cancel",
                    null,
                    "Create new QR Code",
                    "View existing QR Code");

                    if (option == "Create new QR Code")
                    {
                        var confirm = await DisplayAlert("Alert", "Create a new QR Code will replace the old QR Code. \nDo you still want to continue ?", "Yes", "No");
                        
                        if (confirm)
                        {
                            qrCodeInfo.QRCode = Guid.NewGuid().ToString();

                            var res = await ApiService.ReplaceQrCode(qrCodeInfo, (int)qrSessionId);
                            
                            if (res)
                            {
                                await DisplayAlert("Success", "Successful replace old QR code and generate a new QR code.", "Ok");
                                await Navigation.PushAsync(new DisplayQRCodePage(qrCodeInfo, 0));
                            }
                            else
                            {
                                await DisplayAlert("Alert", "Failed to replace old QR code and generate a new QR code. \nPlease try again.", "Ok");
                            }
                        }
                    }
                    else if (option == "View existing QR Code")
                    {
                        await Navigation.PushAsync(new DisplayQRCodePage(qrCodeInfo, 0));
                    }
                }
            }
        }
    }
}