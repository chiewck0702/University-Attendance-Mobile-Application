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

namespace FYP_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgotPasswordPage : ContentPage
    {
        public ApiService ApiService { get; set; }
        public ForgotPasswordPage()
        {
            InitializeComponent();

            ApiService = new ApiService();

            NavigationPage.SetHasNavigationBar(this, false);

            backIcon.Source = ImageSource.FromResource("FYP_App.Image.Arrow left.png");

            newPasswordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye off.png");

            confirmPasswordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye off.png");
        }

        bool isEmailEntryEnable = true;

        private void TapBackButton_Tapped(object sender, EventArgs e)
        {
            if (isEmailEntryEnable)
            {
                App.Current.MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                buttonLabel.Text = "Click to send verification email";
                isEmailEntryEnable = true;
                emailEntry.IsEnabled = true;
            }
        }

        void loadingSendEmail(bool enable)
        {
            sendEmailButton.IsEnabled = !enable;

            loadingIndicator.IsVisible = enable;
            loadingIndicator.IsRunning = enable;
        }

        private async void TapSendEmail_Tapped(object sender, EventArgs e)
        {
            loadingSendEmail(true);
            if (string.IsNullOrEmpty(emailEntry.Text))
            {
                await DisplayAlert("Alert", "Email entry cannot be empty", "Ok");
                loadingSendEmail(false);
                return;
            }

            if (!emailEntry.Text.Contains("@"))
            {
                await DisplayAlert("Alert", "Please enter a valid email address.", "OK");
                loadingSendEmail(false);
                return;
            }

            var emailRequest = new EmailRequest
            {
                Email = emailEntry.Text
            };

            var res = await ApiService.SendEmail(emailRequest);

            if (res != null)
            {
                if (res.Status == "success")
                {
                    await DisplayAlert("Info", $"Verification email successful send to {emailRequest.Email}. \nPlease check Spam or Junk folder if you do not see it in your inbox.", "Ok");

                    buttonLabel.Text = "Click to send verification email again";

                    isEmailEntryEnable = false;
                    emailEntry.IsEnabled = false;
                    loadingSendEmail(false);

                    verificationCodeLayout.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Alert", res.Message, "Ok");
                    loadingSendEmail(false);
                    return;
                }
            }
        }

        public string userRole {  get; set; }
        public string userId { get; set; }

        private async void TapVerifyCode_Tapped(object sender, EventArgs e)
        {
            if (isEmailEntryEnable)
            {
                await DisplayAlert("Alert", "Please click send verification email first.", "Ok");
                return;
            }

            if (string.IsNullOrEmpty(codeEntry.Text))
            {
                await DisplayAlert("Alert", "Code entry cannot be empty", "Ok");
                return;
            }

            var res = await ApiService.VerifyCode(emailEntry.Text, codeEntry.Text);

            if (res != null)
            {
                if (res.Status == "success")
                {
                    await DisplayAlert("Success", "Verification code match.", "Ok");

                    userRole = res.UserRole;
                    userId = res.UserID;

                    codeEntry.IsEnabled = false;
                    verifyCodeButton.IsEnabled = false;
                    newPasswordLayout.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Alert", res.Message, "Ok");
                    return;
                }
            }
        }

        private async void TapInfoButton_Tapped(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Password requirement : \n- Minimum 8 character length \n- At least 1 uppercase \n- At least 1 lowercase \n- At least 1 special character ", "Ok");
            return;
        }

        public bool newPassVisible = false;
        private void TapNewPasswordVisibility_Tapped(object sender, EventArgs e)
        {
            if (newPassVisible)
            {
                // hide password
                newPasswordEntry.IsPassword = true;
                newPasswordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye off.png");
                newPassVisible = false;
            }
            else
            {
                // show password 
                newPasswordEntry.IsPassword = false;
                newPasswordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye.png");
                newPassVisible = true;
            }
        }

        public bool confirmPassVisible = false;
        private void TapConfirmPasswordVisibility_Tapped(object sender, EventArgs e)
        {
            if (confirmPassVisible)
            {
                // hide password
                confirmPasswordEntry.IsPassword = true;
                confirmPasswordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye off.png");
                confirmPassVisible = false;
            }
            else
            {
                // show password 
                confirmPasswordEntry.IsPassword = false;
                confirmPasswordVisibility.Source = ImageSource.FromResource("FYP_App.Image.Eye.png");
                confirmPassVisible = true;
            }
        }

        private async void TapConfirm_Tapped(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(newPasswordEntry.Text))
            {
                await DisplayAlert("Alert", "New password entry cannot be empty", "Ok");
                return;
            }

            var newPassword = newPasswordEntry.Text;

            if (newPassword.Length < 8)
            {
                await DisplayAlert("Alert", "Minimum password length is 8", "Ok");
                return;
            }

            bool hasUpper = newPassword.Any(c => char.IsUpper(c));
            if (!hasUpper)
            {
                await DisplayAlert("Alert", "Password must contain at least 1 uppercase", "Ok");
                return;
            }

            bool hasLower = newPassword.Any(c => char.IsLower(c));
            if (!hasLower)
            {
                await DisplayAlert("Alert", "Password must contain at least 1 lowercase", "Ok");
                return;
            }

            bool hasSpecial = newPassword.Any(c => !char.IsLetterOrDigit(c));
            if (!hasSpecial)
            {
                await DisplayAlert("Alert", "Password must contain at least 1 special character", "Ok");
                return;
            }

            if (string.IsNullOrEmpty(confirmPasswordEntry.Text))
            {
                await DisplayAlert("Alert", "Confirm password entry cannot be empty", "Ok");
                return;
            }

            if (newPassword != confirmPasswordEntry.Text)
            {
                await DisplayAlert("Alert", "Confirm password must match New password", "Ok");
                return;
            }

            var updatePassword = new UpdatePasswordInfo
            {
                UserRole = userRole,
                UserID = userId,
                Password = confirmPasswordEntry.Text
            };

            var res = await ApiService.UpdatePassword(updatePassword);

            if (res != null)
            {
                if (res.Status == "success")
                {
                    await DisplayAlert("Success", "Successful reset password. \nYou may try login again using New Password", "Ok");

                    App.Current.MainPage = new NavigationPage(new MainPage());
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