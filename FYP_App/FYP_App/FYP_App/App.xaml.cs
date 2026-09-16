using FYP_App.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FYP_App
{
    public partial class App : Application
    {
        public static User User { get; set; }
        public App()
        {
            InitializeComponent();

            UserAppTheme = OSAppTheme.Light;

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
