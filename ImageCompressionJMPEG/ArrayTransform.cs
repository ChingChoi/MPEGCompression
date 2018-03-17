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

        /// <summary>
        /// Pad all channels with 0s that are not divisible by y or cr divider
        /// </summary>
        /// <param name="subYCrCb">Y Cr Cb channels</param>
        /// <param name="yDivider">y divider</param>
        /// <param name="crCbDivider">cr divider</param>
        /// <returns>padded channels</returns>
        public static YCrCb padChannels(YCrCb subYCrCb, int yDivider, int crCbDivider)
        {
            if (subYCrCb.yWidth % yDivider != 0 || subYCrCb.yHeight % yDivider != 0)
            {
                int rightPad = 0;
                if (subYCrCb.yWidth % yDivider != 0)
                {
                    rightPad = yDivider - (subYCrCb.yWidth % yDivider);
                }
                int bottomPad = 0;
                if (subYCrCb.yHeight % yDivider != 0)
                {
                    bottomPad = yDivider - (subYCrCb.yHeight % yDivider);
                }
                subYCrCb.Y = padChannel(subYCrCb.Y, subYCrCb.yWidth, subYCrCb.yHeight, yDivider);
                subYCrCb.yWidth += rightPad;
                subYCrCb.yHeight += bottomPad;
            }
            if (subYCrCb.crCbWidth % crCbDivider != 0 || 
                subYCrCb.crCbHeight % crCbDivider != 0)
            {
                subYCrCb.Cr = padChannel(subYCrCb.Cr, subYCrCb.crCbWidth, subYCrCb.crCbHeight, crCbDivider);
                subYCrCb.Cb = padChannel(subYCrCb.Cb, subYCrCb.crCbWidth, subYCrCb.crCbHeight, crCbDivider);
                if (subYCrCb.crCbWidth % crCbDivider != 0)
                {
                    subYCrCb.crCbWidth += crCbDivider - (subYCrCb.crCbWidth % crCbDivider);
                }
                if (subYCrCb.crCbHeight % crCbDivider != 0)
                {
                    subYCrCb.crCbHeight += crCbDivider - (subYCrCb.crCbHeight % crCbDivider);
                }
            }
            return subYCrCb;
        }

        /// <summary>
        /// Unpad all channels that were padded with 0 due to not divisible by y or cr divider
        /// </summary>
        /// <param name="iYCrCb">Y Cr Cb channels</param>
        /// <param name="yDivider">y channel pad divider</param>
        /// <param name="crCbDivider">cr channel pad divider</param>
        /// <returns>Unpadded channels</returns>
        public static YCrCb unpadChannels(YCrCb iYCrCb, int yDivider, int crCbDivider)
        {
            if (Compression.originalWidth % yDivider != 0 
                || Compression.originalHeight % yDivider != 0)
            {
                int rightPad = 0;
                if (Compression.originalWidth % yDivider != 0)
                {
                    rightPad = yDivider - (Compression.originalWidth % yDivider);
                }
                int bottomPad = 0;
                if (Compression.originalHeight % yDivider != 0)
                {
                    bottomPad = yDivider - (Compression.originalHeight % yDivider);
                }
                iYCrCb.Y = unpadChannel(iYCrCb.Y, Compression.originalWidth, 
                    Compression.originalHeight, yDivider);
                iYCrCb.yWidth -= rightPad;
                iYCrCb.yHeight -= bottomPad;
            }
            if ((int)Math.Ceiling(Compression.originalWidth / 2.0) % crCbDivider != 0 ||
                (int)Math.Ceiling(Compression.originalHeight / 2.0) % crCbDivider != 0)
            {
                iYCrCb.Cr = unpadChannel(iYCrCb.Cr, 
                    (int)Math.Ceiling(Compression.originalWidth / 2.0), 
                    (int)Math.Ceiling(Compression.originalHeight / 2.0), crCbDivider);
                iYCrCb.Cb = unpadChannel(iYCrCb.Cb, 
                    (int)Math.Ceiling(Compression.originalWidth / 2.0), 
                    (int)Math.Ceiling(Compression.originalHeight / 2.0), crCbDivider);
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
        /// <returns>padded channel</returns>
        public static byte[] padChannel(byte[] channel, int width, int height, int divider)
        {
            int rightPad = 0;
            if (width % divider != 0)
            {
                rightPad = divider - (width % divider);
            }
            int bottomPad = 0;
            if (height % divider != 0)
            {
                bottomPad = divider - (height % divider);
            }
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
        /// <returns>Original unpadded channel</returns>
        public static byte[] unpadChannel(byte[] paddedChannel, int width, int height, int divider)
        {
            int rightPad = 0;
            if (width % divider != 0)
            {
                rightPad = divider - (width % divider);
            }
            int bottomPad = 0;
            if (height % divider != 0)
            {
                bottomPad = divider - (height % divider);
            }
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

        /// <summary>
        /// Convert vector array to byte array
        /// </summary>
        /// <param name="vectors">vector array</param>
        /// <returns>byte array</returns>
        public static byte[] convertToByteFromVector(Vector[] vectors)
        {
            byte[] result = new byte[vectors.Length * 2];
            for (int i = 0, index = 0; i < vectors.Length; i++)
            {
                result[index++] = (byte)((sbyte)vectors[i].x);
                result[index++] = (byte)((sbyte)vectors[i].y);
            }
            return result;
        }

        /// <summary>
        /// Convert vectors in byte array into vector array
        /// </summary>
        /// <param name="vectorInByte">vectors in byte array</param>
        /// <returns>Vector array</returns>
        public static Vector[] convertToVectorFromByte(byte[] vectorInByte)
        {
            Vector[] vectors = new Vector[vectorInByte.Length / 2];
            for (int i = 0, index = 0; i < vectors.Length; i++) {
                vectors[i] = new Vector((sbyte)vectorInByte[index++], (sbyte)vectorInByte[index++]);
            }
            return vectors;
        }
    }
}
