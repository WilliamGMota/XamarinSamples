using System;
using System.Collections.Generic;
using System.Text;

namespace CompressVideo
{

    public interface IConvertVideoService
    {
        void ConvertVideo(string srcPath, string destPath, int bitrateMode = 10);
        bool NeedCompress(string srcPath, int bitrateMode = 10);
        event EventHandler<float> Percent;
        event EventHandler<bool> Success;
        event EventHandler<string> Fail;
    }
}
