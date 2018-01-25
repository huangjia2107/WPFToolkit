using System.Text.RegularExpressions; 

namespace Utils.Common
{
    public static class StringUtil
    {
        public static string RemoveLineBreak(this string originString)
        {
            return Regex.Replace(originString, @"[\n\r]", "");
        }
        
         
      
    }
}
