using System;
using System.Text;

namespace EmailsImporter.Services.Google
{
    public static class Base64Extensions
    {
        /// <summary>
        /// Convert Base64 encoded string to UTF8 format.
        /// </summary>
        public static string ToUTF8Format(this string base64Txt)
        {
            var encodedTxt = GetEncodedTxt(base64Txt);

            byte[] bytes = Convert.FromBase64String(encodedTxt);

            return Encoding.UTF8.GetString(bytes);
        }

        private static string GetEncodedTxt(string base64Txt)
        {
            // Replace all special characters
            var encodedTxt = base64Txt.Replace("-", "+");
            encodedTxt = encodedTxt.Replace("_", "/");
            encodedTxt = encodedTxt.Replace(" ", "+");
            encodedTxt = encodedTxt.Replace("=", "+");

            // Fixed invalid length
            if (encodedTxt.Length % 4 > 0) { encodedTxt += new string('=', 4 - encodedTxt.Length % 4); }
            else if (encodedTxt.Length % 4 == 0)
            {
                encodedTxt = encodedTxt.Substring(0, encodedTxt.Length - 1);
                if (encodedTxt.Length % 4 > 0) { encodedTxt += new string('+', 4 - encodedTxt.Length % 4); }
            }

            return encodedTxt;
        }

        /// <summary>
        /// Convert Base64 encoded string to bytes.
        /// </summary>
        /// <param name="base64Txt"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string base64Txt)
        {
            var encodedTxt = GetEncodedTxt(base64Txt);
            return Convert.FromBase64String(encodedTxt);
        }
    }
}