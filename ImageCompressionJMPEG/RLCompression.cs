﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressionJMPEG
{
    class RLCompression
    {
        private static byte key = 254;

        /// <summary>
        /// Compresses byte array using modified run length encoding
        /// </summary>
        /// <param name="original">original uncompressed data</param>
        /// <returns>compressed byte array</returns>
        public static byte[] ModifiedRunLengthCompression(byte[] original)
        {
            if (original == null || original.Length < 3)
            {
                return original;
            }
            int count = 0;
            int currentPos = 1;
            byte runLength = 2;
            byte current = original[1];
            byte previous = original[0];
            byte[] temp = new byte[original.Length * 2];
            while (currentPos < original.Length - 1)
            {
                while (current == previous && currentPos < original.Length - 1 && runLength < byte.MaxValue)
                {
                    currentPos++;
                    runLength++;
                    current = original[currentPos];
                }
                if (previous == current && runLength == byte.MaxValue)
                {
                    temp[count++] = key;
                    temp[count++] = runLength;
                    temp[count++] = previous;
                    currentPos++;
                    if (currentPos < original.Length)
                    {
                        current = original[currentPos];
                    }
                }
                else if (previous == key)
                {
                    temp[count++] = key;
                    temp[count++] = (byte)(runLength - 1);
                    temp[count++] = key;
                }
                else if (runLength < 3)
                {
                    for (int i = 1; i < runLength; i++)
                    {
                        temp[count++] = previous;
                    }
                }
                else
                {
                    temp[count++] = key;
                    temp[count++] = (byte)(runLength - 1);
                    temp[count++] = previous;
                }
                previous = current;
                runLength = 1;
            }
            if (temp[count - 3] != key)
            {
                temp[count++] = previous;
            }
            else
            {
                temp[count - 2] += (byte)1;
            }

            byte[] compressed = new byte[count];
            for (int i = 0; i < count; i++)
            {
                compressed[i] = temp[i];
            }
            return compressed;
        }

        /// <summary>
        /// Decompress byte array using modified run length decoding
        /// </summary>
        /// <param name="compressedBuffer">Buffer compressed using MRLE</param>
        /// <returns>decompressed byte array</returns>
        public static byte[] ModifiedRunLengthDecompress(byte[] compressedBuffer)
        {
            int count = 0;
            int currentPosC = 0;
            //int match = 0;
            byte current = 0;
            byte runLength = 0;

            byte[] temp = new byte[compressedBuffer.Length * 10];

            for (int i = 0; i < compressedBuffer.Length; i++)
            {
                if (currentPosC >= compressedBuffer.Length)
                {
                    i = compressedBuffer.Length;
                }
                else
                {
                    current = compressedBuffer[currentPosC++];
                    if (current == key)
                    {
                        runLength = compressedBuffer[currentPosC++];
                        for (int j = 0; j < runLength; j++)
                        {
                            temp[count++] = compressedBuffer[currentPosC];
                        }
                        currentPosC++;
                    }
                    else
                    {
                        temp[count++] = current;
                    }
                }
            }

            byte[] uncompressed = new byte[count];
            for (int i = 0; i < count; i++)
            {
                uncompressed[i] = temp[i];
            }
            return uncompressed;
        }

    }
}
