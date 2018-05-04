# MPEGCompression

Basic feature

Compresses bitmap image/video(bitmaps) using JPEG and MPEG techniques

Compresses bitmap image using Wavelet technique

Does not include complete JPG / MPEG header, hence not compatible with regular PC, only this program can decode the compressed image using CJPG or CMPEG extension

Program implements multi-threading on some methods only (performance was not main goal)

Wavelet compression saving feature is absent

How to use - Compress a single bitmap image using JPEG and save
1. Open a bitmap image file in File -> Open [Ctrl + Shift + O]
2. Compress the image in Edit -> JPEG [Ctrl + Shift + J]
3. Save the result in File -> Save [Ctrl + Shift + S]

Original bitmap image (left), compressed bitmap image (right)
![demo-jpg](https://github.com/ChingChoi/MPEGCompression/blob/master/Resource/img/demo-jpg.png)

Size is 3% of original image
![demo-jpg-compression](https://github.com/ChingChoi/MPEGCompression/blob/master/Resource/img/demo-jpg-compression.png)

Cr Cb channel view
![demo-crcb](https://github.com/ChingChoi/MPEGCompression/blob/master/Resource/img/demo-crcb.png)

How to use - Compress a video (bitmap images) using JPEG + MPEG and save
1. Click "M" button to enter MPEG mode
2. Open a list of bitmap images in File -> Open [Ctrl + Shift + O]
3. Compress the bitmap images in Edit -> Video -> MPEG [Ctrl + Shift + M]
4. Save the result in File -> Save [Ctrl + Shift + S]

Compressed video view
![demo-mpg](https://github.com/ChingChoi/MPEGCompression/blob/master/Resource/img/demo-mpg.png)

Size is roughly 25 times smaller than all 360 frames altogether
![demo-mpg-compress](https://github.com/ChingChoi/MPEGCompression/blob/master/Resource/img/demo-mpg-compress.png)


How to use - Compress a single bitmap image using Wavelet (short proof of concept implementation - currently not compatible with images width different from height)
1. Open a bitmap image file in File -> Open [Ctrl + Shift + O]
2. Compress the image in Edit -> Wavelet [Ctrl + Shift + W]

Note:
The program is based on JPEG, MPEG theory

Steps behind the compression
JPEG
1. RGB -> Y Cr Cb channels.
2. Down-sample Cr Cb channels (compression of 50% of image size)
3. Discrete cosine transform on 8x8 blocks of pixel (extract intensity info)
4. Quantize (using standard quantization table for JPEG)
5. Zigzag every 8x8 block and run length encoding (Instead of standard Entropy Encoding)

MPEG
1. JPEG a frame to use as I frame (1st frame and every 10th frame)
2. Discrete cosine transform current frame
3. Quantize current frame
4. Motion vector to find difference block on 8x8 block using current frame vs I frame
5. Repeat for all frames
6. Zigzag and run length encoding 

Wavelet
1. Primal lift and Dual lift every row
2. Transpose and repeat for half the rows
3. Transpose and repeat primal lift and dual lift with half col of prev step
4. Repeat until only 1 pixel remaining
5. Compress by removing detail portion of the result