using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CompressVideo
{
    public interface IFileService
    {
        void SaveFile(string name, Stream data, string location = "temp");
    }
}
