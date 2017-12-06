using System;
using System.Collections.Generic;
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
    }
}
