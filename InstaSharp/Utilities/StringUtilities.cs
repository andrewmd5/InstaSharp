using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace InstaSharp.Utilities
{
    public class StringUtilities
    {
        public static string GetRandomString()
        {
            var path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }

        public static string GenerateTimeStamp()
        {
            var strGenerateTimeStamp = string.Empty;
            try
            {
                // Default implementation of UNIX time of the current UTC time
                var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                strGenerateTimeStamp = Convert.ToInt64(ts.TotalSeconds).ToString();
            }
            catch (Exception ex)
            {
                // ignored
            }
            return strGenerateTimeStamp;
        }

        public static string SecureStringToString(SecureString value)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}