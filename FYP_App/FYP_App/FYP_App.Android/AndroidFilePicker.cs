using Android.App;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
using FYP_App.Droid;
using FYP_App.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using AndroidUri = Android.Net.Uri;

[assembly: Dependency(typeof(AndroidFilePicker))]
namespace FYP_App.Droid
{
    public class AndroidFilePicker : IAndroidFilePicker
    {
        const int PickFileRequestCode = 999;
        TaskCompletionSource<string> _tcs;

        public Task<string> PickCsvFileAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            string[] mimeTypes = { "text/csv", "text/comma-separated-values", "application/csv", "application/pdf" };
            intent.PutExtra(Intent.ExtraMimeTypes, mimeTypes);

            MainActivity.Instance.StartActivityForResult(intent, MainActivity.PickFileRequestCode, tcs);

            return tcs.Task;
        }
    }
}
