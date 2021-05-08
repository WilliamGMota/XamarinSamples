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

                //Seleciona o arquivo de vídeo
                //var video = await MediaPicker.PickVideoAsync();

                var video = await MediaPicker.CaptureVideoAsync();
                await LoadPhotoAsync(video);
                    
                //Mostra informações do vídeo de origem
                var sizeSrc = new FileInfo(video.FullPath).Length;
                sizeSrcLabel.Text = sizeSrc.ToString();

                //Busca a bliblioteca nativa para verficiar o vídeo
                var convertVideo = DependencyService.Get<IConvertVideoService>();

                //Verifica o bitrate para verificar se vai precisar comprimir o arquivo
                bool needConvert = convertVideo.NeedCompress(video.FullPath, 10);
                needConvert = true;
                string exportPath = String.Empty;
                string exportFilePath = String.Empty;

                //Converte o arquivo para cada plataforma
                if (needConvert && Device.RuntimePlatform == Device.iOS)
                {
                    exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    exportFilePath = Path.Combine(exportPath, "compressed_video.mp4");

                    convertVideo.Success += (object sender, bool e) =>
                    {
                        var newVideo = new FileInfo(exportFilePath);
                        var sizeNew = newVideo.Length;
                        sizeDestLabel.Text = sizeNew.ToString();
                    };

                    convertVideo.Fail += (object sender, string e) =>
                    {
                        var newVideo = new FileInfo(exportFilePath);
                    };

                    var succes = await convertVideo.CompressVideoiOS(video.FullPath, exportFilePath);
                    /*
                    if (succes && File.Exists(exportFilePath))
                    {
                        var newVideo2 = new FileInfo(exportFilePath);
                        var sizeNew2 = newVideo2.Length;
                        sizeDestLabel.Text = sizeNew2.ToString();
                    }
                    */
                }

                if (needConvert && Device.RuntimePlatform == Device.Android)
                {
                    Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            percentLabel.Text = percentConvert.ToString() + " %";
                        });
                        return polling;
                    });

                    exportFilePath = Path.Combine(FileSystem.CacheDirectory, "compressed_" + video.FileName);

                    convertVideo.Percent += (object sender, float e) =>
                    {
                        percentConvert = e;
                    };

                    convertVideo.Success += (object sender, bool e) =>
                    {
                        var newVideo = new FileInfo(exportFilePath);
                        var sizeNew = newVideo.Length;
                        sizeDestLabel.Text = sizeNew.ToString();
                    };

                    convertVideo.CompressVideoAndroid(PhotoPath, exportFilePath);
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
