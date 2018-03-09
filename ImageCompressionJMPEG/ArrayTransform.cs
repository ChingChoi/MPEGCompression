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
    }
}
