using System;
using System.Windows.Media.Imaging;
using Amazon.S3.Model;
using S3Browser.Utils;

namespace S3Browser.Model
{
    class S3Objects
    {
        public BitmapFrame Image { get; set; }

        public string Type { get; set; }

        public string BucketName { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public Owner Owner { get; set; }

        public string Size { get; set;  }

        public string StorageClass { get; set; }

        public string GetFileName()
        {
            var keys = this.Key.Split('/');
            return keys[keys.Length - 1];
        }

        public void SetSize(long size)
        {
            this.Size = ResourceHelper.GetMemoryString(size);
        }
    }
}
