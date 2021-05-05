using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CompressVideo.Droid.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileServiceImplementation))]
namespace CompressVideo.Droid.Services
{
    public class FileServiceImplementation : IFileService
    {
        public void SaveFile(string name, Stream data, string location = "temp")
        {


            try
            {

                var documentsPath = "/storage/emulated/0/Download"; 
                Directory.CreateDirectory(documentsPath);

                string filePath = Path.Combine(documentsPath, name);

                byte[] bArray = new byte[data.Length];
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    using (data)
                    {
                        data.Read(bArray, 0, (int)data.Length);
                    }
                    int length = bArray.Length;
                    fs.Write(bArray, 0, length);
                }


                if (File.Exists(filePath)) 
                {
                    var teste = new FileInfo(filePath);
                    Console.WriteLine(teste.FullName);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}