using FYP_App.Service;
using FYP_App.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FYP_App
{
    public partial class MainPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public MainPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            utemLogo.Source = ImageSource.FromResource("FYP_App.Image.UTEM logo 1.png");
            passwordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye off.png");
        }

        private async void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(usernameEntry.Text))
            {
                await DisplayAlert("Alert", "Username cannot be empty", "OK");
                return;
            }
            else if (string.IsNullOrEmpty(passwordEntry.Text))
            {
                await DisplayAlert("Alert", "Password cannot be empty", "OK");
                return;
            }

            string userid = usernameEntry.Text.ToLower();

            char userRole = userid[0];

            bool res = false;

            if (userRole == 'd' || userRole == 'b')
            {
                res = await ApiService.LoginStudent(usernameEntry.Text, passwordEntry.Text);
            }
            else
            {
                res = await ApiService.LoginStaff(usernameEntry.Text, passwordEntry.Text);
            }

            if (res)
            {
                await DisplayAlert("Success", "Successful login!", "Ok");
                App.Current.MainPage = new NavigationPage(new UserMainPage());
            }
            else
            {
                await DisplayAlert("Alert", "Login failed. Please try again! \nOr click 'Forgot password' to reset password", "Ok");
                return;
            }
        }

        public bool passVisible = false;
        private void TapPasswordVisibility_Tapped(object sender, EventArgs e)
        {
            if (passVisible)
            {
                // hide password
                passwordEntry.IsPassword = true;
                passwordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye off.png");
                passVisible = false;
            }
            else
            {
                // show password 
                passwordEntry.IsPassword = false;
                passwordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye.png");
                passVisible = true;
            }

        }

        private void TapForgotPassword_Tapped(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new ForgotPasswordPage());
        }
    }
}
