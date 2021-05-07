using System;
using AVFoundation;
using Foundation;
using System.Linq;

namespace CompressVideo.iOS.Services
{
    public class LightCompressTestFail
    {

        /* Para chamar
         *  NSUrl source = NSUrl.FromFilename(srcPath);
         *  NSUrl destination = NSUrl.FromFilename(srcPath);
         *
         *  CompressVideo(source, destination);
         */


        public enum VideoQuality { high, medium, low }

        public event EventHandler<float> Percent;
        public event EventHandler<bool> Success;
        public event EventHandler<string> Fail;

        const float MIN_BITRATE = 2000000;
        const float MIN_HEIGHT = 640;
        const float MIN_WIDTH = 360;

        public LightCompressTestFail()
        {
        }

        public Compression CompressVideo(NSUrl source, NSUrl destination, bool isMinBitRateEnabled = true, VideoQuality quality = VideoQuality.high, bool keepOriginalResolution = false)
        {

            var frameCount = 0;
            var compressionOperation = new Compression();

            var videoAsset = new AVUrlAsset(source);

            try
            {

                var videoTrack = videoAsset.Tracks.First(x => x.MediaType == AVMediaType.Video);
                var bitrate = videoTrack.EstimatedDataRate;

                // Check for a min video bitrate before compression
                if (isMinBitRateEnabled && bitrate <= MIN_BITRATE)
                {
                    var error = new Compression();
                    error.title = "The provided bitrate is smaller than what is needed for compression try to set isMinBitRateEnabled to false";
                    //TODO: ENVIAR ENVENTO DE ERRO
                    return error;
                }

                var newBitrate = getBitrate(bitrate, quality);

                // Handle new width and height values
                var videoSize = videoTrack.NaturalSize;
                var size = generateWidthAndHeight(videoSize.Width, videoSize.Height, keepOriginalResolution);
                var newWidth = size.Width;
                var newHeight = size.Height;

                // Total Frames
                var durationInSeconds = videoAsset.Duration.Seconds;
                var frameRate = videoTrack.NominalFrameRate;
                var totalFrames = Math.Ceiling(durationInSeconds * (double)(frameRate));

                // Progress
                var totalUnits = Convert.ToInt64(totalFrames);
                //var progress = NSProgress(totalUnits);

                // Setup video writer input
                var videoWriterInput = new AVAssetWriterInput(AVMediaType.Video, getVideoWriterSettings(newBitrate, newWidth, newHeight));
                videoWriterInput.ExpectsMediaDataInRealTime = true;
                videoWriterInput.Transform = videoTrack.PreferredTransform;

                NSError nSError;

                var videoWriter = new AVAssetWriter(destination, AVFileType.QuickTimeMovie, out nSError);
                videoWriter.AddInput(videoWriterInput);

                var videoReaderSettings = new NSDictionary(
                    "PixelFormatType", new NSNumber(875704438)
                );

                var videoReaderOutput = new AVAssetReaderTrackOutput(videoTrack, videoReaderSettings);

                AVAssetReader videoReader;
                try
                {
                    videoReader = new AVAssetReader(videoAsset, out nSError);
                    videoReader.AddOutput(videoReaderOutput);

                }
                catch
                {
                    Console.WriteLine("video reader error: (error)");
                    //TODO - Chamar eventi de erro
                }

                //TODO: Verificar como passar parametro nil
                var audioSettings = new AudioSettings()
                {

                    //EncoderBitRate = 64000,
                    //Format = AudioToolbox.AudioFormatType.,
                    NumberChannels = 1,
                    //SampleRate = 44100
                };

                var audioWriterInput = new AVAssetWriterInput(AVMediaType.Audio, audioSettings);
                audioWriterInput.ExpectsMediaDataInRealTime = false;
                videoWriter.AddInput(audioWriterInput);

                //setup audio reader
                var audioTrack = videoAsset.Tracks.FirstOrDefault(x => x.MediaType == AVMediaType.Audio);
                var audioReaderOutput = new AVAssetReaderTrackOutput(audioTrack, audioSettings);
                var audioReader = new AVAssetReader(videoAsset, out nSError);
                audioReader.AddOutput(audioReaderOutput);
                    videoWriter.StartWriting();


            }
            catch (Exception ex)
            {
                //TODO: Incluir tratamento de erro
                return new Compression();
            }

            return new Compression();
        }

        WidthHeight generateWidthAndHeight(nfloat width, nfloat height, bool keepOriginalResolution)
        {

            if (keepOriginalResolution)
                return new WidthHeight()
                {
                    Height = height,
                    Width = width
                };

            float newWidth;
            float newHeight;

            if (width >= 1920 || height >= 1920)
            {
                newWidth = (float)(width * 0.5);
                newHeight = (float)(height * 0.5);
            }
            else if (width >= 1280 || height >= 1280)
            {
                newWidth = (float)(width * 0.75);
                newHeight = (float)(height * 0.75);
            }
            else if (width >= 960 || height >= 960)
            {
                newWidth = (float)(MIN_HEIGHT * 0.95);
                newHeight = (float)(MIN_WIDTH * 0.95);
            }
            else
            {
                newWidth = (float)(width * 0.9);
                newHeight = (float)(height * 0.9);
            }

            return new WidthHeight()
            {
                Height = Convert.ToInt64(newHeight),
                Width = Convert.ToInt64(newWidth)
            };
        }


        private AVVideoSettingsCompressed getVideoWriterSettings(nfloat bitrate, nfloat width, nfloat height)
        {

            var outputSettings = new AVVideoSettingsCompressed()
            {
                Height = (int)height,
                Width = (int)width,
                Codec = AVVideoCodec.H264,
                CodecSettings = new AVVideoCodecSettings()
                {
                    AverageBitRate = (int)bitrate,
                    VideoCleanAperture = new AVVideoCleanApertureSettings(
                        new NSDictionary(
                        AVVideo.CleanApertureWidthKey, new NSNumber(width),
                        AVVideo.CleanApertureHeightKey, new NSNumber(height),
                        AVVideo.CleanApertureVerticalOffsetKey, new NSNumber(10),
                        AVVideo.CleanApertureHorizontalOffsetKey, new NSNumber(10)
                        )
                    )
                },

                ScalingMode = AVVideoScalingMode.ResizeAspectFill
            };

            return outputSettings;
        }


        int getBitrate(float bitrate, VideoQuality quality)
        {

            if (quality == VideoQuality.low)
                return Convert.ToInt32(bitrate * 0.1);
            else if (quality == VideoQuality.medium)
                return Convert.ToInt32(bitrate * 0.2);
            else if (quality == VideoQuality.high)
                return Convert.ToInt32(bitrate * 0.3);

            return Convert.ToInt32(bitrate * 0.2);
        }

    }

    // Compression Interruption Wrapper
    public class Compression
    {
        public Compression() { }

        public string title;
        public bool cancel = false;
    }

    public class WidthHeight
    {
        public nfloat Height { get; set; }
        public nfloat Width { get; set; }
    }

}
