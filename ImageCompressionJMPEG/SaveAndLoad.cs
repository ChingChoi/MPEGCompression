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
            int pFramesSpace = pFrames.Length * (pFrames[0].MotionVectorsY.Length * 6 +
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
                    YCrCb frame = iFrames[i / Compression.I_FRAME_RANGE];
                    System.Buffer.BlockCopy(frame.Y, 0, compressedByteArray, offset, frame.Y.Length);
                    offset += frame.Y.Length;
                    System.Buffer.BlockCopy(frame.Cr, 0, compressedByteArray, offset, frame.Cr.Length);
                    offset += frame.Cr.Length;
                    System.Buffer.BlockCopy(frame.Cb, 0, compressedByteArray, offset, frame.Cb.Length);
                    offset += frame.Cb.Length;
                }
                else
                {
                    PFrame frame = pFrames[pFrameIndex];
                    System.Buffer.BlockCopy(ArrayTransform.convertToByteFromVector(frame.MotionVectorsY), 0,
                        compressedByteArray, offset, frame.MotionVectorsY.Length * 2);
                    offset += frame.MotionVectorsY.Length * 2;
                    System.Buffer.BlockCopy(ArrayTransform.convertToByteFromVector(frame.MotionVectorsCr), 0,
                        compressedByteArray, offset, frame.MotionVectorsCr.Length * 2);
                    offset += frame.MotionVectorsCr.Length * 2;
                    System.Buffer.BlockCopy(ArrayTransform.convertToByteFromVector(frame.MotionVectorsCb), 0,
                        compressedByteArray, offset, frame.MotionVectorsCb.Length * 2);
                    offset += frame.MotionVectorsCb.Length * 2;
                    System.Buffer.BlockCopy(frame.DiffBlock.Y, 0, compressedByteArray,
                        offset, frame.DiffBlock.Y.Length);
                    offset += frame.DiffBlock.Y.Length;
                    System.Buffer.BlockCopy(frame.DiffBlock.Cr, 0, compressedByteArray,
                                            offset, frame.DiffBlock.Cr.Length);
                    offset += frame.DiffBlock.Cr.Length;
                    System.Buffer.BlockCopy(frame.DiffBlock.Cb, 0, compressedByteArray,
                                            offset, frame.DiffBlock.Cb.Length);
                    offset += frame.DiffBlock.Cb.Length;
                    pFrameIndex++;
                }
            }
            compressedByteArray = RLCompression.ModifiedRunLengthCompression(compressedByteArray);
            return compressedByteArray;
        }

        /// <summary>
        /// Loads a saved mpeg byte array
        /// </summary>
        /// <param name="savedArray">saved mpeg byte array</param>
        /// <returns>MPEGInfo of the saved mpeg images</returns>
        public static MPEGInfo loadByteArrayMPEG(byte[] savedArray)
        {
            savedArray = RLCompression.ModifiedRunLengthDecompress(savedArray);
            byte[] widthByteArray = new byte[Compression.intToByteSize];
            byte[] heightByteArray = new byte[Compression.intToByteSize];
            int offset = 0;
            System.Buffer.BlockCopy(savedArray, 0, widthByteArray, 0, Compression.intToByteSize);
            offset += Compression.intToByteSize;
            System.Buffer.BlockCopy(savedArray, offset, heightByteArray, 0, Compression.intToByteSize);
            offset += Compression.intToByteSize;

            int originalWidth = BitConverter.ToInt32(widthByteArray, 0);
            int originalHeight = BitConverter.ToInt32(heightByteArray, 0);
            // recalculate iframe width and height
            int iFrameYWidth = originalWidth;
            int iFrameYHeight = originalHeight;
            int iFrameCrCbWidth = (int)Math.Ceiling(originalWidth / 2.0);
            int iFrameCrCbHeight = (int)Math.Ceiling(originalHeight / 2.0);
            if (originalWidth % Compression.DCT_BLOCK_SIZE != 0)
            {
                iFrameYWidth += Compression.DCT_BLOCK_SIZE - (originalWidth % Compression.DCT_BLOCK_SIZE);
            }
            if (originalHeight % Compression.DCT_BLOCK_SIZE != 0)
            {
                iFrameYHeight += Compression.DCT_BLOCK_SIZE - (originalHeight % Compression.DCT_BLOCK_SIZE);
            }
            if (iFrameCrCbWidth % Compression.DCT_BLOCK_SIZE != 0)
            {
                iFrameCrCbWidth += Compression.DCT_BLOCK_SIZE - (iFrameCrCbWidth % Compression.DCT_BLOCK_SIZE);
            }
            if (iFrameCrCbHeight % Compression.DCT_BLOCK_SIZE != 0)
            {
                iFrameCrCbHeight += Compression.DCT_BLOCK_SIZE - (iFrameCrCbHeight % Compression.DCT_BLOCK_SIZE);
            }
            int pFrameYWidth = originalWidth;
            int pFrameYHeight = originalHeight;
            int pFrameCrCbWidth = (int)Math.Ceiling(originalWidth / 2.0);
            int pFrameCrCbHeight = (int)Math.Ceiling(originalHeight / 2.0);
            if (originalWidth % Compression.macroSizeY != 0)
            {
                pFrameYWidth += Compression.macroSizeY - (originalWidth % Compression.macroSizeY);
            }
            if (originalHeight % Compression.macroSizeY != 0)
            {
                pFrameYHeight += Compression.macroSizeY - (originalHeight % Compression.macroSizeY);
            }
            if (pFrameCrCbWidth % Compression.macroSizeCrCb != 0)
            {
                pFrameCrCbWidth += Compression.macroSizeCrCb - (pFrameCrCbWidth % Compression.macroSizeCrCb);
            }
            if (pFrameCrCbHeight % Compression.macroSizeCrCb != 0)
            {
                pFrameCrCbHeight += Compression.macroSizeCrCb - (pFrameCrCbHeight % Compression.macroSizeCrCb);
            }
            int motionVectorsSize = pFrameYWidth / Compression.macroSizeY * pFrameYHeight / Compression.macroSizeY;

            int iYCrCbSize = iFrameYWidth * iFrameYHeight + iFrameCrCbHeight * iFrameCrCbWidth * 2;
            int pYCrCbSize = pFrameYWidth * pFrameYHeight + pFrameCrCbHeight * pFrameCrCbWidth * 2;
            int pMVSize = motionVectorsSize * 6;

            int numOfFrames = 0;
            int numOfIFrames = 0;
            int numOfPFrames = 0;
            int currentRemaining = savedArray.Length - Compression.intToByteSize * 2;


            while (currentRemaining > 0)
            {
                if (numOfFrames % 10 == 0)
                {
                    currentRemaining -= iYCrCbSize;
                    numOfIFrames++;
                }
                else
                {
                    currentRemaining -= (pYCrCbSize + pMVSize);
                    numOfPFrames++;
                }
                numOfFrames++;
            }

            YCrCb[] iFrames = new YCrCb[numOfIFrames];
            PFrame[] pFrames = new PFrame[numOfPFrames];
            int pFrameIndex = 0;

            for (int i = 0; i < numOfFrames; i++)
            {
                byte[] iFrameY = new byte[iFrameYHeight * iFrameYWidth];
                byte[] iFrameCr = new byte[iFrameCrCbHeight * iFrameCrCbWidth];
                byte[] iFrameCb = new byte[iFrameCr.Length];
                byte[] pFrameY = new byte[pFrameYHeight * pFrameYWidth];
                byte[] pFrameCr = new byte[pFrameCrCbHeight * pFrameCrCbWidth];
                byte[] pFrameCb = new byte[pFrameCr.Length];
                byte[] motionVectorsYByte = new byte[motionVectorsSize * 2];
                byte[] motionVectorsCrByte = new byte[motionVectorsSize * 2];
                byte[] motionVectorsCbByte = new byte[motionVectorsSize * 2];

                if (i % Compression.I_FRAME_RANGE == 0)
                {
                    System.Buffer.BlockCopy(savedArray, offset, iFrameY, 0, iFrameY.Length);
                    offset += iFrameY.Length;
                    System.Buffer.BlockCopy(savedArray, offset, iFrameCr, 0, iFrameCr.Length);
                    offset += iFrameCr.Length;
                    System.Buffer.BlockCopy(savedArray, offset, iFrameCb, 0, iFrameCb.Length);
                    offset += iFrameCb.Length;
                    iFrames[i / Compression.I_FRAME_RANGE] = new YCrCb(iFrameY, iFrameCr, iFrameCb, 
                        iFrameYHeight, iFrameYWidth, iFrameCrCbHeight, iFrameCrCbWidth);
                }
                else
                {
                    System.Buffer.BlockCopy(savedArray, offset, motionVectorsYByte, 0, motionVectorsSize * 2);
                    offset += motionVectorsSize * 2;
                    System.Buffer.BlockCopy(savedArray, offset, motionVectorsCrByte, 0, motionVectorsSize * 2);
                    offset += motionVectorsSize * 2;
                    System.Buffer.BlockCopy(savedArray, offset, motionVectorsCbByte, 0, motionVectorsSize * 2);
                    offset += motionVectorsSize * 2;
                    System.Buffer.BlockCopy(savedArray, offset, pFrameY, 0, pFrameY.Length);
                    offset += pFrameY.Length;
                    System.Buffer.BlockCopy(savedArray, offset, pFrameCr, 0, pFrameCr.Length);
                    offset += pFrameCr.Length;
                    System.Buffer.BlockCopy(savedArray, offset, pFrameCb, 0, pFrameCb.Length);
                    offset += pFrameCb.Length;
                    YCrCb diffBlock = new YCrCb(pFrameY, pFrameCr, pFrameCb, pFrameYHeight, pFrameYWidth,
                        pFrameCrCbHeight, pFrameCrCbWidth);
                    pFrames[pFrameIndex++] = new PFrame(diffBlock, ArrayTransform.convertToVectorFromByte(motionVectorsYByte),
                        ArrayTransform.convertToVectorFromByte(motionVectorsCrByte), ArrayTransform.convertToVectorFromByte(motionVectorsCbByte));
                }
            }
            return new MPEGInfo(originalWidth, originalHeight, iFrames, pFrames);
        }

        /// <summary>
        /// Load saved byte array into JPEG info format
        /// </summary>
        /// <param name="savedArray">saved byte array</param>
        /// <returns>JPEG info format of loaded file</returns>
        public static JPEGInfo loadByteArrayJPEG(byte[] savedArray)
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
            int width = originalWidth;
            int height = originalHeight;
            if (originalHeight % Compression.DCT_BLOCK_SIZE != 0)
            {
                height += Compression.DCT_BLOCK_SIZE - (originalHeight % Compression.DCT_BLOCK_SIZE);
            }
            if (originalWidth % Compression.DCT_BLOCK_SIZE != 0)
            {
                width += Compression.DCT_BLOCK_SIZE - (originalWidth % Compression.DCT_BLOCK_SIZE);
            }
            int reducedWidth = (int)Math.Ceiling(originalWidth / 2.0);
            int reducedHeight = (int)Math.Ceiling(originalHeight / 2.0);
            if (reducedWidth % Compression.DCT_BLOCK_SIZE != 0)
            {
                reducedWidth += Compression.DCT_BLOCK_SIZE - (reducedWidth % Compression.DCT_BLOCK_SIZE);
            }
            if (reducedHeight % Compression.DCT_BLOCK_SIZE != 0)
            {
                reducedHeight += Compression.DCT_BLOCK_SIZE - (reducedHeight % Compression.DCT_BLOCK_SIZE);
            }

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
