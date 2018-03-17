using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressionJMPEG
{
    /// <summary>
    /// Handle all saving and loading related tasks
    /// </summary>
    class SaveAndLoad
    {
        /// <summary>
        /// prepare and return save byte array for custom jpeg compressed image
        /// </summary>
        /// <param name="jpegSaveInfo">JPEGSaveInfo object</param>
        /// <returns>Arraysaved byte array</returns>
        public static byte[] saveIntoByteArray(JPEGInfo jpegSaveInfo)
        {
            byte[] widthByteArray = BitConverter.GetBytes(jpegSaveInfo.originalWidth);
            byte[] heightByteArray = BitConverter.GetBytes(jpegSaveInfo.originalHeight);
            byte[] compressedByteArray = new byte[widthByteArray.Length + heightByteArray.Length +
                jpegSaveInfo.qYCrCb.Y.Length + jpegSaveInfo.qYCrCb.Cr.Length + jpegSaveInfo.qYCrCb.Cb.Length];
            System.Buffer.BlockCopy(widthByteArray, 0, compressedByteArray, 0, widthByteArray.Length);
            System.Buffer.BlockCopy(heightByteArray, 0, compressedByteArray, widthByteArray.Length, heightByteArray.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.qYCrCb.Y, 0, compressedByteArray, widthByteArray.Length + heightByteArray.Length,
                                    jpegSaveInfo.qYCrCb.Y.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.qYCrCb.Cr, 0, compressedByteArray, widthByteArray.Length + heightByteArray.Length
                                    + jpegSaveInfo.qYCrCb.Y.Length, jpegSaveInfo.qYCrCb.Cr.Length);
            System.Buffer.BlockCopy(jpegSaveInfo.qYCrCb.Cb, 0, compressedByteArray, widthByteArray.Length + heightByteArray.Length
                                    + jpegSaveInfo.qYCrCb.Y.Length + jpegSaveInfo.qYCrCb.Cr.Length, jpegSaveInfo.qYCrCb.Cb.Length);
            compressedByteArray = RLCompression.ModifiedRunLengthCompression(compressedByteArray);
            return compressedByteArray;
        }

        /// <summary>
        /// prepare and return save byte array for custom mpeg compressed images
        /// </summary>
        /// <param name="mpegSaveInfo">mpeg save info</param>
        /// <returns>saved byte array</returns>
        public static byte[] saveIntoByteArray(MPEGInfo mpegSaveInfo)
        {
            byte[] widthByteArray = BitConverter.GetBytes(mpegSaveInfo.originalWidth);
            byte[] heightByteArray = BitConverter.GetBytes(mpegSaveInfo.originalHeight);
            YCrCb[] iFrames = mpegSaveInfo.iFrames;
            PFrame[] pFrames = mpegSaveInfo.pFrames;
            int iFramesSpace = iFrames.Length * (iFrames[0].Y.Length + iFrames[0].Cr.Length * 2);
            int pFramesSpace = pFrames.Length * (pFrames[0].MotionVectorsY.Length * 3 +
                pFrames[0].DiffBlock.Y.Length + pFrames[0].DiffBlock.Cr.Length * 2);

            byte[] compressedByteArray = new byte[widthByteArray.Length + 
                heightByteArray.Length + iFramesSpace + pFramesSpace];

            int offset = 0;
            int numOfFrame = iFrames.Length + pFrames.Length;

            System.Buffer.BlockCopy(widthByteArray, 0, compressedByteArray, 0, widthByteArray.Length);
            offset += widthByteArray.Length;
            System.Buffer.BlockCopy(heightByteArray, 0, compressedByteArray, offset, heightByteArray.Length);
            offset += heightByteArray.Length;

            int pFrameIndex = 0;
            for (int i = 0; i < numOfFrame; i++)
            {
                if (i % Compression.I_FRAME_RANGE == 0)
                {

                }
            }



            return null;
        }

        /// <summary>
        /// Load saved byte array into JPEG info format
        /// </summary>
        /// <param name="savedArray">saved byte array</param>
        /// <returns>JPEG info format of loaded file</returns>
        public static JPEGInfo loadByteArray(byte[] savedArray)
        {
            savedArray = RLCompression.ModifiedRunLengthDecompress(savedArray);
            byte[] widthByteArray = new byte[Compression.intToByteSize];
            byte[] heightByteArray = new byte[Compression.intToByteSize];
            System.Buffer.BlockCopy(savedArray, 0, widthByteArray, 0, 
                Compression.intToByteSize);
            System.Buffer.BlockCopy(savedArray, Compression.intToByteSize, 
                heightByteArray, 0, Compression.intToByteSize);

            int originalWidth = BitConverter.ToInt32(widthByteArray, 0);
            int originalHeight = BitConverter.ToInt32(heightByteArray, 0);
            int width = originalWidth + originalWidth % 8;
            int height = originalHeight + originalHeight % 8;

            int reducedWidth = (int)Math.Ceiling(originalWidth / 2.0);
            int reducedHeight = (int)Math.Ceiling(originalHeight / 2.0);
            reducedWidth += reducedWidth % 8;
            reducedHeight += reducedHeight % 8;

            byte[] qY = new byte[width * height];
            byte[] qCr = new byte[reducedWidth * reducedHeight];
            byte[] qCb = new byte[reducedWidth * reducedHeight];

            System.Buffer.BlockCopy(savedArray, Compression.intToByteSize * 2, qY, 0, qY.Length);
            System.Buffer.BlockCopy(savedArray, Compression.intToByteSize * 2 + qY.Length,
                                    qCr, 0, qCr.Length);
            System.Buffer.BlockCopy(savedArray, Compression.intToByteSize * 2 + qY.Length +
                                    qCr.Length, qCb, 0, qCb.Length);
            YCrCb qYCrCb = new YCrCb(qY, qCr, qCb, height, width, reducedHeight, reducedWidth);
            return new JPEGInfo(originalWidth, originalHeight, qYCrCb);
        }
    }
}
