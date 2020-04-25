using CashierLibrary.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CashierLibrary.Utill
{
    public class ImgToolUtill
    {

        /// <summary>Convert bitmap to Base64 String
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static String Jpeg2String(Bitmap bitmap)
        {
            using (Stream stream = new System.IO.MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Seek(0, SeekOrigin.Begin);
                Byte[] picData = new byte[stream.Length];
                stream.Read(picData, 0, picData.Length);
                String sbmp = System.Convert.ToBase64String(picData);
                return sbmp;
            }
        }

        /// <summary> Convert base64 String to Jpeg
        /// </summary>
        /// <param name="base64Str"></param>
        /// <returns></returns>
        public static Bitmap GetBitMap(String base64Str)
        {
            try
            {
                byte[] b = Convert.FromBase64String(base64Str);
                MemoryStream ms = new MemoryStream(b);
                Bitmap bitmap = new Bitmap(ms);
                return bitmap;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("字符串转bitmap出错", ex);
            }
            return null;
        }
    }
}
