using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressionJMPEG
{
    class SaveAndLoad
    {
        /// <summary>
        /// Holds all information required for saving a custom jpeg compressed image
        /// </summary>
        public struct JPEGSaveInfo
        {
            public byte[] widthByteArray;
            public byte[] heightByteArray;
            public YCrCb qYCrCb;

            public JPEGSaveInfo(byte[] widthByteArray, byte[] heightByteArray, YCrCb qYCrCb)
            {
                this.widthByteArray = widthByteArray;
                this.heightByteArray = heightByteArray;
                this.qYCrCb = qYCrCb;
            }
        }

        /// <summary>
        /// prepare and return save byte array for custom jpeg compressed image
        /// </summary>
        /// <param name="jpegSaveInfo">JPEGSaveInfo object</param>
        /// <returns>Array</returns>
        public static byte[] prepareSave(JPEGSaveInfo jpegSaveInfo)
        {
            byte[] compressedByteArray = new byte[jpegSaveInfo.widthByteArray.Length + jpegSaveInfo.heightByteArray.Length +
                                  jpegSaveInfo.qYCrCb.Y.Length + jpegSaveInfo.qYCrCb.Cr.Length + jpegSaveInfo.qYCrCb.Cb.Length];
            System.Buffer.BlockCopy(jpegSaveInfo.widthByteArray, 0, compressedByteArray, 0, jpegSaveInfo.widthByteArray.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.heightByteArray, 0, compressedByteArray, jpegSaveInfo.widthByteArray.Length, jpegSaveInfo.heightByteArray.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.qYCrCb.Y, 0, compressedByteArray, jpegSaveInfo.widthByteArray.Length + jpegSaveInfo.heightByteArray.Length,
                                    jpegSaveInfo.qYCrCb.Y.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.qYCrCb.Cr, 0, compressedByteArray, jpegSaveInfo.widthByteArray.Length + jpegSaveInfo.heightByteArray.Length
                                    + jpegSaveInfo.qYCrCb.Y.Length, jpegSaveInfo.qYCrCb.Cr.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.qYCrCb.Cb, 0, compressedByteArray, jpegSaveInfo.widthByteArray.Length + jpegSaveInfo.heightByteArray.Length
                                    + jpegSaveInfo.qYCrCb.Y.Length + jpegSaveInfo.qYCrCb.Cr.Length, jpegSaveInfo.qYCrCb.Cb.Length);
            compressedByteArray = RLCompression.ModifiedRunLengthCompression(compressedByteArray);
            return compressedByteArray;
        }
    }
}
