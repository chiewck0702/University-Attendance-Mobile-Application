using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using FYP_App.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.IO;

namespace FYP_App.Droid
{
    [Activity(Label = "FYP_App", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }
        public const int PickFileRequestCode = 1000;
        private TaskCompletionSource<string> _pickFileTaskCompletionSource;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Instance = this;
            Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void StartActivityForResult(Intent intent, int requestCode, TaskCompletionSource<string> tcs)
        {
            _pickFileTaskCompletionSource = tcs;
            StartActivityForResult(intent, requestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == PickFileRequestCode && resultCode == Result.Ok)
            {
                try
                {
                    var uri = data.Data;
                    var stream = ContentResolver.OpenInputStream(uri);

                    // Save to cache directory
                    var fileName = GetFileName(uri);
                    var filePath = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, fileName);

                    using (var fileStream = File.Create(filePath))
                    {
                        stream.CopyTo(fileStream);
                    }

                    _pickFileTaskCompletionSource?.SetResult(filePath);
                }
                catch (Exception ex)
                {
                    _pickFileTaskCompletionSource?.SetException(ex);
                }
            }
            else
            {
                _pickFileTaskCompletionSource?.SetResult(null);
            }
        }

        private string GetFileName(Android.Net.Uri uri)
        {
            string fileName = "file.csv";
            var cursor = ContentResolver.Query(uri, null, null, null, null);
            if (cursor != null && cursor.MoveToFirst())
            {
                int nameIndex = cursor.GetColumnIndex(Android.Provider.OpenableColumns.DisplayName);
                if (nameIndex >= 0)
                {
                    fileName = cursor.GetString(nameIndex);
                }
                cursor.Close();
            }
            return fileName;
        }

    }
}