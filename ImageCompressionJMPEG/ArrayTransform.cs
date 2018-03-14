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

        public static YCrCb padChannels(YCrCb subYCrCb, int divider)
        {
            if (subYCrCb.yWidth % divider != 0 || subYCrCb.yHeight % divider != 0)
            {
                int rightPad = subYCrCb.yWidth % 8;
                int bottomPad = subYCrCb.yHeight % 8;
                subYCrCb.Y = padChannel(subYCrCb.Y, subYCrCb.yWidth, subYCrCb.yHeight, divider);
                subYCrCb.yWidth += rightPad;
                subYCrCb.yHeight += bottomPad;
            }
            if ((int)Math.Ceiling(Compression.originalWidth / 2.0) % 8 != 0 || (int)Math.Ceiling(Compression.originalHeight / 2.0) % 8 != 0)
            {
                subYCrCb.Cr = padChannel(subYCrCb.Cr, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0), divider);
                subYCrCb.Cb = padChannel(subYCrCb.Cb, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0), divider);
                subYCrCb.crCbWidth += (int)Math.Ceiling(Compression.originalWidth / 2.0) % divider;
                subYCrCb.crCbHeight += (int)Math.Ceiling(Compression.originalHeight / 2.0) % divider;
            }
            return subYCrCb;
        }

        public static YCrCb unpadChannels(YCrCb iYCrCb, int divider)
        {
            if (Compression.originalWidth % 8 != 0 || Compression.originalHeight % 8 != 0)
            {
                int rightPad = Compression.originalWidth % 8;
                int bottomPad = Compression.originalHeight % 8;
                iYCrCb.Y = unpadChannel(iYCrCb.Y, Compression.originalWidth, Compression.originalHeight, divider);
                iYCrCb.yWidth -= rightPad;
                iYCrCb.yHeight -= bottomPad;
            }
            if ((int)Math.Ceiling(Compression.originalWidth / 2.0) % 8 != 0 || (int)Math.Ceiling(Compression.originalHeight / 2.0) % 8 != 0)
            {
                iYCrCb.Cr = unpadChannel(iYCrCb.Cr, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0), divider);
                iYCrCb.Cb = unpadChannel(iYCrCb.Cb, (int)Math.Ceiling(Compression.originalWidth / 2.0), (int)Math.Ceiling(Compression.originalHeight / 2.0), divider);
                iYCrCb.crCbWidth = (int)Math.Ceiling(Compression.originalWidth / 2.0);
                iYCrCb.crCbHeight = (int)Math.Ceiling(Compression.originalHeight / 2.0);
            }
            return iYCrCb;
        }

        /// <summary>
        /// Pad channel to be divisible by divider
        /// </summary>
        /// <param name="channel">input channel</param>
        /// <param name="width">channel width</param>
        /// <param name="height">channel height</param>
        /// <param name="divider">divider</param>
        /// <returns></returns>
        public static byte[] padChannel(byte[] channel, int width, int height, int divider)
        {
            int rightPad = width % divider;
            int bottomPad = height % divider;
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

        /// <summary>
        /// Unpad a channel to original width and height
        /// </summary>
        /// <param name="paddedChannel">padded channel</param>
        /// <param name="width">original channel width</param>
        /// <param name="height">original channel height</param>
        /// <param name="divider">divider</param>
        /// <returns></returns>
        public static byte[] unpadChannel(byte[] paddedChannel, int width, int height, int divider)
        {
            int rightPad = width % divider;
            int bottomPad = height % divider;
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

        /// <summary>
        /// Increases the capacity of input byte array to twice its original
        /// </summary>
        /// <param name="input">input byte array</param>
        /// <returns>byte array with twice the capacity</returns>
        public static byte[] increaseCapacity(byte[] input)
        {
            byte[] output = new byte[input.Length * 2];
            System.Buffer.BlockCopy(input, 0, output, 0, input.Length);
            return output;
        }
    }
}
