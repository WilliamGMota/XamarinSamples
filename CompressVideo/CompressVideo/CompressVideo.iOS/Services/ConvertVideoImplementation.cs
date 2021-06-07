using System;
using CompressVideo.iOS.Services;
using AVFoundation;
using Foundation;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(ConvertVideoImplementation))]
namespace CompressVideo.iOS.Services
{
    public class ConvertVideoImplementation : IConvertVideoService
    {

        const int bitrateMode10 = 1024 * 10;
        const int bitrateMode2 = 1024 * 2;

        public event EventHandler<float> Percent;
        public event EventHandler<bool> Success;
        public event EventHandler<string> Fail;

        public ConvertVideoImplementation()
        {
        }

        /// <summary>
        /// Compress the video
        /// </summary>
        /// <param name="inputPath">Arquivo de Origem</param>
        /// <param name="outputPath">Arquivo de Destino</param>
        /// <returns></returns>
        public async Task CompressVideo(string inputPath, string outputPath, int bitrateMode = 10)
        {

            AVAssetExportSessionPreset quality = AVAssetExportSessionPreset.Preset1280x720;

            float bitrate = 0;

            //Delete compress video if exist
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            //Buscar o bitrate do video
            try
            {
                NSUrl source = NSUrl.FromFilename(inputPath);
                var videoAsset = new AVUrlAsset(source);

                var videoTrack = videoAsset.Tracks.First(x => x.MediaType == AVMediaType.Video);
                bitrate = videoTrack.EstimatedDataRate;

                bitrate /= 1024;
            }
            catch { }

            //Define a qualidade
            bitrateMode = bitrateMode == 10 ? bitrateMode10 : bitrateMode2;

            if (bitrate > 0 && bitrate > bitrateMode)
            {
                float reduce = (float)bitrate / (float)bitrateMode;
                if (reduce > 6)
                    quality = AVAssetExportSessionPreset.LowQuality;
                else if (reduce > 1.1)
                    quality = AVAssetExportSessionPreset.MediumQuality;
            }

            //Comprime o vídeo
            try
            {
                var asset = AVAsset.FromUrl(NSUrl.FromFilename(inputPath));
                AVAssetExportSession export = new AVAssetExportSession(asset, quality);

                export.OutputUrl = NSUrl.FromFilename(outputPath);
                export.OutputFileType = AVFileType.Mpeg4;
                export.ShouldOptimizeForNetworkUse = true;

                await RunExportAsync(export);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Erro ao comprimir video");
            }

            return;
        }

        private async Task RunExportAsync(AVAssetExportSession exp)
        {
            await exp.ExportTaskAsync();
            if (exp.Status == AVAssetExportSessionStatus.Completed)
            {
                Success(this, true);
            }
            else if (exp.Status == AVAssetExportSessionStatus.Failed)
            {
                Fail(this, exp.Error.Description);
            }
            else
            {
                Fail(this, "Fail to compress video. Error unknown");
            }
        }

        /// <summary>
        /// Check bitrate video is bigger than 2 or 10
        /// </summary>
        /// <param name="srcPath">Video file path</param>
        /// <param name="bitrateMode">Max bitrate allowed</param>
        /// <returns></returns>
        public bool NeedCompress(string srcPath, int bitrateMode = 10)
        {

            //Define a compressão padrão para alta
            bitrateMode = bitrateMode == 10 ? bitrateMode10 : bitrateMode2;

            NSUrl source = NSUrl.FromFilename(srcPath);

            var videoAsset = new AVUrlAsset(source);

            try
            {
                var videoTrack = videoAsset.Tracks.First(x => x.MediaType == AVMediaType.Video);
                var bitrate = videoTrack.EstimatedDataRate;

                bitrate /= 1024;
                return bitrate > bitrateMode;
            }
            catch { }

            return false;
        }
    }
}
