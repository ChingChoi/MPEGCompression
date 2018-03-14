﻿using System;
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
            public int originalWidth;
            public int originalHeight;
            public YCrCb qYCrCb;

            public JPEGSaveInfo(int originalWidth, int originalHeight, YCrCb qYCrCb)
            {
                this.originalWidth = originalWidth;
                this.originalHeight = originalHeight;
                this.qYCrCb = qYCrCb;
            }
        }

        /// <summary>
        /// prepare and return save byte array for custom jpeg compressed image
        /// </summary>
        /// <param name="jpegSaveInfo">JPEGSaveInfo object</param>
        /// <returns>Array</returns>
        public static byte[] saveIntoByteArray(JPEGSaveInfo jpegSaveInfo)
        {
            byte[] widthByteArray = BitConverter.GetBytes(jpegSaveInfo.originalWidth);
            byte[] heightByteArray = BitConverter.GetBytes(jpegSaveInfo.originalHeight);
            int backWidth = BitConverter.ToInt32(widthByteArray, 0);
            int backHeight = BitConverter.ToInt32(widthByteArray, 0);

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

        public static JPEGSaveInfo loadByteArray(byte[] savedArray)
        {
            savedArray = RLCompression.ModifiedRunLengthDecompress(savedArray);
            byte[] widthByteArray = new byte[Compression.intToByteSize];
            byte[] heightByteArray = new byte[Compression.intToByteSize];
            System.Buffer.BlockCopy(savedArray, 0, widthByteArray, 0, Compression.intToByteSize);
            System.Buffer.BlockCopy(savedArray, Compression.intToByteSize, heightByteArray, 0, Compression.intToByteSize);

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
            return new JPEGSaveInfo(originalWidth, originalHeight, qYCrCb);
        }
    }
}
