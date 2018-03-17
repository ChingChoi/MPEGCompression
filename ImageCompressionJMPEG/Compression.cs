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
        public int yHeight;
        public int yWidth;
        public int crCbHeight;
        public int crCbWidth;

        /// <summary>
        /// Constructor for Y Cr Cb structure
        /// </summary>
        /// <param name="Y">Y channel</param>
        /// <param name="Cr">Cr channel</param>
        /// <param name="Cb">Cb channel</param>
        public YCrCb(byte[] Y, byte[] Cr, byte[] Cb, int yHeight, int yWidth, int crCbHeight, int crCbWidth)
        {
            this.Y = Y;
            this.Cr = Cr;
            this.Cb = Cb;
            this.yHeight = yHeight;
            this.yWidth = yWidth;
            this.crCbHeight = crCbHeight;
            this.crCbWidth = crCbWidth;
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
        public int yHeight;
        public int yWidth;
        public int crCbHeight;
        public int crCbWidth;

        /// <summary>
        /// Constructor for double Y Cr Cb structure
        /// </summary>
        /// <param name="Y">Y channel</param>
        /// <param name="Cr">Cr channel</param>
        /// <param name="Cb">Cb channel</param>
        public DYCrCb(double[] Y, double[] Cr, double[] Cb, int yHeight, int yWidth, int crCbHeight, int crCbWidth)
        {
            this.Y = Y;
            this.Cr = Cr;
            this.Cb = Cb;
            this.yHeight = yHeight;
            this.yWidth = yWidth;
            this.crCbHeight = crCbHeight;
            this.crCbWidth = crCbWidth;
        }
    }

    /// <summary>
    /// Holds all information required for saving a custom jpeg compressed image
    /// </summary>
    public struct JPEGInfo
    {
        public int originalWidth;
        public int originalHeight;
        public YCrCb qYCrCb;

        public JPEGInfo(int originalWidth, int originalHeight, YCrCb qYCrCb)
        {
            this.originalWidth = originalWidth;
            this.originalHeight = originalHeight;
            this.qYCrCb = qYCrCb;
        }
    }

    /// <summary>
    /// Hold all information required for saving a custom mpeg compressed images
    /// </summary>
    public struct MPEGInfo
    {
        public int originalWidth;
        public int originalHeight;
        public YCrCb[] iFrames;
        public PFrame[] pFrames;

        public MPEGInfo(int originalWidth, int originalHeight, YCrCb[] iFrames, PFrame[] pFrames)
        {
            this.originalWidth = originalWidth;
            this.originalHeight = originalHeight;
            this.iFrames = iFrames;
            this.pFrames = pFrames;
        }
    }

    /// <summary>
    /// Hold information of P frame
    /// </summary>
    public struct PFrame
    {
        YCrCb diffBlock;
        Vector[] motionVectorsY;
        Vector[] motionVectorsCr;
        Vector[] motionVectorsCb;

        public PFrame(YCrCb diffBlock, Vector[] motionVectorY, Vector[] motionVectorCr, Vector[] motionVectorCb)
        {
            this.diffBlock = diffBlock;
            this.motionVectorsY = motionVectorY;
            this.motionVectorsCr = motionVectorCr;
            this.motionVectorsCb = motionVectorCb;
        }

        public YCrCb DiffBlock { get => diffBlock; set => diffBlock = value; }
        public Vector[] MotionVectorsY { get => motionVectorsY; set => motionVectorsY = value; }
        public Vector[] MotionVectorsCr { get => motionVectorsCr; set => motionVectorsCr = value; }
        public Vector[] MotionVectorsCb { get => motionVectorsCb; set => motionVectorsCb = value; }
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

        /// <summary>
        /// Bitmap for display
        /// </summary>
        public static Bitmap displayBitmap;

        /// <summary>
        /// Byte array size after converting from an int
        /// </summary>
        public static int intToByteSize = 4;

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

        /// <summary>
        /// macro block size for Y Channel
        /// </summary>
        public static int macroSizeY = 16;

        /// <summary>
        /// macro block size for Cr Cb channels
        /// </summary>
        public static int macroSizeCrCb = 8;

        /// <summary>
        /// Number of frames to be processed
        /// </summary>
        public static int numOfFrame = 0;

        /// <summary>
        /// Original width of the image
        /// </summary>
        public static int originalWidth = 0;

        /// <summary>
        /// Original height of the image
        /// </summary>
        public static int originalHeight = 0;

        /// <summary>
        /// Indicate if currently on jpeg view
        /// </summary>
        public static bool jView = true;

        /// <summary>
        /// Indicate if currently on mpeg view
        /// </summary>
        public static bool mView = false;

        /// <summary>
        /// Indicate if currently on grayscale view
        /// </summary>
        public static bool gView = false;

        /// <summary>
        /// Range between each I frame
        /// </summary>
        public static int I_FRAME_RANGE = 10;

        /// <summary>
        /// Will hold the currently selected quantization table depending on jpeg or mpeg
        /// </summary>
        public static double[] currentQuantizationTable;
        public static int SearchArea { get => searchArea; set => searchArea = value; }

        /// <summary>
        /// Compress image into JPEG format
        /// </summary>
        /// <param name="bitmap">Original bitmap</param>
        /// <param name="width">Original bitmap width</param>
        /// <param name="height">Original bitmap height</param>
        /// <returns></returns>
        public static JPEGInfo JPEGCompression(Bitmap bitmap)
        {
            int yDivider = 8;
            int crCbDivider = 8;
            originalHeight = bitmap.Height;
            originalWidth = bitmap.Width;
            currentQuantizationTable = quantizationTableJPEG;
            // compressing
            YCrCb yCrCb = convertToYCrCb(bitmap);
            YCrCb subYCrCb = subSample(yCrCb);
            subYCrCb = ArrayTransform.padChannels(subYCrCb, yDivider, crCbDivider);
            DYCrCb dctYCrCb = DiscreteCosineTransform(subYCrCb);
            YCrCb qYCrCb = QuantizationAndZigzag(dctYCrCb);
            return new JPEGInfo(originalWidth, originalHeight, qYCrCb);
        }

        public static void Grayscale(Bitmap bitmap, out Bitmap grayscaleBitmap, out Bitmap grayscaleBitmapTwo)
        {
            YCrCb yCrCb = convertToYCrCb(bitmap);
            YCrCb subYCrCb = subSample(yCrCb);
            // grayscale channel display
            grayscaleBitmap = convertToGrayscaleBitmap(subYCrCb.Cr, subYCrCb.crCbWidth, subYCrCb.crCbHeight);
            grayscaleBitmapTwo = convertToGrayscaleBitmap(subYCrCb.Cb, subYCrCb.crCbWidth, subYCrCb.crCbHeight);
        }

        /// <summary>
        /// Decompress JPEG into bitmap
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        public static Bitmap JPEGDecompression(JPEGInfo jpegInfo)
        {
            int yDivider = 8;
            int crCbDivder = 8;
            originalHeight = jpegInfo.originalHeight;
            originalWidth = jpegInfo.originalWidth;
            YCrCb qYCrCb = jpegInfo.qYCrCb;
            DYCrCb iQYCrCb = InverseQuantizationAndZigzag(qYCrCb);
            YCrCb iYCrCb = InverseDiscreteCosineTransform(iQYCrCb);
            iYCrCb = ArrayTransform.unpadChannels(iYCrCb, yDivider, crCbDivder);
            YCrCb fillediYCrCb = fillSubSample(iYCrCb);
            Bitmap result = convertToBitmap(fillediYCrCb);
            return result;
        }

        /// <summary>
        /// Motion vector for two frame and prepares for MPEG
        /// </summary>
        /// <param name="referenceArray">Reference frame</param>
        /// <param name="resultArray">Result frame</param>
        /// <returns>MPEGPrep structure</returns>
        public static PFrame MPEGMotionVector(Bitmap reference, Bitmap current)
        {
            numOfFrame = 2;
            originalHeight = reference.Height;
            originalWidth = reference.Width;

            YCrCb rFilledIYCrCb = convertToYCrCb(reference);
            YCrCb rIYCrCb = subSample(rFilledIYCrCb);
            rIYCrCb = ArrayTransform.padChannels(rIYCrCb, macroSizeY, macroSizeCrCb);
            int width = rIYCrCb.yWidth;
            int height = rIYCrCb.yHeight;

            currentQuantizationTable = quantizationTableJPEG;
            DYCrCb rdctYCrCb = DiscreteCosineTransform(rIYCrCb);
            YCrCb rQYCrCb = QuantizationAndZigzag(rdctYCrCb);

            YCrCb cYCrCb = convertToYCrCb(current);
            YCrCb cSubYCrCb = subSample(cYCrCb);
            cSubYCrCb = ArrayTransform.padChannels(cSubYCrCb, macroSizeY, macroSizeCrCb);

            Vector[] motionVectorsY;
            Vector[] motionVectorsCr;
            Vector[] motionVectorsCb;
            
            motionVector(cSubYCrCb, rIYCrCb, width, height, out motionVectorsY, out motionVectorsCr, out motionVectorsCb);

            double[] diffBlockY = new double[cYCrCb.Y.Length];
            double[] diffBlockCr = new double[cYCrCb.Cr.Length];
            double[] diffBlockCb = new double[cYCrCb.Cb.Length];

            currentQuantizationTable = quantizationTableMPEG;
            diffBlockY = DiffBlock(motionVectorsY, rIYCrCb.Y, cSubYCrCb.Y, width, height, macroSizeY);
            diffBlockCr = DiffBlock(motionVectorsCr, rIYCrCb.Cr, cSubYCrCb.Cr, cSubYCrCb.crCbWidth, cSubYCrCb.crCbHeight, macroSizeCrCb);
            diffBlockCb = DiffBlock(motionVectorsCb, rIYCrCb.Cb, cSubYCrCb.Cb, cSubYCrCb.crCbWidth, cSubYCrCb.crCbHeight, macroSizeCrCb);
                        
            DYCrCb mDiffBlocks = new DYCrCb(diffBlockY, diffBlockCr, diffBlockCb, cSubYCrCb.yHeight, cSubYCrCb.yWidth, cSubYCrCb.crCbHeight, cSubYCrCb.crCbWidth);
            DYCrCb dctMDiffBlocks = DiscreteCosineTransform(mDiffBlocks);
            YCrCb qMDiffBlocks = QuantizationAndZigzag(dctMDiffBlocks);

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

            DYCrCb iQMDiffBlocks = InverseQuantizationAndZigzag(qMDiffBlocks);
            DYCrCb iMDiffBlocks = InverseDiscreteCosineTransformMPEG(iQMDiffBlocks);
            byte[] currentY = InverseDiffBlock(motionVectorsY, rIYCrCb.Y, iMDiffBlocks.Y, iMDiffBlocks.yWidth, iMDiffBlocks.yHeight, macroSizeY);
            byte[] currentCr = InverseDiffBlock(motionVectorsCr, rIYCrCb.Cr, iMDiffBlocks.Cr, iMDiffBlocks.crCbWidth, iMDiffBlocks.crCbHeight, macroSizeCrCb);
            byte[] currentCb = InverseDiffBlock(motionVectorsCb, rIYCrCb.Cb, iMDiffBlocks.Cb, iMDiffBlocks.crCbWidth, iMDiffBlocks.crCbHeight, macroSizeCrCb);
            // to be fixed dimension
            YCrCb currentBlocks = new YCrCb(currentY, currentCr, currentCb, height, width, height / 2, width / 2);
            currentBlocks = ArrayTransform.unpadChannels(currentBlocks, macroSizeY, macroSizeCrCb);
            YCrCb filledIDiffBlocks = fillSubSample(currentBlocks);

            displayBitmap = convertToBitmap(filledIDiffBlocks);

            return new PFrame(filledIDiffBlocks, motionVectorsY, motionVectorsCr, motionVectorsCb);
        }

        public static PFrame MPEGMotionVector(YCrCb reference, Bitmap current)
        {
            originalHeight = current.Height;
            originalWidth = current.Width;
            // Prepare reference frame
            currentQuantizationTable = quantizationTableJPEG;
            DYCrCb rIQYCrCb = InverseQuantizationAndZigzag(reference);
            YCrCb rSubYCrCb = InverseDiscreteCosineTransform(rIQYCrCb);
            rSubYCrCb = ArrayTransform.padChannels(rSubYCrCb, macroSizeY, macroSizeCrCb);
            // Prepare current frame
            YCrCb cYCrCb = convertToYCrCb(current);
            YCrCb cSubYCrCb = subSample(cYCrCb);
            cSubYCrCb = ArrayTransform.padChannels(cSubYCrCb, macroSizeY, macroSizeCrCb);
            // Find motion vectors
            Vector[] motionVectorsY;
            Vector[] motionVectorsCr;
            Vector[] motionVectorsCb;
            motionVector(cSubYCrCb, rSubYCrCb, rSubYCrCb.yWidth, rSubYCrCb.yHeight,
                out motionVectorsY, out motionVectorsCr, out motionVectorsCb);
            // Calculate differences blocks
            double[] diffBlockY = DiffBlock(motionVectorsY, rSubYCrCb.Y, cSubYCrCb.Y,
                rSubYCrCb.yWidth, rSubYCrCb.yHeight, macroSizeY);
            double[] diffBlockCr = DiffBlock(motionVectorsCr, rSubYCrCb.Cr, cSubYCrCb.Cr,
                rSubYCrCb.crCbWidth, rSubYCrCb.crCbHeight, macroSizeCrCb);
            double[] diffBlockCb = DiffBlock(motionVectorsCb, rSubYCrCb.Cb, cSubYCrCb.Cb,
                rSubYCrCb.crCbWidth, rSubYCrCb.crCbHeight, macroSizeCrCb);
            // Compress differences blocks
            DYCrCb diffBlocks = new DYCrCb(diffBlockY, diffBlockCr, diffBlockCb, 
                cSubYCrCb.yHeight, cSubYCrCb.yWidth, cSubYCrCb.crCbHeight, cSubYCrCb.crCbWidth);
            DYCrCb dctDiffBlocks = DiscreteCosineTransform(diffBlocks);
            YCrCb qDiffBlocks = QuantizationAndZigzag(dctDiffBlocks);
            return new PFrame(qDiffBlocks, motionVectorsY, motionVectorsCr, motionVectorsCb);
        }

        /// <summary>
        /// Find motion vector for a given reference and current frame
        /// </summary>
        /// <param name="cSubYCrCb">Current frame</param>
        /// <param name="rIYCrCb">Reference frame</param>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <param name="motionVectorsY">motion vectors for Y channel</param>
        /// <param name="motionVectorsCr">motion vectors for Cr channel</param>
        /// <param name="motionVectorsCb">motion vectors for Cb channel</param>
        public static void motionVector(YCrCb cSubYCrCb, YCrCb rIYCrCb, int width, int height, out Vector[] motionVectorsY, out Vector[] motionVectorsCr, out Vector[] motionVectorsCb)
        {
            motionVectorsY = new Vector[width / macroSizeY * height / macroSizeY];
            motionVectorsCr = new Vector[motionVectorsY.Length];
            motionVectorsCb = new Vector[motionVectorsY.Length];
            int indexY = 0;
            int indexCr = 0;
            int indexCb = 0;
            for (int y = 0; y < height; y += macroSizeCrCb)
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
                    for (int j = -searchArea; j <= searchArea; j++)
                    {
                        for (int i = -searchArea; i <= searchArea; i++)
                        {
                            if (x + i >= 0 && y + j >= 0 && x + i + macroSizeY - 1 < width && y + j + macroSizeY - 1 < height)
                            {
                                if (x < rIYCrCb.crCbWidth && y < rIYCrCb.crCbHeight && x + i + macroSizeCrCb - 1 < rIYCrCb.crCbWidth && y + j + macroSizeCrCb - 1 < rIYCrCb.crCbHeight)
                                {
                                    double tempMinCr;
                                    double tempMinCb;
                                    tempMinCr = MAD(macroSizeCrCb, x, y, i, j, cSubYCrCb.Cr, rIYCrCb.Cr, rIYCrCb.crCbWidth, rIYCrCb.crCbHeight);
                                    tempMinCb = MAD(macroSizeCrCb, x, y, i, j, cSubYCrCb.Cb, rIYCrCb.Cb, rIYCrCb.crCbWidth, rIYCrCb.crCbHeight);
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
                                if (x % macroSizeY == 0 && y % macroSizeY == 0 && x + i - 1 < width && y + j - 1 < height)
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
                    if (x < rIYCrCb.crCbWidth && y < rIYCrCb.crCbHeight)
                    {
                        if (currentMinCr == centralMinCr)
                        {
                            motionVectorCr.x = 0;
                            motionVectorCr.y = 0;
                        }
                        motionVectorsCr[indexCr++] = motionVectorCr;
                    }
                    if (x < rIYCrCb.crCbWidth && y < rIYCrCb.crCbHeight)
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
        }

        /// <summary>
        /// MPEG compress all input frames
        /// pre-condition: all frames of same width and height
        /// </summary>
        /// <param name="inputFrames">input frames</param>
        /// <returns>Compressed MPEG information to be saved or rerendered</returns>
        public static MPEGInfo MPEGCompression(Bitmap[] inputFrames)
        {
            YCrCb[] iFrames = new YCrCb[inputFrames.Length / I_FRAME_RANGE + 1];
            PFrame[] pFrames = new PFrame[inputFrames.Length - iFrames.Length];
            int pFrameIndex = 0;
            JPEGInfo currentIFrame = JPEGCompression(inputFrames[0]);
            iFrames[0] = currentIFrame.qYCrCb;
            for (int currentFrame = 1; currentFrame < inputFrames.Length; currentFrame++)
            {
                if (currentFrame % I_FRAME_RANGE == 0 && currentFrame != 0)
                {
                    currentIFrame = JPEGCompression(inputFrames[currentFrame]);
                    iFrames[currentFrame / I_FRAME_RANGE] = currentIFrame.qYCrCb;
                }
                else
                {
                    pFrames[pFrameIndex++] = MPEGMotionVector(currentIFrame.qYCrCb, inputFrames[currentFrame]);
                }
            }
            return new MPEGInfo(inputFrames[0].Width, inputFrames[0].Height, iFrames, pFrames);
        }

        public static Bitmap[] MPEGDecompression(MPEGInfo mpegInfo)
        {
            originalHeight = mpegInfo.originalHeight;
            originalWidth = mpegInfo.originalWidth;
            int numOfFrames = mpegInfo.iFrames.Length + mpegInfo.pFrames.Length;
            int pFrameIndex = 0;
            int yDivider = 8;
            int crCbDivder = 8;
            Bitmap[] mpegFrames = new Bitmap[numOfFrames];
            YCrCb currentIFrame = mpegInfo.iFrames[0];
            DYCrCb iQCurrentIFrame = InverseQuantizationAndZigzag(currentIFrame);
            YCrCb iCurrentYCrCb = InverseDiscreteCosineTransform(iQCurrentIFrame);
            currentIFrame = ArrayTransform.padChannels(iCurrentYCrCb, macroSizeY, macroSizeCrCb);
            iCurrentYCrCb = ArrayTransform.unpadChannels(iCurrentYCrCb, yDivider, crCbDivder);
            mpegFrames[0] = convertToBitmap(fillSubSample(iCurrentYCrCb));

            for (int currentFrame = 1; currentFrame < numOfFrames; currentFrame++)
            {
                if (currentFrame % I_FRAME_RANGE == 0)
                {
                    currentQuantizationTable = quantizationTableJPEG;
                    DYCrCb iQYCrCb = InverseQuantizationAndZigzag(mpegInfo.iFrames[currentFrame / I_FRAME_RANGE]);
                    YCrCb iYCrCb = InverseDiscreteCosineTransform(iQYCrCb);
                    currentIFrame = ArrayTransform.padChannels(iYCrCb, macroSizeY, macroSizeCrCb);
                    iYCrCb = ArrayTransform.unpadChannels(iYCrCb, yDivider, crCbDivder);
                    mpegFrames[currentFrame] = convertToBitmap(fillSubSample(iYCrCb));
                }
                else
                {
                    currentQuantizationTable = quantizationTableMPEG;
                    DYCrCb iQDiffBlocks = InverseQuantizationAndZigzag(mpegInfo.pFrames[pFrameIndex].DiffBlock);
                    DYCrCb iDiffBlocks = InverseDiscreteCosineTransformMPEG(iQDiffBlocks);
                    byte[] currentY = InverseDiffBlock(mpegInfo.pFrames[pFrameIndex].MotionVectorsY,
                        currentIFrame.Y, iDiffBlocks.Y, iDiffBlocks.yWidth, iDiffBlocks.yHeight, macroSizeY);
                    byte[] currentCr = InverseDiffBlock(mpegInfo.pFrames[pFrameIndex].MotionVectorsCr,
                        currentIFrame.Cr, iDiffBlocks.Cr, iDiffBlocks.crCbWidth, iDiffBlocks.crCbHeight, macroSizeCrCb);
                    byte[] currentCb = InverseDiffBlock(mpegInfo.pFrames[pFrameIndex].MotionVectorsCb,
                        currentIFrame.Cb, iDiffBlocks.Cb, iDiffBlocks.crCbWidth, iDiffBlocks.crCbHeight, macroSizeCrCb);
                    YCrCb currentBlocks = new YCrCb(currentY, currentCr, currentCb, iDiffBlocks.yHeight,
                        iDiffBlocks.yWidth, iDiffBlocks.yHeight / 2, iDiffBlocks.yWidth / 2);
                    currentBlocks = ArrayTransform.unpadChannels(currentBlocks, macroSizeY, macroSizeCrCb);
                    mpegFrames[currentFrame] = convertToBitmap(fillSubSample(currentBlocks));
                    pFrameIndex++;
                }
            }
            return mpegFrames;
        }

        //public static Bitmap MPEGDecompression(byte[] inputArray)
        //{
        //    currentQuantizationTable = quantizationTableJPEG;
        //    inputArray = RLCompression.ModifiedRunLengthDecompress(inputArray);
        //    byte[] widthByteArray = new byte[intToByteSize];
        //    byte[] heightByteArray = new byte[intToByteSize];
        //    int offset = 0;
        //    System.Buffer.BlockCopy(inputArray, 0, widthByteArray, 0, intToByteSize);
        //    offset += intToByteSize;
        //    System.Buffer.BlockCopy(inputArray, offset, heightByteArray, 0, intToByteSize);
        //    offset += intToByteSize;

        //    int width = BitConverter.ToInt32(widthByteArray, 0);
        //    int height = BitConverter.ToInt32(widthByteArray, 0);
        //    int reducedWidth = (int)(((double)width / 2.0));
        //    int reducedHeight = (int)(((double)height / 2.0));

        //    byte[] qY = new byte[width * height];
        //    byte[] qCr = new byte[reducedWidth * reducedHeight];
        //    byte[] qCb = new byte[reducedWidth * reducedHeight];

        //    System.Buffer.BlockCopy(inputArray, offset, qY, 0, qY.Length);
        //    offset += qY.Length;
        //    System.Buffer.BlockCopy(inputArray, offset, qCr, 0, qCr.Length);
        //    offset += qCr.Length;
        //    System.Buffer.BlockCopy(inputArray, offset, qCb, 0, qCb.Length);
        //    offset += qCb.Length;

        //    YCrCb qYCrCb = new YCrCb(qY, qCr, qCb, height, width, reducedHeight, reducedWidth);
        //    DYCrCb iQYCrCb = InverseQuantizationAndZigzag(qYCrCb);
        //    YCrCb iYCrCb = InverseDiscreteCosineTransform(iQYCrCb);
        //    YCrCb fillediYCrCb = fillSubSample(iYCrCb);
        //    Bitmap result = convertToBitmap(fillediYCrCb);
        //    return result;
        //}

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
                    for (int l = 0; l < N; l++)
                    {
                        for (int k = 0; k < N; k++)
                        {
                            if (y + l < height && x + k < width && x + i + k < width && y + j + l < height)
                            {
                                diffBlock[x + k + (y + l) * width] = (double)current[x + k + (y + l) * width] - reference[x + i + k + (y + j + l) * width];
                            }
                        }
                    }
                    index++;
                }
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
                    for (int l = 0; l < N; l++)
                    {
                        for (int k = 0; k < N; k++)
                        {
                            if (y + l < height && x + k < width && x + i + k < width && y + j + l < height)
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
            int numRan = 0;
            for (int l = 0; l < N; l++)
            {
                for (int k = 0; k < N; k++)
                {
                    if (x + i + k >= 0 && y + j + l >= 0 && x + i + k < width && y + j + l < height && x + k < width && y + l < height)
                    {
                        double differences = 0;
                        differences = Math.Abs(C[x + k + (y + l) * width] - R[x + i + k + (y + j + l) * width]);
                        result += differences;
                        numRan++;
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

            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
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
            yCrCb = new YCrCb(Y, Cr, Cb, bitmap.Height, bitmap.Width, bitmap.Height, bitmap.Width);
            return yCrCb;
        }

        /// <summary>
        /// Convert Y Cr Cb channels back into bitmap
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">width of bitmap</param>
        /// <param name="height">height of bitmap</param>
        /// <returns></returns>
        public static Bitmap convertToBitmap(YCrCb yCrCb)
        {
            Bitmap bitmap = new Bitmap(yCrCb.yWidth, yCrCb.yHeight);
            int index = 0;
            Color[] pixels = new Color[yCrCb.Y.Length];
            
            for (int j = 0; j < yCrCb.yHeight; j++)
            {
                for (int i = 0; i < yCrCb.yWidth; i++)
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
        /// Convert a channel to a grayscale bitmap
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap convertToGrayscaleBitmap(byte[] channel, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            int index = 0;
            Color[] pixels = new Color[channel.Length];
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Color pixel = Color.FromArgb(channel[index], channel[index], channel[index]);
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
        public static YCrCb subSample(YCrCb yCrCb)
        {
            YCrCb subYCrCb;
            int reducedWidth = (int)Math.Ceiling(yCrCb.yWidth / 2.0);
            int reducedHeight = (int)Math.Ceiling(yCrCb.yHeight / 2.0);
            byte[] subCr = new byte[reducedWidth * reducedHeight];
            byte[] subCb = new byte[reducedWidth * reducedHeight];
            int originalIndex = 0;
            int index = 0;

            for (int i = 0; i < yCrCb.yHeight; i++)
            {
                for (int j = 0; j < yCrCb.yWidth; j++)
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
            subYCrCb = new YCrCb(yCrCb.Y, subCr, subCb, yCrCb.yHeight, yCrCb.yWidth, reducedHeight, reducedWidth);
            return subYCrCb;
        }

        /// <summary>
        /// Filled sub sampled Y Cr Cb channels with repeated values
        /// </summary>
        /// <param name="subYCrCb"> sub sampled Y Cr Cb channels</param>
        /// <returns></returns>
        public static YCrCb fillSubSample(YCrCb subYCrCb)
        {
            byte[] filledCr = new byte[subYCrCb.Y.Length];
            byte[] filledCb = new byte[subYCrCb.Y.Length];
            int unfilledIndex = 0;
            int index = 0;

            for (int i = 0; i < subYCrCb.yHeight; i++)
            {
                for (int j = 0; j < subYCrCb.yWidth; j++)
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
                        filledCr[index] = filledCr[index - subYCrCb.yWidth];
                        filledCb[index] = filledCb[index - subYCrCb.yWidth];
                    }
                    index++;
                }
            }
            return new YCrCb(subYCrCb.Y, filledCr, filledCb, subYCrCb.yHeight, subYCrCb.yWidth, subYCrCb.yHeight, subYCrCb.yWidth);
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
        /// <returns></returns>
        public static DYCrCb DiscreteCosineTransform(YCrCb yCrCb)
        {
            int numOfBlockRow = (int)Math.Ceiling((yCrCb.yHeight / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(yCrCb.yWidth / 8.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(yCrCb.crCbHeight / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(yCrCb.crCbWidth / 8.0);
            double[] modY = BlockTransform(ArrayTransform.byteArrayToDouble(yCrCb.Y),
                numOfBlockRow, numOfBlockColumn, yCrCb.yWidth, yCrCb.yHeight);
            double[] modCr = BlockTransform(ArrayTransform.byteArrayToDouble(yCrCb.Cr),
                reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            double[] modCb = BlockTransform(ArrayTransform.byteArrayToDouble(yCrCb.Cb),
                reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            return new DYCrCb(modY, modCr, modCb, yCrCb.yHeight, yCrCb.yWidth, yCrCb.crCbHeight, yCrCb.crCbWidth);
        }

        public static DYCrCb DiscreteCosineTransform(DYCrCb yCrCb)
        {
            int numOfBlockRow = (int)Math.Ceiling((yCrCb.yHeight / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(yCrCb.yWidth / 8.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(yCrCb.crCbHeight / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(yCrCb.crCbWidth / 8.0);
            double[] modY = BlockTransform(yCrCb.Y, numOfBlockRow, numOfBlockColumn, yCrCb.yWidth, yCrCb.yWidth);
            double[] modCr = BlockTransform(yCrCb.Cr, reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            double[] modCb = BlockTransform(yCrCb.Cb, reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            return new DYCrCb(modY, modCr, modCb, yCrCb.yHeight, yCrCb.yWidth, yCrCb.crCbHeight, yCrCb.crCbWidth);
        }

        /// <summary>
        /// Perform discrete cosine transformation on given Y Cr Cb channels of an image
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <returns></returns>
        public static YCrCb InverseDiscreteCosineTransform(DYCrCb yCrCb)
        {
            int numOfBlockRow = (int)Math.Ceiling((yCrCb.yHeight / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(yCrCb.yWidth / 8.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(yCrCb.crCbHeight / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(yCrCb.crCbWidth / 8.0);
            double[] modY = InverseBlockTransform(yCrCb.Y, numOfBlockRow, numOfBlockColumn,
                yCrCb.yWidth, yCrCb.yHeight);
            double[] modCr = InverseBlockTransform(yCrCb.Cr, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                yCrCb.crCbWidth, yCrCb.crCbHeight);
            double[] modCb = InverseBlockTransform(yCrCb.Cb, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                yCrCb.crCbWidth, yCrCb.crCbHeight);
            return new YCrCb(ArrayTransform.doubleArrayToByte(modY),
                ArrayTransform.doubleArrayToByte(modCr),
                ArrayTransform.doubleArrayToByte(modCb),
                yCrCb.yHeight, yCrCb.yWidth, yCrCb.crCbHeight, yCrCb.crCbWidth);
        }

        public static DYCrCb InverseDiscreteCosineTransformMPEG(DYCrCb yCrCb)
        {
            int numOfBlockRow = (int)Math.Ceiling((yCrCb.yHeight / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(yCrCb.yWidth / 8.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(yCrCb.crCbHeight / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(yCrCb.crCbWidth / 8.0);
            double[] modY = InverseBlockTransform(yCrCb.Y, numOfBlockRow, numOfBlockColumn,
                yCrCb.yWidth, yCrCb.yHeight);
            double[] modCr = InverseBlockTransform(yCrCb.Cr, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                 yCrCb.crCbWidth, yCrCb.crCbHeight);
            double[] modCb = InverseBlockTransform(yCrCb.Cb, reducedNumOfBlockRow, reducedNumOfBlockColumn,
                 yCrCb.crCbWidth, yCrCb.crCbHeight);
            return new DYCrCb(modY, modCr, modCb, yCrCb.yHeight, yCrCb.yWidth, yCrCb.crCbHeight, yCrCb.crCbWidth);
        }

        /// <summary>
        /// Quantize and zigzag reorder
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <returns>Quantized and Zigzagged Y Cr Cb structure</returns>
        public static YCrCb QuantizationAndZigzag(DYCrCb yCrCb)
        {
            int numOfBlockRow = (int)Math.Ceiling((yCrCb.yHeight / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(yCrCb.yWidth / 8.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(yCrCb.crCbHeight / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(yCrCb.crCbWidth / 8.0);
            byte[] modY = BlockQuantization(yCrCb.Y,
                numOfBlockRow, numOfBlockColumn, yCrCb.yWidth, yCrCb.yHeight);
            byte[] modCr = BlockQuantization(yCrCb.Cr,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            byte[] modCb = BlockQuantization(yCrCb.Cb,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            if (zigzag)
            {
                modY = ByteArrayToZigzag(modY, numOfBlockRow, numOfBlockColumn, yCrCb.yWidth, yCrCb.yHeight);
                modCr = ByteArrayToZigzag(modCr,
                    reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
                modCb = ByteArrayToZigzag(modCb,
                    reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            }
            return new YCrCb(modY, modCr, modCb, yCrCb.yHeight, yCrCb.yWidth, yCrCb.crCbHeight, yCrCb.crCbWidth);
        }

        /// <summary>
        /// Inverse quantization and zigzag pattern
        /// </summary>
        /// <param name="yCrCb">Y Cr Cb channels</param>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <returns></returns>
        public static DYCrCb InverseQuantizationAndZigzag(YCrCb yCrCb)
        {
            int numOfBlockRow = (int)Math.Ceiling((yCrCb.yHeight / 8.0));
            int numOfBlockColumn = (int)Math.Ceiling(yCrCb.yWidth / 8.0);
            int reducedNumOfBlockRow = (int)Math.Ceiling(yCrCb.crCbHeight / 8.0);
            int reducedNumOfBlockColumn = (int)Math.Ceiling(yCrCb.crCbWidth / 8.0);
            double[] modY = InverseBlockQuantization(yCrCb.Y,
                numOfBlockRow, numOfBlockColumn, yCrCb.yWidth, yCrCb.yHeight);
            double[] modCr = InverseBlockQuantization(yCrCb.Cr,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            double[] modCb = InverseBlockQuantization(yCrCb.Cb,
                reducedNumOfBlockRow, reducedNumOfBlockColumn, yCrCb.crCbWidth, yCrCb.crCbHeight);
            
            return new DYCrCb(modY, modCr, modCb, yCrCb.yHeight, yCrCb.yWidth, yCrCb.crCbHeight, yCrCb.crCbWidth);
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
                for (int v = 0; v < blockSize && (v + currentBlockRow * blockSize) < height; v++)
                {
                    if (v == 0)
                    {
                        cv = C_ZERO;
                    }
                    else
                    {
                        cv = C_NONZERO;
                    }
                    for (int u = 0; u < blockSize && u + currentBlockColumn * blockSize < width; u++)
                    {
                        if (u == 0)
                        {
                            cu = C_ZERO;
                        }
                        else
                        {
                            cu = C_NONZERO;
                        }
                        for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < height; y++)
                        {
                            for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < width; x++)
                            {
                                tempResult += channel[(y + currentBlockRow * blockSize) *
                                    width + (x + currentBlockColumn * blockSize)] *
                                    Math.Cos((2.0 * x + 1.0) * u * Math.PI / (2.0 * blockSize)) *
                                    Math.Cos((2.0 * y + 1.0) * v * Math.PI / (2.0 * blockSize));
                            }
                        }
                        tempResult *= (2.0 * cu * cv / Math.Sqrt(blockSize * blockSize));
                        result[(v + currentBlockRow * blockSize) * width + u + currentBlockColumn * blockSize] = tempResult;
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
                for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < height; y++)
                {

                    for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < width; x++)
                    {

                        for (int v = 0; v < blockSize && v + currentBlockRow * blockSize < height; v++)
                        {
                            if (v == 0)
                            {
                                cv = C_ZERO;
                            }
                            else
                            {
                                cv = C_NONZERO;
                            }
                            for (int u = 0; u < blockSize && u + currentBlockColumn * blockSize < width; u++)
                            {
                                if (u == 0)
                                {
                                    cu = C_ZERO;
                                }
                                else
                                {
                                    cu = C_NONZERO;
                                }
                                tempResult += channel[(v + currentBlockRow * blockSize) * width +
                                    (u + currentBlockColumn * blockSize)] *
                                    Math.Cos((2.0 * x + 1.0) * u * Math.PI / (2.0 * blockSize)) *
                                    Math.Cos((2.0 * y + 1.0) * v * Math.PI / (2.0 * blockSize)) *
                                    (2.0 * cu * cv / Math.Sqrt(blockSize * blockSize));
                            }
                        }
                        result[(y + currentBlockRow * blockSize) * width + x + currentBlockColumn * blockSize] = tempResult;
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

                for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < height; y++)
                {
                    for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < width; x++)
                    {
                        result[(y + currentBlockRow * blockSize) * width +
                            (x + currentBlockColumn * blockSize)] = getBoundedSByte(Math.Round((channel[(y + currentBlockRow * blockSize) * width +
                            (x + currentBlockColumn * blockSize)] / currentQuantizationTable[y * blockSize + x])));
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

                for (int y = 0; y < blockSize && y + currentBlockRow * blockSize < height; y++)
                {
                    for (int x = 0; x < blockSize && x + currentBlockColumn * blockSize < width; x++)
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

    }
}
