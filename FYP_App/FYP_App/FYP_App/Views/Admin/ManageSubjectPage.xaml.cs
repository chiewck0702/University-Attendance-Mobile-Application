using FYP_App.Models;
using FYP_App.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FYP_App.Views.Subject;

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageSubjectPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public ManageSubjectPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            searchIcon.Source = ImageSource.FromResource("FYP_App.Image.Search icon.png");
            addIcon.Source = ImageSource.FromResource("FYP_App.Image.Add icon.png");
            editIcon.Source = ImageSource.FromResource("FYP_App.Image.Edit icon.png");
            deleteIcon.Source = ImageSource.FromResource("FYP_App.Image.Delete icon.png");

            displayInfo();
        }

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new UserMainPage());
        }

        public List<AvailableSubjectList> availableSubjectLists = new List<AvailableSubjectList>();

        async void displayInfo()
        {
            var res = await ApiService.RetrieveAvailableSubjectList();
            if (res != null)
            {
                availableSubjectLists = res;
                subjectList.ItemsSource = availableSubjectLists;
            }
        }

        private void searchEntry_Completed(object sender, EventArgs e)
        {
            subjectList.ItemsSource = availableSubjectLists.Where(x => x.SubjectId.Contains(searchEntry.Text) || x.SubjectName.Contains(searchEntry.Text)).ToList();
        }

        private async void TapAddSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddSubjectPage());
        }

        private void TapViewSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var label = sender as Label;
            var subject = label.BindingContext as AvailableSubjectList;

            Console.WriteLine(subject.SubjectId);

            if (subject != null)
            {
                App.Current.MainPage = new NavigationPage(new StaticViewSubjectDetailPage(subject.SubjectId, subject.SubjectName));
            }
        }

        bool anyCheckboxChecked = false;

        private void subjectCheckbox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkbox = (CheckBox)sender;

            if (checkbox.BindingContext is AvailableSubjectList availableSubjectList)
            {
                availableSubjectList.isSelected = e.Value;
            }

            anyCheckboxChecked = availableSubjectLists.Any(x => x.isSelected);
        }

        private async void TapEditSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (!anyCheckboxChecked)
            {
                await DisplayAlert("Alert", "Please select one subject first (by click checkbox beside target subject).", "Ok");
                return;
            }

            var subject = availableSubjectLists.Where(x => x.isSelected).ToList();

            if (subject.Count > 1)
            {
                await DisplayAlert("Alert", "Please select ONLY one subject to be edit (by untick checkbox beside unwanted subject).", "Ok");
                return;
            }

            App.Current.MainPage = new NavigationPage(new EditSubjectPage(subject.First().SubjectId, subject.First().SubjectName));
        }

        private async void TapDeleteSubjectGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (!anyCheckboxChecked)
            {
                await DisplayAlert("Alert", "Please select one subject first (by click checkbox beside target subject).", "Ok");
                return;
            }

            var subject = availableSubjectLists.Where(x => x.isSelected).ToList();

            string subjectNameList = string.Join(",", subject.Select(x => x.SubjectName));

            var confirm = await DisplayAlert("Alert", $"Are you sure you want to DELETE Subject: {subjectNameList} ? \nIt will delete also attendance record for this subject.", "Yes", "No");

            if (confirm)
            {
                var deleteSubjectList = new DeleteSubjectList();
                deleteSubjectList.SubjectIdList = subject.Select(x => x.SubjectId).ToList();

                var res = await ApiService.DeleteSubject(deleteSubjectList);

                if (res.Status == "success")
                {
                    await DisplayAlert("Success", $"Success delete Subject: {subjectNameList}", "Ok");

                    availableSubjectLists.RemoveAll(x => x.isSelected);
                    subjectList.ItemsSource = null;
                    subjectList.ItemsSource = availableSubjectLists;

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