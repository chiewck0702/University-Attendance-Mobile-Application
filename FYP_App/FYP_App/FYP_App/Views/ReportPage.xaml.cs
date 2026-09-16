using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FYP_App.Views.Report;
using FYP_App.Models;

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public ReportPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            searchIcon.Source = ImageSource.FromResource("FYP_App.Image.Search icon.png");
            
            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        public List<Report_SubjectList> SubjectLists { get; set; }

        async void displayInfo()
        {
            var res = await ApiService.RetrieveReportSubjectList(App.User.StaffID, App.User.Role);

            if (res != null)
            {
                SubjectLists = res;
                subjectListView.ItemsSource = SubjectLists;
            }
        }

        private void ButtonLowAttendance_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedItem = button.BindingContext as Report_SubjectList;

            if (selectedItem != null)
            {
                string subjectId = selectedItem.SubjectId;
                string subjectName = selectedItem.SubjectName;
                App.Current.MainPage = new NavigationPage(new LowAttendancePage(subjectId, subjectName));
            }

        }

        private void ButtonFullAttendance_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedItem = button.BindingContext as Report_SubjectList;

            if (selectedItem != null)
            {
                string subjectId = selectedItem.SubjectId;
                string subjectName = selectedItem.SubjectName;
                App.Current.MainPage = new NavigationPage(new FullAttendancePage(subjectId, subjectName));
            }
        }

        private async void searchEntry_Completed(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(searchEntry.Text))
            //{
            //    await DisplayAlert("Alert", "Subject code / name cannot be empty", "Ok");
            //    return;
            //}

            subjectListView.ItemsSource = SubjectLists.Where(x => x.SubjectId.ToLower().Contains(searchEntry.Text.ToLower()) || x.SubjectName.ToLower().Contains(searchEntry.Text.ToLower())).ToList();
        }
    }
}