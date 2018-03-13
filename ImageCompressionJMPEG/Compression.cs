using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressionJMPEG
{
    /// <summary>
    /// Structure for holding Y Cr Cb channels
    /// </summary>
    public struct YCrCb
    {
        public byte[] Y;
        public byte[] Cr;
        public byte[] Cb;

        /// <summary>
        /// Constructor for Y Cr Cb structure
        /// </summary>
        /// <param name="Y">Y channel</param>
        /// <param name="Cr">Cr channel</param>
        /// <param name="Cb">Cb channel</param>
        public YCrCb(byte[] Y, byte[] Cr, byte[] Cb)
        {
            this.Y = Y;
            this.Cr = Cr;
            this.Cb = Cb;
        }
    }
    
    /// <summary>
    /// Structure for holding double format Y Cr Cb channels
    /// </summary>
    public struct DYCrCb
    {
        public double[] Y;
        public double[] Cr;
        public double[] Cb;

        /// <summary>
        /// Constructor for double Y Cr Cb structure
        /// </summary>
        /// <param name="Y">Y channel</param>
        /// <param name="Cr">Cr channel</param>
        /// <param name="Cb">Cb channel</param>
        public DYCrCb(double[] Y, double[] Cr, double[] Cb)
        {
            this.Y = Y;
            this.Cr = Cr;
            this.Cb = Cb;
        }
    }

    /// <summary>
    /// Structure for holding a two-dimensional vector
    /// </summary>
    public struct Vector
    {
        public int x;
        public int y;

        public Vector(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// Structure that holds information to prepare for MPEG compression
    /// </summary>
    public struct MPEGPrep
    {
        Vector[] motionVectorsY;
        Vector[] motionVectorsCr;
        Vector[] motionVectorsCb;
        YCrCb mYCrCb;

        public MPEGPrep(Vector[] motionVectorsY, Vector[] motionVectorsCr, Vector[] motionVectorsCb, YCrCb mYCrCb)
        {
            this.motionVectorsY = motionVectorsY;
            this.motionVectorsCr = motionVectorsCr;
            this.motionVectorsCb = motionVectorsCb;
            this.mYCrCb = mYCrCb;
        }

        public Vector[] MotionVectorsY { get => motionVectorsY; set => motionVectorsY = value; }
        public Vector[] MotionVectorsCr { get => motionVectorsCr; set => motionVectorsCr = value; }
        public Vector[] MotionVectorsCb { get => motionVectorsCb; set => motionVectorsCb = value; }
        public YCrCb MYCrCb { get => mYCrCb; set => mYCrCb = value; }
    }

    /// <summary>
    /// Handles compression of image using JPEG and MPEG
    /// </summary>
    class Compression
    {
        /// <summary>
        /// Kr value used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double KR = 0.299;

        /// <summary>
        /// Kg value used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double KG = 0.587;

        /// <summary>
        /// Kb value used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double KB = 0.114;

        /// <summary>
        /// Constant used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double CBR = 0.168736;

        /// <summary>
        /// Constant used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double CBG = 0.331264;

        /// <summary>
        /// Constant used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double CBB = 0.5;

        /// <summary>
        /// Constant used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double CRR = 0.5;

        /// <summary>
        /// Constant used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double CRG = 0.418688;

        /// <summary>
        /// Constant used to convert RGB to Y, Cr and Cb
        /// </summary>
        private static double CRB = 0.081312;

        /// <summary>
        /// C value for DCT when it is the first row or column
        /// </summary>
        private static double C_ZERO = 0.707106781;

        /// <summary>
        /// C value for DCT when it is not the first row or column
        /// </summary>
        private static double C_NONZERO = 1;

        /// <summary>
        /// Quantization table
        /// </summary>
        private static double[] quantizationTableJPEG = new double[64]
        {
            16, 11, 10, 16, 24, 40, 51, 61,
            12, 12, 14, 19, 26, 58, 60, 55,
            14, 13, 16, 24, 40, 57, 69, 56,
            14, 17, 22, 29, 51, 87, 80, 62,
            18, 22, 37, 56, 68, 109, 103, 77,
            24, 35, 55, 64, 81, 104, 113, 92,
            49, 64, 78, 87, 103, 121, 120, 101,
            72, 92, 95, 98, 112, 100, 103, 99
        };

        private static double[] quantizationTableMPEG = new double[64]
        {
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8
        };

        /// <summary>
        /// Upper search area range limit
        /// </summary>
        public const int UPPER_SEARCH_RANGE = 15;

        /// <summary>
        /// Lower search area range limit
        /// </summary>
        public const int LOWER_SEARCH_RANGE = 1;

        /// <summary>
        /// Search area for motion vector
        /// </summary>
        private static int searchArea = UPPER_SEARCH_RANGE;

        /// <summary>
        /// Compressed byte array with width and height information
        /// </summary>
        static byte[] compressedByteArray;

        public  static Bitmap displayBitmap;

        /// <summary>
        /// Byte array size after converting from an int
        /// </summary>
        static int intToByteSize = 4;

        /// <summary>
        /// Enable or disable zigzag
        /// </summary>
        private static bool zigzag = true;

        /// <summary>
        /// If image one is JPEG or not
        /// </summary>
        public static bool JPEGOne = false;

        /// <summary>
        /// If image two is JPEG or not
        /// </summary>
        public static bool JPEGTwo = false;

        public static int macroSizeY = 16;

        public static int macroSizeCrCb = 8;

        public static int numOfFrame = 0;

        /// <summary>
        /// Original width of the image
        /// </summary>
        public static int originalWidth = 0;

        /// <summary>
        /// Original height of the image
        /// </summary>
        public static int originalHeight = 0;

        public static double[] currentQuantizationTable;
        public static int SearchArea { get => searchArea; set => searchArea = value; }

        /// <summary>
        /// Compress image into JPEG format
        /// </summary>
        /// <param name="bitmap">Original bitmap</param>
        /// <param name="width">Original bitmap width</param>
        /// <param name="height">Original bitmap height</param>
        /// <returns></returns>
        public static Bitmap JPEGCompression(Bitmap bitmap, int width, int height)
        {
            originalHeight = height;
            originalWidth = width;
            currentQuantizationTable = quantizationTableJPEG;
            YCrCb yCrCb = convertToYCrCb(bitmap);
            if (width % 8 != 0 || height % 8 != 0)
            {
                int rightPad = width % 8;
                int bottomPad = height % 8;
                yCrCb.Y = padChannel(yCrCb.Y, width, height);
                yCrCb.Cr = padChannel(yCrCb.Cr, width, height);
                yCrCb.Cb = padChannel(yCrCb.Cb, width, height);
                width += rightPad;
                height += bottomPad;
            }
            YCrCb subYCrCb = subSample(yCrCb, width, height);
            YCrCb filledYCrCb = fillSubSample(subYCrCb, width, height);
            DYCrCb dctYCrCb = DiscreteCosineTransform(subYCrCb, width, height);
            YCrCb qYCrCb = QuantizationAndZigzag(dctYCrCb, width, height);
            DYCrCb iQYCrCb = InverseQuantizationAndZigzag(qYCrCb, width, height);
            YCrCb iYCrCb = InverseDiscreteCosineTransform(iQYCrCb, width, height);
            YCrCb fillediYCrCb = fillSubSample(iYCrCb, width, height);

            if (originalWidth % 8 != 0 || originalHeight % 8 != 0)
            {
                int rightPad = originalWidth % 8;
                int bottomPad = originalHeight % 8;
                fillediYCrCb.Y = unpadChannel(fillediYCrCb.Y, originalWidth, originalHeight);
                fillediYCrCb.Cr = unpadChannel(fillediYCrCb.Cr, originalWidth, originalHeight);
                fillediYCrCb.Cb = unpadChannel(fillediYCrCb.Cb, originalWidth, originalHeight);
                width -= rightPad;
                height -= bottomPad;
            }

            byte[] widthByteArray = BitConverter.GetBytes(originalWidth);
            byte[] heightByteArray = BitConverter.GetBytes(originalHeight);

            int backWidth = BitConverter.ToInt32(widthByteArray, 0);
            int backHeight = BitConverter.ToInt32(widthByteArray, 0);

            Bitmap result = convertToBitmap(fillediYCrCb, width, height);

            compressedByteArray = new byte[widthByteArray.Length + heightByteArray.Length +
                                  qYCrCb.Y.Length + qYCrCb.Cr.Length + qYCrCb.Cb.Length];
            System.Buffer.BlockCopy(widthByteArray, 0, compressedByteArray, 0, widthByteArray.Length);
            System.Buffer.BlockCopy(heightByteArray, 0, compressedByteArray, widthByteArray.Length, heightByteArray.Length);
            System.Buffer.BlockCopy(qYCrCb.Y, 0, compressedByteArray, widthByteArray.Length + heightByteArray.Length,
                                    qYCrCb.Y.Length);
            System.Buffer.BlockCopy(qYCrCb.Cr, 0, compressedByteArray, widthByteArray.Length + heightByteArray.Length
                                    + qYCrCb.Y.Length, qYCrCb.Cr.Length);
            System.Buffer.BlockCopy(qYCrCb.Cb, 0, compressedByteArray, widthByteArray.Length + heightByteArray.Length
                                    + qYCrCb.Y.Length + qYCrCb.Cr.Length, qYCrCb.Cb.Length);

            compressedByteArray = RLCompression.ModifiedRunLengthCompression(compressedByteArray);
            return result;
        }

        /// <summary>
        /// Decompress JPEG into bitmap
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        public static Bitmap JPEGDecompression(byte[] inputArray)
        {
            currentQuantizationTable = quantizationTableJPEG;
            inputArray = RLCompression.ModifiedRunLengthDecompress(inputArray);
            byte[] widthByteArray = new byte[intToByteSize];
            byte[] heightByteArray = new byte[intToByteSize];
            System.Buffer.BlockCopy(inputArray, 0, widthByteArray, 0, intToByteSize);
            System.Buffer.BlockCopy(inputArray, intToByteSize, heightByteArray, 0, intToByteSize);

            int width = BitConverter.ToInt32(widthByteArray, 0);
            int height = BitConverter.ToInt32(widthByteArray, 0);
            int reducedWidth = (int)(((double)width / 2.0));
            int reducedHeight = (int)(((double)height / 2.0));

            byte[] qY = new byte[width * height];
            byte[] qCr = new byte[reducedWidth * reducedHeight];
            byte[] qCb = new byte[reducedWidth * reducedHeight];

            System.Buffer.BlockCopy(inputArray, intToByteSize * 2, qY, 0, qY.Length);
            System.Buffer.BlockCopy(inputArray, intToByteSize * 2 + qY.Length,
                                    qCr, 0, qCr.Length);
            System.Buffer.BlockCopy(inputArray, intToByteSize * 2 + qY.Length +
                                    qCr.Length, qCb, 0, qCb.Length);
            YCrCb qYCrCb = new YCrCb(qY, qCr, qCb);
            DYCrCb iQYCrCb = InverseQuantizationAndZigzag(qYCrCb, width, height);
            YCrCb iYCrCb = InverseDiscreteCosineTransform(iQYCrCb, width, height);
            YCrCb fillediYCrCb = fillSubSample(iYCrCb, width, height);
            Bitmap result = convertToBitmap(fillediYCrCb, width, height);
            return result;
        }

        /// <summary>
        /// Motion vector for two frame and prepares for MPEG
        /// </summary>
        /// <param name="referenceArray">Reference frame</param>
        /// <param name="resultArray">Result frame</param>
        /// <returns>MPEGPrep structure</returns>
        public static MPEGPrep MPEGMotionVector(Bitmap reference, Bitmap current)
        {
            numOfFrame = 2;

            int width = reference.Width;
            int height = reference.Height;

            YCrCb rFilledIYCrCb = convertToYCrCb(reference);
            YCrCb rIYCrCb = subSample(rFilledIYCrCb, width, height);
            DYCrCb rdctYCrCb = DiscreteCosineTransform(rIYCrCb, width, height);
            YCrCb rQYCrCb = QuantizationAndZigzag(rdctYCrCb, width, height);

            YCrCb cYCrCb = convertToYCrCb(current);
            YCrCb cSubYCrCb = subSample(cYCrCb, width, height);

            Vector[] motionVectorsY = new Vector[(int)(Math.Ceiling((double)width / macroSizeY) * Math.Ceiling((double)height / macroSizeY))];
            Vector[] motionVectorsCr = new Vector[(int)(Math.Ceiling((double)width / macroSizeY) * Math.Ceiling((double)height / macroSizeY))];
            Vector[] motionVectorsCb = new Vector[motionVectorsCr.Length];
            double[] diffBlockY = new double[cYCrCb.Y.Length];
            double[] diffBlockCr = new double[cYCrCb.Cr.Length];
            double[] diffBlockCb = new double[cYCrCb.Cb.Length];
            int indexY = 0;
            int indexCr = 0;
            int indexCb = 0;
            for (int y = 0; y < height; y+= macroSizeCrCb)
            {
                for (int x = 0; x < width; x += macroSizeCrCb)
                {
                    Vector motionVectorY = new Vector(0, 0);
                    Vector motionVectorCr = new Vector(0, 0);
                    Vector motionVectorCb = new Vector(0, 0);
                    double currentMinY = Double.MaxValue;
                    double centralMinY = Double.MaxValue;
                    double currentMinCr = Double.MaxValue;
                    double centralMinCr = Double.MaxValue;
                    double currentMinCb = Double.MaxValue;
                    double centralMinCb = Double.MaxValue;
                    for (int i = -searchArea; i <= searchArea; i++)
                    {
                        for (int j = -searchArea; j <= searchArea; j++)
                        {
                            if (x + i >= 0 && y + j >= 0 && x + i + macroSizeY - 1 < width && y + j + macroSizeY - 1 < height)
                            {
                                if (x < width / 2 && y < height / 2 && x + i + macroSizeCrCb - 1 < height / 2 && y + j + macroSizeCrCb - 1 < width / 2)
                                {
                                    double tempMinCr;
                                    double tempMinCb;
                                    tempMinCr = MAD(macroSizeCrCb, x, y, i, j, cSubYCrCb.Cr, rIYCrCb.Cr, width / 2, height / 2);
                                    tempMinCb = MAD(macroSizeCrCb, x, y, i, j, cSubYCrCb.Cb, rIYCrCb.Cb, width / 2, height / 2);
                                    if (i == 0 && j == 0)
                                    {
                                        centralMinCr = tempMinCr;
                                        centralMinCb = tempMinCb;
                                    }
                                    if (tempMinCr < currentMinCr)
                                    {
                                        currentMinCr = tempMinCr;
                                        motionVectorCr.x = i;
                                        motionVectorCr.y = j;
                                    }
                                    if (tempMinCb < currentMinCb)
                                    {
                                        currentMinCb = tempMinCb;
                                        motionVectorCb.x = i;
                                        motionVectorCb.y = j;
                                    }
                                }
                                if (x % macroSizeY == 0 && y % macroSizeY == 0 && x + i - 1 < height && y + j - 1 < width)
                                {
                                    double tempMinY;
                                    tempMinY = MAD(macroSizeY, x, y, i, j, cSubYCrCb.Y, rIYCrCb.Y, width, height);
                                    if (i == 0 && j == 0)
                                    {
                                        centralMinY = tempMinY;
                                    }
                                    if (tempMinY < currentMinY)
                                    {
                                        currentMinY = tempMinY;
                                        motionVectorY.x = i;
                                        motionVectorY.y = j;
                                    }
                                }
                            }
                        }
                    }
                    if (x % macroSizeY == 0 && y % macroSizeY == 0)
                    {
                        if (Math.Round(currentMinY) == Math.Round(centralMinY))
                        {
                            motionVectorY.x = 0;
                            motionVectorY.y = 0;
                        }
                        motionVectorsY[indexY++] = motionVectorY;
                    }
                    if (x < width / 2 && y < height / 2)
                    {
                        if (currentMinCr == centralMinCr)
                        {
                            motionVectorCr.x = 0;
                            motionVectorCr.y = 0;
                        }
                        motionVectorsCr[indexCr++] = motionVectorCr;
                    }
                    if (x < width / 2 && y < height / 2)
                    {
                        if (currentMinCb == centralMinCb)
                        {
                            motionVectorCb.x = 0;
                            motionVectorCb.y = 0;
                        }
                        motionVectorsCb[indexCb++] = motionVectorCb;
                    }
                }
            }
            currentQuantizationTable = quantizationTableMPEG;
            diffBlockY = DiffBlock(motionVectorsY, rIYCrCb.Y, cSubYCrCb.Y, width, height, macroSizeY);
            diffBlockCr = DiffBlock(motionVectorsCr, rIYCrCb.Cr, cSubYCrCb.Cr, width / 2, height / 2, macroSizeCrCb);
            diffBlockCb = DiffBlock(motionVectorsCb, rIYCrCb.Cb, cSubYCrCb.Cb, width / 2, height / 2, macroSizeCrCb);
            DYCrCb mDiffBlocks = new DYCrCb(diffBlockY, diffBlockCr, diffBlockCb);
            DYCrCb dctMDiffBlocks = DiscreteCosineTransform(mDiffBlocks, width, height);
            YCrCb qMDiffBlocks = QuantizationAndZigzag(dctMDiffBlocks, width, height);

            byte[] widthByteArray = BitConverter.GetBytes(width);
            byte[] heightByteArray = BitConverter.GetBytes(height);

            int backWidth = BitConverter.ToInt32(widthByteArray, 0);
            int backHeight = BitConverter.ToInt32(widthByteArray, 0);

            int channelLength = rIYCrCb.Y.Length + rIYCrCb.Cr.Length + rIYCrCb.Cb.Length;
            int PFrameLength = (numOfFrame - 1) * ((motionVectorsY.Length + motionVectorsCr.Length + motionVectorsCb.Length)*2 + channelLength);
            int IFrameLength = widthByteArray.Length + heightByteArray.Length + channelLength;
            int compressedByteArrayLength = PFrameLength + IFrameLength;

            compressedByteArray = new byte[compressedByteArrayLength];
            int offset = 0;

            System.Buffer.BlockCopy(widthByteArray, 0, compressedByteArray, 0, widthByteArray.Length);
            offset += widthByteArray.Length;
            System.Buffer.BlockCopy(heightByteArray, 0, compressedByteArray, offset, heightByteArray.Length);
            offset += heightByteArray.Length;
            System.Buffer.BlockCopy(rQYCrCb.Y, 0, compressedByteArray, offset, rQYCrCb.Y.Length);
            offset += rQYCrCb.Y.Length;
            System.Buffer.BlockCopy(rQYCrCb.Cr, 0, compressedByteArray, offset, rQYCrCb.Cr.Length);
            offset += rQYCrCb.Cr.Length;
            System.Buffer.BlockCopy(rQYCrCb.Cb, 0, compressedByteArray, offset, rQYCrCb.Cb.Length);
            offset += rQYCrCb.Cb.Length;
            System.Buffer.BlockCopy(convertToByteFromVector(motionVectorsY), 0, compressedByteArray, offset, motionVectorsY.Length * 2);
            offset += motionVectorsY.Length * 2;
            System.Buffer.BlockCopy(convertToByteFromVector(motionVectorsCr), 0, compressedByteArray, offset, motionVectorsCr.Length * 2);
            offset += motionVectorsCr.Length * 2;
            System.Buffer.BlockCopy(convertToByteFromVector(motionVectorsCb), 0, compressedByteArray, offset, motionVectorsCb.Length * 2);
            offset += motionVectorsCb.Length * 2;
            System.Buffer.BlockCopy(qMDiffBlocks.Y, 0, compressedByteArray, offset, qMDiffBlocks.Y.Length);
            offset += qMDiffBlocks.Y.Length;
            System.Buffer.BlockCopy(qMDiffBlocks.Cr, 0, compressedByteArray, offset, qMDiffBlocks.Cr.Length);
            offset += qMDiffBlocks.Cr.Length;
            System.Buffer.BlockCopy(qMDiffBlocks.Cb, 0, compressedByteArray, offset, qMDiffBlocks.Cb.Length);
            offset += qMDiffBlocks.Cb.Length;
            compressedByteArray = RLCompression.ModifiedRunLengthCompression(compressedByteArray);

            DYCrCb iQMDiffBlocks = InverseQuantizationAndZigzag(qMDiffBlocks, width, height);
            DYCrCb iMDiffBlocks = InverseDiscreteCosineTransformMPEG(iQMDiffBlocks, width, height);
            byte[] currentY = InverseDiffBlock(motionVectorsY, rIYCrCb.Y, iMDiffBlocks.Y, width, height, macroSizeY);
            byte[] currentCr = InverseDiffBlock(motionVectorsCr, rIYCrCb.Cr, iMDiffBlocks.Cr, width / 2, height / 2, macroSizeCrCb);
            byte[] currentCb = InverseDiffBlock(motionVectorsCb, rIYCrCb.Cb, iMDiffBlocks.Cb, width / 2, height / 2, macroSizeCrCb);
            YCrCb currentBlocks = new YCrCb(currentY, currentCr, currentCb);
            YCrCb filledIDiffBlocks = fillSubSample(currentBlocks, width, height);

            displayBitmap = convertToBitmap(filledIDiffBlocks, width, height);

            MPEGPrep mPEGPrep = new MPEGPrep(motionVectorsY, motionVectorsCr, motionVectorsCb, filledIDiffBlocks);
            return mPEGPrep;
        }

        public static Bitmap MPEGDecompression(byte[] inputArray)
        {
            currentQuantizationTable = quantizationTableJPEG;
            inputArray = RLCompression.ModifiedRunLengthDecompress(inputArray);
            byte[] widthByteArray = new byte[intToByteSize];
            byte[] heightByteArray = new byte[intToByteSize];
            int offset = 0;
            System.Buffer.BlockCopy(inputArray, 0, widthByteArray, 0, intToByteSize);
            offset += intToByteSize;
            System.Buffer.BlockCopy(inputArray, offset, heightByteArray, 0, intToByteSize);
            offset += intToByteSize;

            int width = BitConverter.ToInt32(widthByteArray, 0);
            int height = BitConverter.ToInt32(widthByteArray, 0);
            int reducedWidth = (int)(((double)width / 2.0));
            int reducedHeight = (int)(((double)height / 2.0));

            byte[] qY = new byte[width * height];
            byte[] qCr = new byte[reducedWidth * reducedHeight];
            byte[] qCb = new byte[reducedWidth * reducedHeight];

            System.Buffer.BlockCopy(inputArray, offset, qY, 0, qY.Length);
            offset += qY.Length;
            System.Buffer.BlockCopy(inputArray, offset, qCr, 0, qCr.Length);
            offset += qCr.Length;
            System.Buffer.BlockCopy(inputArray, offset, qCb, 0, qCb.Length);
            offset += qCb.Length;

            YCrCb qYCrCb = new YCrCb(qY, qCr, qCb);
            DYCrCb iQYCrCb = InverseQuantizationAndZigzag(qYCrCb, width, height);
            YCrCb iYCrCb = InverseDiscreteCosineTransform(iQYCrCb, width, height);
            YCrCb fillediYCrCb = fillSubSample(iYCrCb, width, height);
            Bitmap result = convertToBitmap(fillediYCrCb, width, height);
            return result;
        }

        public static byte[] convertToByteFromVector(Vector[] vectors)
        {
            byte[] result = new byte[vectors.Length * 2];
            for (int i = 0, index = 0; i < vectors.Length; i++)
            {
                result[index++] = (byte)vectors[i].x;
                result[index++] = (byte)vectors[i].y;
            }
            return result;
        }

        /// <summary>
        /// Returns difference block of two frame using given motion vectors
        /// </summary>
        /// <param name="motionVectors">Motion vectors</param>
        /// <param name="reference">reference frame</param>
        /// <param name="current">current frame</param>
        /// <param name="width">width of the channel</param>
        /// <param name="height">height of the channel</param>
        ///  <param name="N">macro block side length</param>
        public static double[] DiffBlock(Vector[] motionVectors, byte[] reference, byte[] current, int width, int height, int N)
        {
            int index = 0;
            double[] diffBlock = new double[reference.Length];
            for (int y = 0; y < height; y+=N)
            {
                for (int x = 0; x < width; x+=N)
                {
                    int i = motionVectors[index].x;
                    int j = motionVectors[index].y;
                    for (int k = 0; k < N; k++)
                    {
                        for (int l = 0; l < N; l++)
                        {
                            if (y + l < width && x + k < height && x + i + k < height && y + j + l < width)
                            {
                                diffBlock[x + k + (y + l) * width] = (double)current[x + k + (y + l) * width] - (double)reference[x + i + k + (y + j + l) * width];
                            }
                        }
                    }
                }
                index++;
            }
            return diffBlock;
        }

        /// <summary>
        /// Returns back a current frame from reference and difference block frame
        /// </summary>
        /// <param name="motionVectors">motion vectors</param>
        /// <param name="reference">reference frame</param>
        /// <param name="diffBlock">difference block frame</param>
        /// <param name="width">channel width</param>
        /// <param name="height">channel height</param>
        /// <param name="N">macro block side length</param>
        /// <returns></returns>
        public static byte[] InverseDiffBlock(Vector[] motionVectors, byte[] reference, double[] diffBlock, int width, int height, int N)
        {
            int index = 0;
            byte[] current = new byte[reference.Length];
            for (int y = 0; y < height; y += N)
            {
                for (int x = 0; x < width; x += N)
                {
                    int i = motionVectors[index].x;
                    int j = motionVectors[index].y;
                    for (int k = 0; k < N; k++)
                    {
                        for (int l = 0; l < N; l++)
                        {
                            if (y + l < width && x + k < height && x + i + k < height && y + j + l < width)
                            {
                                current[x + k + (y + l) * width] = getBoundedByte((double)reference[x + i + k + (y + j + l) * width] + diffBlock[x + k + (y + l) * width]);
                            }
                        }
                    }
                    index++;
                }
            }
            return current;
        }

        /// <summary>
        /// Mean Absolute Difference for motion vector
        /// </summary>
        /// <param name="N">macro block side length</param>
        /// <param name="x">upper left corner x coordinate</param>
        /// <param name="y">upper left corner y coordinate</param>
        /// <param name="i">x offset of search area</param>
        /// <param name="j">y offset of search area</param>
        /// <param name="CR">current frame R value</param>
        /// <param name="CG">current frame G value</param>
        /// <param name="CB">current frame B value</param>
        /// <param name="RR">reference frame R value</param>
        /// <param name="RG">reference frame G value</param>
        /// <param name="RB">reference frame B value</param>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <returns>Mean absolute differences</returns>
        public static double MAD(int N, int x, int y, int i, int j,  byte[] C, byte[] R, int width, int height)
        {
            double result = 0;
            for (int k = 0; k < N; k++)
            {
                for (int l = 0; l < N; l++)
                {
                    if (x + i + k >= 0 && y + j + l >= 0 && x + i + k < height && y + j + l < width && x + k < height && y + l < width)
                    {
                        double differences = 0;
                        differences = Math.Abs(C[x + k + (y + l) * width] - R[x + i + k + (y + j + l) * width]);
                        result += differences;
                    }
                }
            }
            return result / N / N;
        }

        /// <summary>
        /// Return compressed byte array
        /// </summary>
        /// <returns></returns>
        public static byte[] getCompressedByteArray()
        {
            return compressedByteArray;
        }

        /// <summary>
        /// Convert a bitmap's RGB channels into Y Cr Cb channels
        /// </summary>
        /// <param name="bitmap">Original bitmap</param>
        /// <returns></returns>
        public static YCrCb convertToYCrCb(Bitmap bitmap)
        {
            YCrCb yCrCb;
            int bitmapSize = bitmap.Width * bitmap.Height;
            byte[] Y = new byte[bitmapSize];
            byte[] Cr = new byte[bitmapSize];
            byte[] Cb = new byte[bitmapSize];
            int index = 0;

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    double tempY = KR * pixel.R + KG * pixel.G + KB * pixel.B;
                    double tempCb = 128.0 - CBR * pixel.R - CBG * pixel.G + CBB * pixel.B;
                    double tempCr = 128.0 + CRR * pixel.R - CRG * pixel.G - CRB * pixel.B;
                    Y[index] = getBoundedByte(tempY);
                    Cb[index] = getBoundedByte(tempCb);
                    Cr[index] = getBoundedByte(tempCr);
                    index++;
                }
            }
            yCrCb = new YCrCb(Y, Cr, Cb);
            return yCrCb;
        }

        /// <summary>
        /// Convert Y Cr Cb channels back into bitmap
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">width of bitmap</param>
        /// <param name="height">height of bitmap</param>
        /// <returns></returns>
        public static Bitmap convertToBitmap(YCrCb yCrCb, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            int index = 0;
            Color[] pixels = new Color[yCrCb.Y.Length];
            
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double tempR = yCrCb.Y[index] + 1.402 * (yCrCb.Cr[index] - 128.0);
                    double tempG = yCrCb.Y[index] - 0.344136 * (yCrCb.Cb[index] - 128.0) -
                                0.714136 * (yCrCb.Cr[index] - 128.0);
                    double tempB = yCrCb.Y[index] + 1.772 * (yCrCb.Cb[index] - 128.0);
                    byte R = getBoundedByte(tempR);
                    byte G = getBoundedByte(tempG);
                    byte B = getBoundedByte(tempB);
                    Color pixel = Color.FromArgb(R, G, B);
                    pixels[index] = pixel;

                    bitmap.SetPixel(i, j, pixel);
                    index++;
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Sub sample a given yCrCb's Cr Cb channel by 3/4
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">Width of the channels</param>
        /// <param name="height">Height of the channels</param>
        /// <returns></returns>
        public static YCrCb subSample(YCrCb yCrCb, int width, int height)
        {
            YCrCb subYCrCb;
            int reducedWidth = (int)(width / 2.0);
            int reducedHeight = (int)(height / 2.0);
            byte[] subCr = new byte[reducedWidth * reducedHeight];
            byte[] subCb = new byte[reducedWidth * reducedHeight];
            int originalIndex = 0;
            int index = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        subCr[index] = yCrCb.Cr[originalIndex];
                        subCb[index] = yCrCb.Cb[originalIndex];
                        index++;
                    }
                    originalIndex++;
                }
            }
            subYCrCb = new YCrCb(yCrCb.Y, subCr, subCb);
            return subYCrCb;
        }

        /// <summary>
        /// Filled sub sampled Y Cr Cb channels with repeated values
        /// </summary>
        /// <param name="subYCrCb"> sub sampled Y Cr Cb channels</param>
        /// <param name="width">Width of the channels</param>
        /// <param name="height">Height of the channels</param>
        /// <returns></returns>
        public static YCrCb fillSubSample(YCrCb subYCrCb, int width, int height)
        {
            YCrCb filledYCrCb;
            byte[] filledCr = new byte[subYCrCb.Y.Length];
            byte[] filledCb = new byte[subYCrCb.Y.Length];
            int unfilledIndex = 0;
            int index = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        // even row and even column
                        filledCr[index] = subYCrCb.Cr[unfilledIndex];
                        filledCb[index] = subYCrCb.Cb[unfilledIndex];
                        unfilledIndex++;
                    }
                    else if (i % 2 == 0 && j % 2 != 0)
                    {
                        // even row and odd column
                        filledCr[index] = filledCr[index - 1];
                        filledCb[index] = filledCb[index - 1];
                    }
                    else if (i % 2 != 0)
                    {
                        // odd row
                        filledCr[index] = filledCr[index - width];
                        filledCb[index] = filledCb[index - width];
                    }
                    index++;
                }
            }
            filledYCrCb = new YCrCb(subYCrCb.Y, filledCr, filledCb);
            return filledYCrCb;
        }

        /// <summary>
        /// Returns byte after checking for boundary
        /// </summary>
        /// <param name="tempValue">Value to be casted into byte</param>
        /// <returns></returns>
        public static byte getBoundedByte(double tempValue)
        {
            if (tempValue < 0)
            {
                tempValue = 0;
            }
            else if (tempValue > 255)
            {
                tempValue = 255;
            }

            return (byte)tempValue;
        }

        /// <summary>
        /// Return byte after checking for boundary for signed byte type double
        /// </summary>
        /// <param name="tempValue">signed byte type double</param>
        /// <returns></returns>
        public static byte getBoundedSByte(double tempValue)
        {
            if (tempValue < -128)
            {
                tempValue = -128;
            }
            else if (tempValue > 127)
            {
                tempValue = 127;
            }
            return (byte)tempValue;
        }

        /// <summary>
        /// Perform discrete cosine transformation on given Y Cr Cb channels of an image
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <returns></returns>
        public static DYCrCb DiscreteCosineTransform(YCrCb yCrCb, int width, int height)
        {
            int numOfBlockRow = (int)Math.Ceiling((height / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(width / 8.0);
            int reducedWidth = (int)Math.Ceiling(height / 2.0);
            int reducedHeight = (int)Math.Ceiling(width / 2.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(reducedWidth / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(reducedHeight / 8.0);
            double[] modY = BlockTransform(ArrayTransform.byteArrayToDouble(yCrCb.Y),
                numOfBlockRow, numOfBlockColumn, width, height);
            double[] modCr = BlockTransform(ArrayTransform.byteArrayToDouble(yCrCb.Cr),
                reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            double[] modCb = BlockTransform(ArrayTransform.byteArrayToDouble(yCrCb.Cb),
                reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            DYCrCb result = new DYCrCb(modY, modCr, modCb);
            return result;
        }

        public static DYCrCb DiscreteCosineTransform(DYCrCb yCrCb, int width, int height)
        {
            int numOfBlockRow = (int)(height / 8.0);
            int numOfBlockColumn = (int)(width / 8.0);
            int reducedWidth = (int)(height / 2.0);
            int reducedHeight = (int)(width / 2.0);
            int reducedNumOfBlockRow = (int)(reducedWidth / 8.0);
            int reducedNumOfBlockColumn = (int)(reducedHeight / 8.0);
            double[] modY = BlockTransform(yCrCb.Y, numOfBlockRow, numOfBlockColumn, width, height);
            double[] modCr = BlockTransform(yCrCb.Cr, reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            double[] modCb = BlockTransform(yCrCb.Cb, reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            DYCrCb result = new DYCrCb(modY, modCr, modCb);
            return result;
        }

        /// <summary>
        /// Perform discrete cosine transformation on given Y Cr Cb channels of an image
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <returns></returns>
        public static YCrCb InverseDiscreteCosineTransform(DYCrCb yCrCb, int width, int height)
        {
            int numOfBlockRow = (int)Math.Ceiling((height / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(width / 8.0);
            int reducedWidth = (int)Math.Ceiling(height / 2.0);
            int reducedHeight = (int)Math.Ceiling(width / 2.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(reducedWidth / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(reducedHeight / 8.0);
            double[] modY = InverseBlockTransform(yCrCb.Y, numOfBlockRow, numOfBlockColumn,
                width, height);
            double[] modCr = InverseBlockTransform(yCrCb.Cr, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                reducedWidth, reducedHeight);
            double[] modCb = InverseBlockTransform(yCrCb.Cb, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                reducedWidth, reducedHeight);
            YCrCb result = new YCrCb(ArrayTransform.doubleArrayToByte(modY),
                ArrayTransform.doubleArrayToByte(modCr),
                ArrayTransform.doubleArrayToByte(modCb));
            return result;
        }

        public static DYCrCb InverseDiscreteCosineTransformMPEG(DYCrCb yCrCb, int width, int height)
        {
            int numOfBlockRow = (int)Math.Ceiling((height / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(width / 8.0);
            int reducedWidth = (int)Math.Ceiling(height / 2.0);
            int reducedHeight = (int)Math.Ceiling(width / 2.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(reducedWidth / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(reducedHeight / 8.0);
            double[] modY = InverseBlockTransform(yCrCb.Y, numOfBlockRow, numOfBlockColumn,
                width, height);
            double[] modCr = InverseBlockTransform(yCrCb.Cr, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                reducedWidth, reducedHeight);
            double[] modCb = InverseBlockTransform(yCrCb.Cb, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                reducedWidth, reducedHeight);
            DYCrCb result = new DYCrCb(modY, modCr, modCb);
            return result;
        }

        /// <summary>
        /// Quantize and zigzag reorder
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <returns>Quantized and Zigzagged Y Cr Cb structure</returns>
        public static YCrCb QuantizationAndZigzag(DYCrCb yCrCb, int width, int height)
        {
            int numOfBlockRow = (int)Math.Ceiling((height / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(width / 8.0);
            int reducedWidth = (int)Math.Ceiling(height / 2.0);
            int reducedHeight = (int)Math.Ceiling(width / 2.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(reducedWidth / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(reducedHeight / 8.0);
            byte[] modY = BlockQuantization(yCrCb.Y,
                numOfBlockRow, numOfBlockColumn, width, height);
            byte[] modCr = BlockQuantization(yCrCb.Cr,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            byte[] modCb = BlockQuantization(yCrCb.Cb,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            if (zigzag)
            {
                modY = ByteArrayToZigzag(modY, numOfBlockRow, numOfBlockColumn, width, height);
                modCr = ByteArrayToZigzag(modCr,
                    reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
                modCb = ByteArrayToZigzag(modCb,
                    reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            }
            YCrCb result = new YCrCb(modY, modCr, modCb);
            return result;
        }

        /// <summary>
        /// Inverse quantization and zigzag pattern
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <returns></returns>
        public static DYCrCb InverseQuantizationAndZigzag(YCrCb yCrCb, int width, int height)
        {
            int numOfBlockRow = (int)Math.Ceiling((height / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(width / 8.0);
            int reducedWidth = (int)Math.Ceiling(height / 2.0);
            int reducedHeight = (int)Math.Ceiling(width / 2.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(reducedWidth / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(reducedHeight / 8.0);
            double[] modY = InverseBlockQuantization(yCrCb.Y,
                numOfBlockRow, numOfBlockColumn, width, height);
            double[] modCr = InverseBlockQuantization(yCrCb.Cr,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            double[] modCb = InverseBlockQuantization(yCrCb.Cb,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, reducedWidth, reducedHeight);
            
            DYCrCb result = new DYCrCb(modY, modCr, modCb);
            return result;
        }

        /// <summary>
        /// Support method for DCT and IDCT's calculation for a single channel
        /// </summary>
        /// <param name="channel">Given channel</param>
        /// <param name="numOfBlockRow">Number of block rows for the image</param>
        /// <param name="numOfBlockColumn">Number of block column for the image</param>
        /// <param name="width">Width of the channel</param>
        /// <param name="height">Height of the channel</param>
        /// <returns></returns>
        public static double[] BlockTransform(double[] channel, int numOfBlockRow,
            int numOfBlockColumn, int width, int height)
        {
            int numOfblock = numOfBlockRow * numOfBlockColumn;
            double cu;
            double cv;
            double tempResult = 0.0;
            int blockSize = 8;
            int currentBlockRow = 0;
            int currentBlockColumn = 0;
            double[] result = new double[channel.Length];
            for (int block = 0; block < numOfblock; block++)
            {
                for (int u = 0; u < blockSize && (u + currentBlockRow * blockSize) < height; u++)
                {
                    if (u == 0)
                    {
                        cu = C_ZERO;
                    }
                    else
                    {
                        cu = C_NONZERO;
                    }
                    for (int v = 0; v < blockSize && v + currentBlockColumn * blockSize < width; v++)
                    {
                        if (v == 0)
                        {
                            cv = C_ZERO;
                        }
                        else
                        {
                            cv = C_NONZERO;
                        }
                        for (int x = 0; x < blockSize && x + currentBlockRow * blockSize < height; x++)
                        {
                            for ( int y = 0; y < blockSize && y + currentBlockColumn * blockSize < width; y++)
                            {
                                tempResult += channel[(x + currentBlockRow * blockSize) *
                                    width + (y + currentBlockColumn * blockSize)] *
                                    Math.Cos((2.0 * x + 1.0) * u * Math.PI / (2.0 * blockSize)) *
                                    Math.Cos((2.0 * y + 1.0) * v * Math.PI / (2.0 * blockSize));
                            }
                        }
                        tempResult *= (2.0 * cu * cv / Math.Sqrt(blockSize * blockSize));
                        result[(u + currentBlockRow * blockSize) * width + v + currentBlockColumn * blockSize] = tempResult;
                        tempResult = 0;
                    }
                }
                    currentBlockColumn++;
                if (currentBlockColumn == numOfBlockColumn)
                {
                    currentBlockColumn = 0;
                    currentBlockRow++;
                }

            }
            return result;
        }

        public static double[] InverseBlockTransform(double[] channel, int numOfBlockRow,
            int numOfBlockColumn, int width, int height)
        {
            int numOfblock = numOfBlockRow * numOfBlockColumn;
            double cu;
            double cv;
            double tempResult = 0.0;
            int blockSize = 8;
            int currentBlockRow = 0;
            int currentBlockColumn = 0;
            double[] result = new double[channel.Length];
            for (int block = 0; block < numOfblock; block++)
            {
                for (int x = 0; x < blockSize && x + currentBlockRow * blockSize < height; x++)
                {

                    for (int y = 0; y < blockSize && y + currentBlockColumn * blockSize < width; y++)
                    {

                        for (int u = 0; u < blockSize && u + currentBlockRow * blockSize < height; u++)
                        {
                            if (u == 0)
                            {
                                cu = C_ZERO;
                            }
                            else
                            {
                                cu = C_NONZERO;
                            }
                            for (int v = 0; v < blockSize && v + currentBlockColumn * blockSize < width; v++)
                            {
                                if (v == 0)
                                {
                                    cv = C_ZERO;
                                }
                                else
                                {
                                    cv = C_NONZERO;
                                }
                                tempResult += channel[(u + currentBlockRow * blockSize) * width +
                                    (v + currentBlockColumn * blockSize)] *
                                    Math.Cos((2.0 * x + 1.0) * u * Math.PI / (2.0 * blockSize)) *
                                    Math.Cos((2.0 * y + 1.0) * v * Math.PI / (2.0 * blockSize)) *
                                    (2.0 * cu * cv / Math.Sqrt(blockSize * blockSize));
                            }
                        }
                        result[(x + currentBlockRow * blockSize) * width + y + currentBlockColumn * blockSize] = tempResult;
                        tempResult = 0;
                    }
                }
                currentBlockColumn++;
                if (currentBlockColumn == numOfBlockColumn)
                {
                    currentBlockColumn = 0;
                    currentBlockRow++;
                }

            }
            return result;
        }

        public static byte[] BlockQuantization(double[] channel, int numOfBlockRow,
            int numOfBlockColumn, int width, int height)
        {
            int numOfblock = numOfBlockRow * numOfBlockColumn;
            int blockSize = 8;
            int currentBlockRow = 0;
            int currentBlockColumn = 0;
            byte[] result = new byte[channel.Length];
            for (int block = 0; block < numOfblock; block++)
            {

                for (int x = 0; x < blockSize && x + currentBlockRow * blockSize < width; x++)
                {
                    for (int y = 0; y < blockSize && y + currentBlockColumn * blockSize < height; y++)
                    {
                        result[(x + currentBlockRow * blockSize) * width +
                            (y + currentBlockColumn * blockSize)] = getBoundedSByte(Math.Round((channel[(x + currentBlockRow * blockSize) * width +
                            (y + currentBlockColumn * blockSize)] / currentQuantizationTable[x * blockSize + y])));
                    }
                }
                currentBlockColumn++;
                if (currentBlockColumn == numOfBlockColumn)
                {
                    currentBlockColumn = 0;
                    currentBlockRow++;
                }
            }
            return result;
        }
        
        public static double[] InverseBlockQuantization(byte[] channel, int numOfBlockRow,
            int numOfBlockColumn, int width, int height)
        {
            if (zigzag)
            {
                channel = ZigzagToOriginal(channel, numOfBlockRow, numOfBlockColumn, width, height);
            }
            int numOfblock = numOfBlockRow * numOfBlockColumn;
            int blockSize = 8;
            int currentBlockRow = 0;
            int currentBlockColumn = 0;
            double[] result = new double[channel.Length];
            for (int block = 0; block < numOfblock; block++)
            {

                for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < width; y++)
                {
                    for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < height; x++)
                    {
                         result[(x + currentBlockColumn * blockSize) +
                                (y + currentBlockRow * blockSize) * width] = (double)((sbyte)channel[(x + currentBlockColumn * blockSize) +
                                (y + currentBlockRow * blockSize) * width] * currentQuantizationTable[x + blockSize * y]);
                        
                    }
                }
                currentBlockColumn++;
                if (currentBlockColumn == numOfBlockColumn)
                {
                    currentBlockColumn = 0;
                    currentBlockRow++;
                }
            }
            return result;
        }

        /// <summary>
        /// Convert a byte array into a zigzag ordered byte array
        /// </summary>
        /// <param name="channel">Input channel</param>
        /// <param name="numOfBlockRow">number of block rows</param>
        /// <param name="numOfBlockColumn">number of block columns</param>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        /// <returns></returns>
        public static byte[] ByteArrayToZigzag(byte[] channel, int numOfBlockRow, 
            int numOfBlockColumn, int width, int height)
        {
            int numOfblock = numOfBlockRow * numOfBlockColumn;
            int blockSize = 8;
            int currentBlockRow = 0;
            int currentBlockColumn = 0;
            byte[] result = new byte[channel.Length];
            for (int block = 0; block < numOfblock; block++)
            {
                bool begin = true;
                int topCap = 0;
                bool toRight = true;
                int mappedX = 0;
                int mappedY = 0;
                for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < height; y++)
                {
                    for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < width; x++)
                    {
                        if (begin)
                        {
                            begin = false;
                        }
                        else if (toRight && mappedY == topCap && topCap < 7 && mappedY + currentBlockColumn * blockSize + 1 < width)
                        {
                            mappedY++;
                            topCap++;
                            toRight = !toRight;
                        }
                        else if (toRight && mappedY < topCap && mappedY + currentBlockColumn * blockSize + 1 < width)
                        {
                            mappedY++;
                            mappedX--;
                        }
                        else if (!toRight && mappedX == topCap && topCap < 7 && mappedX + currentBlockRow * blockSize + 1 < height)
                        {
                            mappedX++;
                            topCap++;
                            toRight = !toRight;
                        }
                        else if (!toRight && mappedX < topCap && mappedX + currentBlockRow * blockSize + 1 < height)
                        {
                            mappedX++;
                            mappedY--;
                        }
                        else if (toRight && mappedY == topCap && (topCap == 7 || mappedY + currentBlockColumn * blockSize + 1 < width))
                        {
                            mappedX++;
                            toRight = !toRight;
                        }
                        else if (!toRight && mappedX == topCap && (topCap == 7 || mappedX + currentBlockRow * blockSize + 1 < height))
                        {
                            mappedY++;
                            toRight = !toRight;
                        }

                        result[(y + currentBlockRow * blockSize) * width +
                            (x + currentBlockColumn * blockSize)] = channel[
                                (mappedX + currentBlockRow * blockSize) * width +
                                (mappedY + currentBlockColumn * blockSize)];
                    }
                }
                currentBlockColumn++;
                if (currentBlockColumn == numOfBlockColumn)
                {
                    currentBlockColumn = 0;
                    currentBlockRow++;
                }
            }
            return result;
        }

        /// <summary>
        /// Convert a zigzag ordered array back into orignial array
        /// </summary>
        /// <param name="channel">Zigzagged channel</param>
        /// <param name="numOfBlockRow">number of block rows</param>
        /// <param name="numOfBlockColumn">number of block columns</param>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        /// <returns></returns>
        public static byte[] ZigzagToOriginal(byte[] channel, int numOfBlockRow, int numOfBlockColumn, int width, int height)
        {
            int numOfblock = numOfBlockRow * numOfBlockColumn;
            int blockSize = 8;
            int currentBlockRow = 0;
            int currentBlockColumn = 0;
            byte[] result = new byte[channel.Length];
            for (int block = 0; block < numOfblock; block++)
            {
                bool begin = true;
                int topCap = 0;
                bool toRight = true;
                int mappedX = 0;
                int mappedY = 0;
                for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < height; y++)
                {
                    for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < width; x++)
                    {
                        if (begin)
                        {
                            begin = false;
                        }
                        else if (toRight && mappedY == topCap && topCap < 7 && mappedY + currentBlockColumn * blockSize + 1 < width)
                        {
                            mappedY++;
                            topCap++;
                            toRight = !toRight;
                        }
                        else if (toRight && mappedY < topCap && mappedY + currentBlockColumn * blockSize + 1 < width)
                        {
                            mappedY++;
                            mappedX--;
                        }
                        else if (!toRight && mappedX == topCap && topCap < 7 && mappedX + currentBlockRow * blockSize + 1 < height)
                        {
                            mappedX++;
                            topCap++;
                            toRight = !toRight;
                        }
                        else if (!toRight && mappedX < topCap && mappedX + currentBlockRow * blockSize + 1 < height)
                        {
                            mappedX++;
                            mappedY--;
                        }
                        else if (toRight && mappedY == topCap && (topCap == 7 || mappedY + currentBlockColumn * blockSize + 1 < width))
                        {
                            mappedX++;
                            toRight = !toRight;
                        }
                        else if (!toRight && mappedX == topCap && (topCap == 7 || mappedX + currentBlockRow * blockSize + 1 < height))
                        {
                            mappedY++;
                            toRight = !toRight;
                        }

                        result[(mappedX + currentBlockRow * blockSize) * width +
                            (mappedY + currentBlockColumn * blockSize)] = channel[
                                (y + currentBlockRow * blockSize) * width +
                                (x + currentBlockColumn * blockSize)];
                    }
                }
                currentBlockColumn++;
                if (currentBlockColumn == numOfBlockColumn)
                {
                    currentBlockColumn = 0;
                    currentBlockRow++;
                }
            }
            return result;
        }

        public static byte[] padChannel(byte[] channel, int width, int height)
        {
            int rightPad = width % 8;
            int bottomPad = height % 8;
            byte[] paddedChannel = new byte[(width + rightPad) * (height + bottomPad)];
            int padIndex = 0;
            int channelIndex = 0;
            for (int i = 0; i < width + rightPad; i++)
            {
                for (int j = 0; j < height + bottomPad; j++)
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
            for (int i = 0; i < width + rightPad; i++)
            {
                for (int j = 0; j < height + rightPad; j++)
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

    }
}
