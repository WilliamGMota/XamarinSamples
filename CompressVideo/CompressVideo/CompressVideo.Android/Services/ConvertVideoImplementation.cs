using AbedElazizShe.LightCompressorLibrary;
using Android.App;
using Android.Content;
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

        public ConvertVideoImplementation()
        {
        }

        public event EventHandler<float> Percent;
        public event EventHandler<bool> Success;
        public event EventHandler<string> Fail;

        public void ConvertVideo(string srcPath, string destPath)
        {
            try
            {
                var listener = new CompressionListener();
                listener.ProgressPercent += Percent;
                listener.Fail += Fail;
                listener.Success += (sender, e) =>
                {
                    Success(this, true);
                };

                VideoCompressor.Start(srcPath, destPath, listener, VideoQuality.Medium, false, false);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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
                Console.WriteLine("Concluiu");
            }
        }

    }
}