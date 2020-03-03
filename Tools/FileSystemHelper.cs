using System;

namespace Yoloanno.Tools
{
    public static class FileSystemHelper
    {
        public static string GetFileName(string enteredNamePostfix, string extension = "jpg")
        {
            return string.Format("{0:D12}.{1}", Convert.ToInt32(enteredNamePostfix), extension);
        }

        public static string GetCheckedFilePath(string folder, string fileName)
        {
            var filePath = System.IO.Path.Combine(folder, fileName);
            if (System.IO.File.Exists(filePath))
            {
                return filePath;
            }
            return string.Empty;
        }
    }
}
