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

        bool polling;     // Controle para a MainThread
        float percentConvert = 0;     // Controle para a MainThread

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
                             statusPastaWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (await Permissions.RequestAsync<Permissions.StorageRead>() == PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.StorageWrite>() == PermissionStatus.Granted)
            {
                await SelectVideoAsync();
            }
        }

        async Task SelectVideoAsync()
        {
            try
            {

                polling = true;

                //Seleciona o arquivo de vídeo
                var video = await MediaPicker.PickVideoAsync();
                await LoadPhotoAsync(video);

                //Mostra informações do vídeo de origem
                var sizeSrc = new FileInfo(video.FullPath).Length;
                sizeSrcLabel.Text = sizeSrc.ToString();

                var convertVideo = DependencyService.Get<IConvertVideoService>();

                bool needConvert = convertVideo.NeedCompress(video.FullPath, 10);

                if (needConvert)
                {
                    Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            percentLabel.Text = percentConvert.ToString() + " %";
                        });
                        return polling;
                    });

                    var newFile = Path.Combine(FileSystem.CacheDirectory, "converted_" + video.FileName);

                    convertVideo.Percent += (object sender, float e) =>
                    {
                        percentConvert = e;
                    };

                    convertVideo.Success += (object sender, bool e) =>
                    {
                        var newVideo = new FileInfo(newFile);
                        var sizeNew = newVideo.Length;
                        sizeDestLabel.Text = sizeNew.ToString();

                        using (FileStream fs = File.OpenRead(newFile))
                            DependencyService.Get<IFileService>().SaveFile("converted.mp4", fs, "videoFolder");

                    };

                    convertVideo.ConvertVideo(PhotoPath, newFile, 10);

                    Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
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
