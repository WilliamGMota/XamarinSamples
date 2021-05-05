using AbedElazizShe.LightCompressorLibrary;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CompressVideo.Droid.Services;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(ConvertVideoImplementation))]
namespace CompressVideo.Droid.Services
{
    public class ConvertVideoImplementation : IConvertVideoService
    {

        const int bitrateMode10 = 1024 * 10;
        const int bitrateMode2 = 1024 * 2;

        public ConvertVideoImplementation()
        {
        }

        public event EventHandler<float> Percent;
        public event EventHandler<bool> Success;
        public event EventHandler<string> Fail;

        public void ConvertVideo(string srcPath, string destPath, int bitrateMode = 10)
        {
            try
            {
                //Busca dados do vídeo
                var mediaMetadataRetriever = new MediaMetadataRetriever();
                mediaMetadataRetriever.SetDataSource(srcPath);
                string bitrateData = mediaMetadataRetriever.ExtractMetadata(Android.Media.MetadataKey.Bitrate);
                string videoHeight = mediaMetadataRetriever.ExtractMetadata(Android.Media.MetadataKey.VideoHeight);
                string videoWidth = mediaMetadataRetriever.ExtractMetadata(Android.Media.MetadataKey.VideoWidth);

                //Define a compressão padrão para alta
                var videoQuality = VideoQuality.High;

                bitrateMode = bitrateMode == 10 ? bitrateMode10 : bitrateMode2;

                if (!string.IsNullOrEmpty(bitrateData))
                {
                    int bitrate = 0;
                    int.TryParse(bitrateData, out bitrate);
                    bitrate /= 1024;

                    if (bitrate > bitrateMode2)
                    {
                        float reduce = (float)bitrate / (float)bitrateMode2;
                        if (reduce > 6)
                            videoQuality = VideoQuality.Low;
                        else if (reduce > 3)
                            videoQuality = VideoQuality.Medium;
                    }
                }

                var listener = new CompressionListener();
                listener.ProgressPercent += Percent;
                listener.Fail += Fail;
                listener.Success += (sender, e) =>
                {
                    Success(this, true);
                };

                VideoCompressor.Start(srcPath, destPath, listener, videoQuality, false, false);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public bool NeedCompress(string srcPath, int bitrateMode = 10)
        {
            try
            {
                //Busca dados do vídeo
                var mediaMetadataRetriever = new MediaMetadataRetriever();
                mediaMetadataRetriever.SetDataSource(srcPath);
                string bitrateData = mediaMetadataRetriever.ExtractMetadata(Android.Media.MetadataKey.Bitrate);
                string videoHeight = mediaMetadataRetriever.ExtractMetadata(Android.Media.MetadataKey.VideoHeight);
                string videoWidth = mediaMetadataRetriever.ExtractMetadata(Android.Media.MetadataKey.VideoWidth);

                bitrateMode = bitrateMode == 10 ? bitrateMode10 : bitrateMode2;

                if (!string.IsNullOrEmpty(bitrateData))
                {
                    int bitrate = 0;
                    int.TryParse(bitrateData, out bitrate);
                    bitrate /= 1024;
                    return bitrate > bitrateMode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }


        public class CompressionListener : Java.Lang.Object, ICompressionListener
        {

            public event EventHandler<float> ProgressPercent;
            public event EventHandler<bool> Success;
            public event EventHandler<string> Fail;

            public void OnCancelled()
            {
                Console.WriteLine("Cancelou");
            }

            public void OnFailure(string failureMessage)
            {
                Fail(this, failureMessage);
            }

            public void OnProgress(float percent)
            {
                ProgressPercent(this, percent);
            }

            public void OnStart()
            {
                Console.WriteLine("Começou");
            }

            public void OnSuccess()
            {
                Success(this, true);
            }
        }

    }
}