using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CompressVideo
{
    public partial class MainPage : ContentPage
    {

        bool polling;                 // Control to MainThread
        float percentConvert = 0;     // Controle to display percent compress

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        string PhotoPath;

        private async void Button_Clicked(object sender, EventArgs e)
        {
            PermissionStatus statusPastaRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>(),
                             statusPastaWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>(),
                             camera = await Permissions.CheckStatusAsync<Permissions.Camera>(),
                             media = await Permissions.CheckStatusAsync<Permissions.Media>() ;

            if (await Permissions.RequestAsync<Permissions.StorageRead>() == PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.StorageWrite>() == PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.Camera>() == PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.Media>() == PermissionStatus.Granted)
            {
                await SelectVideoAsync();
            }
        }

        async Task SelectVideoAsync()
        {
            try
            {

                polling = true;

                //Enable if you want to pick up video from Gallery
                //var video = await MediaPicker.PickVideoAsync();

                // Capture de video from camera
                var video = await MediaPicker.CaptureVideoAsync();
                await LoadPhotoAsync(video);
                    
                //Get and show de size of the file
                var sizeSrc = new FileInfo(video.FullPath).Length;
                sizeSrcLabel.Text = sizeSrc.ToString();

                //Load inferface from native code
                var convertVideo = DependencyService.Get<IConvertVideoService>();

                //Check the bitrate and define if you need to compress the file
                bool needConvert = convertVideo.NeedCompress(video.FullPath, 10);
                needConvert = true;
                string exportPath = String.Empty;
                string exportFilePath = String.Empty;

                //Compress video if bitrate is langer 
                if (needConvert)
                {
                    //Define the temp name
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        exportFilePath = Path.Combine(exportPath, "compressed_video.mp4");
                    }
                    else
                        exportFilePath = Path.Combine(FileSystem.CacheDirectory, "compressed_" + video.FileName);

                    //Converting Code
                    Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            percentLabel.Text = percentConvert.ToString() + " %";
                        });
                        return polling;
                    });

                    convertVideo.Percent += (object sender, float e) =>
                    {
                        percentConvert = e;
                    };

                    convertVideo.Success += (object sender, bool e) =>
                    {
                        var newVideo = new FileInfo(exportFilePath);
                        var sizeNew = newVideo.Length;
                        sizeDestLabel.Text = sizeNew.ToString();
                        polling = false;
                    };

                    convertVideo.Fail += (object sender, string message) =>
                    {
                        //Do something when convert fail        
                        throw new NotImplementedException();
                    };

                    await convertVideo.CompressVideo(PhotoPath, exportFilePath);
                }
                else
                {
                    //Do something when bitrate is slow
                    throw new NotImplementedException();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature is now supported on the device
                Console.WriteLine(fnsEx.ToString());
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine(pEx.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }

        async Task LoadPhotoAsync(FileResult video)
        {
            // canceled
            if (video == null)
            {
                PhotoPath = null;               
                return;
            }
            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, video.FileName);
            using (var stream = await video.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
                await stream.CopyToAsync(newStream);

            PhotoPath = newFile;
        }

    }
}
