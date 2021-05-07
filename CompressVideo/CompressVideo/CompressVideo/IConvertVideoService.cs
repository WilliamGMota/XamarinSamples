using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CompressVideo
{

    public interface IConvertVideoService
    {
        void CompressVideoAndroid(string srcPath, string destPath, int bitrateMode = 10);
        Task<bool> CompressVideoiOS(string inputPath, string outputPath, int bitrateMode = 10);
        bool NeedCompress(string srcPath, int bitrateMode = 10);
        event EventHandler<float> Percent;
        event EventHandler<bool> Success;
        event EventHandler<string> Fail;
    }
}
 