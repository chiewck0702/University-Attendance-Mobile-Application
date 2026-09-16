using Android.Provider;
using FYP_App.Droid;
using Xamarin.Essentials;
using Xamarin.Forms;
using FYP_App.Models;

[assembly: Dependency(typeof(DeviceID))]
namespace FYP_App.Droid
{
    public class DeviceID : IDeviceInfo
    {
        public string GetDeviceId()
        {
            return Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Settings.Secure.AndroidId);
        }

        public string GetDeviceModel()
        {
            return Android.OS.Build.Model;
        }
    }
}