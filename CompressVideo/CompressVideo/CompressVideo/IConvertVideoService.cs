using System;
using System.Collections.Generic;
using System.Text;

namespace CompressVideo
{

    public interface IConvertVideoService
    {
        void ConvertVideo(string srcPath, string destPath);
        event EventHandler<float> Percent;
        event EventHandler<bool> Success;
        event EventHandler<string> Fail;
    }
}
