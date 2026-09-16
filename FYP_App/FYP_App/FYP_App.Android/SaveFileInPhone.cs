using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Provider;
using FYP_App.Droid;
using FYP_App.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing;
using ZXing.Common;

[assembly: Dependency(typeof(SaveFileInPhone))] // ← Register with Xamarin.Forms
namespace FYP_App.Droid
{
    public class SaveFileInPhone : ISaveFileInPhone
    {
        public async Task<bool> SaveQRCode(string qrCodeValue, string fileName)
        {
            try
            {
                // Generate QR code bitmap
                var writer = new BarcodeWriter<Bitmap>
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions
                    {
                        Width = 500,
                        Height = 500,
                        Margin = 1
                    },
                    Renderer = new ZXing.Mobile.BitmapRenderer()
                };

                var bitmap = writer.Write(qrCodeValue);

                // Use MediaStore for Android 10+ (works for all versions)
                var contentValues = new ContentValues();
                contentValues.Put(MediaStore.Images.Media.InterfaceConsts.DisplayName, fileName); // to declare the file name
                contentValues.Put(MediaStore.Images.Media.InterfaceConsts.MimeType, "image/png"); // to declare the file type 
                contentValues.Put(MediaStore.Images.Media.InterfaceConsts.RelativePath, 
                    Android.OS.Environment.DirectoryPictures); // to declare where to put (Pictures folder) 

                var resolver = Android.App.Application.Context.ContentResolver; // Gets Android's "file manager" that handles saving files and ask them to give / allocate a space to store a file (get hotel Receptionist)
                var imageUri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri, contentValues); // Tells Android: "I want to save an image with these details (contentValues)" (tell it room specification we want)
                // and then it will give back a URI (like an address/ID) also like here is the storage locker number (give the room key and number)

                if (imageUri != null) // as long as locker number is not null, which means success ask the manager to allocate space (success get key and number)
                {
                    using (var outputStream = resolver.OpenOutputStream(imageUri)) // go to the address and store the image file (go to the room ourself and put things inside)
                    {
                        await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, outputStream);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving QR code: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveFileToDownloads(byte[] fileBytes, string fileName)
        {
            try
            {
                var contentValues = new ContentValues();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "text/csv");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

                var resolver = Android.App.Application.Context.ContentResolver;
                var uri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, contentValues);

                if (uri == null)
                    return false;

                using (var stream = resolver.OpenOutputStream(uri))
                {
                    await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
                    await stream.FlushAsync();   // <— make sure data is fully written
                }

                // Force Media Scanner to detect the file immediately
                MediaScannerConnection.ScanFile(Android.App.Application.Context,
                    new[] { uri.ToString() },
                    new[] { "text/csv" },
                    null);


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveFileToDownloads error: " + ex.Message);
                return false;
            }
        }

    }
}