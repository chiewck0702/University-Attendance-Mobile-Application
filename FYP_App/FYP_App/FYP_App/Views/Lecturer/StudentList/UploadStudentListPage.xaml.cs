using FYP_App.Models;
using FYP_App.Service;
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
    public partial class UploadStudentListPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public UploadStudentListPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        async void displayInfo()
        {
            var res = await ApiService.RetrieveReportSubjectList(App.User.StaffID, App.User.Role);
            if (res != null)
            {
                foreach (var item in res)
                {
                    if (item.PeopleNum != 0)
                    {
                        item.ImageIcon = ImageSource.FromResource("FYP_App.Image.done_icon.png");
                    }
                    else
                    {
                        item.ImageIcon = ImageSource.FromResource("FYP_App.Image.red_dot.png");
                    }
                }
            }

            subjectListview.ItemsSource = res;
            subjectListview.HeightRequest = res.Count() * 150;
        }

        private async void TapUploadStudentList_Tapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            var subject = frame.BindingContext as Report_SubjectList;

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

                var res = await ApiService.UploadCSVFile(filePath);
                if (res != null)
                {
                    App.Current.MainPage = new NavigationPage(new DisplayUploadedStudentList(subject, res));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}