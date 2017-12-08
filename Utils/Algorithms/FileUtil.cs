using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Algorithms
{
    public static class FileUtil
    {
        public static bool IsDirectory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            return ((File.GetAttributes(filePath) & FileAttributes.Directory) != 0);
        }

        public static bool CanAccessFile(string file, bool ignoreZeroSize = false)
        {
            if (file == null || string.IsNullOrEmpty(file.Trim()) || !File.Exists(file))
                throw new FileNotFoundException("The file is invalid. File = " + file);

            if (!ignoreZeroSize && (new FileInfo(file)).Length == 0)
            {
                Debug.WriteLine("The file size is zero, File = " + file);
                return false;
            }

            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                }
            }
            catch
            {
                Debug.WriteLine("The file is locked, File = " + file);
                return false;
            }

            return true;
        }
        
        public static void CopyDirectory(string sourcePath, string destinationPath, bool overwrite)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            if (!sourceDir.Exists)
                return;

            if (Directory.Exists(destinationPath) && overwrite)
                Directory.Delete(destinationPath, true);

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (FileInfo fileInfo in sourceDir.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                if ((overwrite || !File.Exists(destinationPath + "\\" + fileInfo.Name)) && fileInfo.Exists)
                    File.Copy(fileInfo.FullName, destinationPath + "\\" + fileInfo.Name, overwrite);
            }

            foreach (DirectoryInfo directoryInfo in sourceDir.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                CopyDirectory(directoryInfo.FullName, destinationPath + "\\" + directoryInfo.Name, overwrite);
            }
        }
    }
}
