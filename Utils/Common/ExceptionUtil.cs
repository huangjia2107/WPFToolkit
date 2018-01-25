using System; 
using System.Text; 

namespace Utils.Common
{
    public static class ExceptionUtil
    {
        public static string StackTraceEx(this Exception exception)
        {
            if (exception == null)
                return string.Empty;

            int HResult = (int)(exception.GetType().GetProperty("HResult", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(exception, null));
            return string.Format("ErrorCode = {0} [ 0x{1:X} ]\nMessage = {2}\nStackTrace = {3}\n\r", HResult, HResult, exception.Message.RemoveLineBreak(), exception.StackTrace) + exception.AggregateStackTraces();
        }
        
        public static string AggregateStackTraces(this Exception exception)
        {
            if (exception.InnerException == null)
                return string.Empty;

            var tempEx = exception.InnerException;
            
            var stringBuilder = new StringBuilder();
            int i = 1;
            
            while (tempEx != null)
            {
                stringBuilder.AppendLine(string.Format("=================== InnerException[{0}] ===================", i++));
                stringBuilder.AppendLine(string.Format("Message = {0}\nStackTrace = {1}\n", tempEx.Message.RemoveLineBreak(), tempEx.StackTrace));

                tempEx = tempEx.InnerException;
            }

            return stringBuilder.ToString();
        }
        
        public static Exception LastInnerException(this Exception exception)
        {
            if (exception == null)
                return null;

            if (exception.InnerException == null)
                return exception;

            var tempEx = exception.InnerException;

            while (tempEx != null)
            {
                if (tempEx.InnerException == null)
                    break;

                tempEx = tempEx.InnerException;
            }

            return tempEx;
        }
      
    }
}
