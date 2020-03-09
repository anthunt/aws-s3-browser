using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace S3Browser.Utils
{
    class ResourceHelper
    {
        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, UriKind.Absolute));
        }

        public static BitmapFrame GetBitmapFrameForResourceImage(Bitmap resource)
        {
            MemoryStream imgStream = new MemoryStream();
            resource.Save(imgStream, ImageFormat.Bmp);
            imgStream.Seek(0, SeekOrigin.Begin);

            return BitmapFrame.Create(imgStream);
        }

        static readonly string[] BinaryPrefix = { "bytes", "KB", "MB", "GB", "TB" }; // , "PB", "EB", "ZB", "YB"

        public static string GetMemoryString(double bytes)
        {
            int counter = 0;
            double value = bytes;
            string text = "";
            do
            {
                text = value.ToString("0.0") + " " + BinaryPrefix[counter];
                value /= 1024;
                counter++;
            }
            while (Math.Floor(value) > 0 && counter < BinaryPrefix.Length);
            return text;
        }
    }
}
