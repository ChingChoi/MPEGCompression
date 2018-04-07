using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressionJMPEG
{
    class Wavelet
    {
        const int INT_SIZE = 4;
        public static Bitmap CompressWithWavelet(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            int[][] imageInt = new int[height][];
            for (int i = 0; i < height; i++)
            {
                imageInt[i] = new int[width];
                for (int j = 0; j < width; j++)
                {
                    Color c = image.GetPixel(i, j);
                    imageInt[i][j] = (c.R + c.G + c.B) / 3;
                }
            }

            //imageInt = new int[4][];
            //imageInt[0] = new int[4]
            //{
            //    9, 7, 5, 3,
            //};
            //imageInt[1] = new int[4]
            //{
            //    3, 5, 7, 9,
            //};
            //imageInt[2] = new int[4]
            //{
            //    2, 4, 6, 8,
            //};
            //imageInt[3] = new int[4]
            //{
            //    4, 6, 8, 10
            //};
    

            int[][] result = encode2D(imageInt);
            result = DropDetail(result);
            int[][] reverse = decode2D(result);

            Bitmap resultBitmap = new Bitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int value = reverse[i][j];
                    if (value > 255)
                    {
                        value = 255;
                    }
                    if (value < 0)
                    {
                        value = 0;
                    }
                    Color pixel = Color.FromArgb(value, value, value);
                    resultBitmap.SetPixel(i, j, pixel);
                }
            }
            return resultBitmap;
        }

        public static int[] waveletCompress(int[] data, int length)
        {
            if (length == 1)
            {
                return data;
            }
            int[] result = new int[data.Length];
            System.Buffer.BlockCopy(data, 0, result, 0, data.Length * INT_SIZE);
            for (int i = 0, index = 0; i < length; i+= 2, index++)
            {
                int sum = (data[i] + data[i + 1]) / 2;
                int difference = (data[i] - data[i + 1]) / 2;
                result[index] = sum;
                result[index + length / 2] = difference;
            }
            return result;
        }

        public static int[] join(int[] a, int[] b)
        {
            int[] results = new int[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, results, 0, a.Length * INT_SIZE);
            System.Buffer.BlockCopy(b, 0, results, a.Length, b.Length * INT_SIZE);
            return results;
        }

        public static int[] waveletDecompress(int[] data, int length)
        {
            int[] result = new int[data.Length];
            System.Buffer.BlockCopy(data, 0, result, 0, data.Length * INT_SIZE);
            if (length == 1)
            {
                result[0] = data[0] + data[1];
                result[0 + 1] = data[0] - data[1];
            }
            for (int i = 0, index = 0; i < length; i++, index += 2)
            {
                result[index] = data[i] + data[i + length];
                result[index + 1] = data[i] - data[i + length];
            }
            return result;
        }

        public static int[] merge(int[] a, int[] b)
        {
            int[] result = new int[a.Length + b.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i * 2] = a[i];
                result[i * 2 + 1] = b[i];
            }
            return result;
        }

        public static int[] dualLiftingStepF(int[] s, int[] d)
        {
            int[] result = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
                result[i] = d[i] - s[i];
            return result;
        }

        public static int[] primalLiftingStepF(int[] s, int[] d)
        {
            int[] result = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                result[i] = s[i] + (int)Math.Floor(d[i] / 2.0 + 0.5);
            }

            return result;
        }

        public static int[] primalLiftingStepB(int[] s, int[] d)
        {
            int[] result = new int[s.Length];

            for (int i = 0; i < s.Length; i++)
                result[i] = s[i] - (int)Math.Floor(d[i] / 2.0 + 0.5);

            return result;
        }

        public static int[] dualLiftingStepB(int[] s, int[] d)
        {
            int[] result = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
                result[i] = d[i] + s[i];
            return result;
        }

        public static int[][] transpose(int[][] a)
        {
            int[][] result = new int[a.Length][];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = new int[a[i].Length];
                for (int j = 0; j < a.Length; j++)
                {
                    result[i][j] = a[j][i];
                }
            }
            return result;
        }

        public static int[][] encode2D(int[][] data)
        {
            int iteration = 1;
            int[][] result = new int[data.Length][];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = new int[data[i].Length];
                System.Buffer.BlockCopy(data[i], 0, result[i], 0, result[i].Length * INT_SIZE);
            }

            while (data.Length/iteration > 1)
            {
                for(int row = 0; row < data.Length/iteration; row++)
                {
                    result[row] = waveletCompress(result[row], data.Length / iteration);
                }
                result = transpose(result);
                for (int row = 0; row < data.Length/(iteration*2); row++)
                {
                    result[row] = waveletCompress(result[row], data.Length / iteration);
                }
                iteration *= 2;
                result = transpose(result);
            }
            return result;
        }

        public static int[][] decode2D(int[][] data)
        {
            int iteration = data.Length;
            int length = data.Length / iteration;

            int[][] result = transpose(data);

            while (iteration > 1)
            {
                for (int row = 0; row < length; row++)
                {
                    result[row] = waveletDecompress(result[row], length);
                }

                result = transpose(result);

                for (int row = 0; row < length * 2; row++)
                {
                    result[row] = waveletDecompress(result[row], length);
                }
                
                result = transpose(result);

                iteration /= 2;
                length *= 2;
            }
            return transpose(result);
        }

        public static int[][] DropDetail(int[][] input)
        {
            int[][] result = new int[input.Length][];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = new int[input[i].Length];
                for (int j = 0; j < input[i].Length / 2; j++)
                {
                    result[i][j] = input[i][j];
                }
            }
            return result;
        }

        public static void printTwoDArray(int[][] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    Debug.Write(input[i][j] + " ");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        }
    }
}