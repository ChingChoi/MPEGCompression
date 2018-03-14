using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressionJMPEG
{
    class ArrayTransform
    {
        /// <summary>
        /// Transform byte array into double array
        /// </summary>
        /// <param name="byteArray">Input byte array</param>
        /// <returns>double array</returns>
        public static double[] byteArrayToDouble(byte[] byteArray)
        {
            double[] doubleArray = new double[byteArray.Length];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                doubleArray[i] = byteArray[i];
            }
            return doubleArray;
        }

        /// <summary>
        /// Transform double array into byte array
        /// </summary>
        /// <param name="doubleArray">input double array</param>
        /// <returns>byte array</returns>
        public static byte[] doubleArrayToByte(double[] doubleArray)
        {
            byte[] byteArray = new byte[doubleArray.Length];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                if (doubleArray[i] > 255)
                {
                    byteArray[i] = 255;
                }
                else if (doubleArray[i] < 0)
                {
                    byteArray[i] = 0;
                }
                else
                {
                    byteArray[i] = (byte)Math.Round(doubleArray[i]);
                }
            }
            return byteArray;
        }

        public static YCrCb padChannels(YCrCb subYCrCb)
        {
            if (subYCrCb.yWidth % 8 != 0 || subYCrCb.yHeight % 8 != 0)
            {
                int rightPad = subYCrCb.yWidth % 8;
                int bottomPad = subYCrCb.yHeight % 8;
                subYCrCb.Y = padChannel(subYCrCb.Y, subYCrCb.yWidth, subYCrCb.yHeight);
                subYCrCb.yWidth += rightPad;
                subYCrCb.yHeight += bottomPad;
            }
            if ((int)Math.Ceiling(Compression.originalWidth / 2.0) % 8 != 0 || (int)Math.Ceiling(Compression.originalHeight / 2.0) % 8 != 0)
            {
                subYCrCb.Cr = padChannel(subYCrCb.Cr, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0));
                subYCrCb.Cb = padChannel(subYCrCb.Cb, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0));
                subYCrCb.crCbWidth += (int)Math.Ceiling(Compression.originalWidth / 2.0) % 8;
                subYCrCb.crCbHeight += (int)Math.Ceiling(Compression.originalHeight / 2.0) % 8;
            }
            return subYCrCb;
        }

        public static YCrCb unpadChannels(YCrCb iYCrCb)
        {
            if (Compression.originalWidth % 8 != 0 || Compression.originalHeight % 8 != 0)
            {
                int rightPad = Compression.originalWidth % 8;
                int bottomPad = Compression.originalHeight % 8;
                iYCrCb.Y = unpadChannel(iYCrCb.Y, Compression.originalWidth, Compression.originalHeight);
                iYCrCb.yWidth -= rightPad;
                iYCrCb.yHeight -= bottomPad;
            }
            if ((int)Math.Ceiling(Compression.originalWidth / 2.0) % 8 != 0 || (int)Math.Ceiling(Compression.originalHeight / 2.0) % 8 != 0)
            {
                iYCrCb.Cr = unpadChannel(iYCrCb.Cr, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0));
                iYCrCb.Cb = unpadChannel(iYCrCb.Cb, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0));
                iYCrCb.crCbWidth = (int)Math.Ceiling(Compression.originalWidth / 2.0);
                iYCrCb.crCbHeight = (int)Math.Ceiling(Compression.originalHeight / 2.0);
            }
            return iYCrCb;
        }

        public static byte[] padChannel(byte[] channel, int width, int height)
        {
            int rightPad = width % 8;
            int bottomPad = height % 8;
            byte[] paddedChannel = new byte[(width + rightPad) * (height + bottomPad)];
            int padIndex = 0;
            int channelIndex = 0;
            for (int j = 0; j < height + bottomPad; j++)
            {
                for (int i = 0; i < width + rightPad; i++)
                {
                    if (i < width && j < height)
                    {
                        paddedChannel[padIndex++] = channel[channelIndex++];
                    }
                    else
                    {
                        paddedChannel[padIndex++] = 0;
                    }
                }
            }
            return paddedChannel;
        }

        public static byte[] unpadChannel(byte[] paddedChannel, int width, int height)
        {
            int rightPad = width % 8;
            int bottomPad = height % 8;
            byte[] unpaddedChannel = new byte[width * height];
            int unpadIndex = 0;
            int channelIndex = 0;
            for (int j = 0; j < height + rightPad; j++)
            {
                for (int i = 0; i < width + rightPad; i++)
                {
                    if (i < width && j < height)
                    {
                        unpaddedChannel[unpadIndex++] = paddedChannel[channelIndex];
                    }
                    channelIndex++;
                }
            }
            return unpaddedChannel;
        }

        public static byte[] increaseCapacity(byte[] input)
        {
            byte[] output = new byte[input.Length * 2];
            System.Buffer.BlockCopy(input, 0, output, 0, input.Length);
            return output;
        }
    }
}
