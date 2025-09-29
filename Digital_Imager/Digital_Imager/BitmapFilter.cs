using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Digital_Imager
{
    public class ConvMatrix
    {
        public int TopLeft = 0, TopMid = 0, TopRight = 0;
        public int MidLeft = 0, Pixel = 1, MidRight = 0;
        public int BottomLeft = 0, BottomMid = 0, BottomRight = 0;
        public int Factor = 1;
        public int Offset = 0;

        public void SetAll(int nVal)
        {
            TopLeft = TopMid = TopRight = MidLeft = Pixel = MidRight =
                      BottomLeft = BottomMid = BottomRight = nVal;
        }
    }

    public class BitmapFilter
    {
        public static bool Conv3x3(Bitmap b, ConvMatrix m)
        {
            // Avoid divide by zero errors
            if (0 == m.Factor)
                return false;

            Bitmap bSrc = (Bitmap)b.Clone();

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                ImageLockMode.ReadWrite,
                                PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height),
                               ImageLockMode.ReadWrite,
                               PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            int stride2 = stride * 2;

            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* pSrc = (byte*)(void*)SrcScan0;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width - 2;
                int nHeight = b.Height - 2;

                int nPixel;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        // Blue channel
                        nPixel = (((pSrc[2] * m.TopLeft) +
                            (pSrc[5] * m.TopMid) +
                            (pSrc[8] * m.TopRight) +
                            (pSrc[2 + stride] * m.MidLeft) +
                            (pSrc[5 + stride] * m.Pixel) +
                            (pSrc[8 + stride] * m.MidRight) +
                            (pSrc[2 + stride2] * m.BottomLeft) +
                            (pSrc[5 + stride2] * m.BottomMid) +
                            (pSrc[8 + stride2] * m.BottomRight))
                            / m.Factor) + m.Offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[5 + stride] = (byte)nPixel;

                        // Green channel
                        nPixel = (((pSrc[1] * m.TopLeft) +
                            (pSrc[4] * m.TopMid) +
                            (pSrc[7] * m.TopRight) +
                            (pSrc[1 + stride] * m.MidLeft) +
                            (pSrc[4 + stride] * m.Pixel) +
                            (pSrc[7 + stride] * m.MidRight) +
                            (pSrc[1 + stride2] * m.BottomLeft) +
                            (pSrc[4 + stride2] * m.BottomMid) +
                            (pSrc[7 + stride2] * m.BottomRight))
                            / m.Factor) + m.Offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[4 + stride] = (byte)nPixel;

                        // Red channel
                        nPixel = (((pSrc[0] * m.TopLeft) +
                                   (pSrc[3] * m.TopMid) +
                                   (pSrc[6] * m.TopRight) +
                                   (pSrc[0 + stride] * m.MidLeft) +
                                   (pSrc[3 + stride] * m.Pixel) +
                                   (pSrc[6 + stride] * m.MidRight) +
                                   (pSrc[0 + stride2] * m.BottomLeft) +
                                   (pSrc[3 + stride2] * m.BottomMid) +
                                   (pSrc[6 + stride2] * m.BottomRight))
                            / m.Factor) + m.Offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[3 + stride] = (byte)nPixel;

                        p += 3;
                        pSrc += 3;
                    }

                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);
            return true;
        }

        // Smoothing Filter
        public static bool Smooth(Bitmap b, int nWeight = 1)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = nWeight;
            m.Factor = nWeight + 8;
            return Conv3x3(b, m);
        }

        // Gaussian Blur Filter
        public static bool GaussianBlur(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.TopRight = m.BottomLeft = m.BottomRight = 1;
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 2;
            m.Pixel = 4;
            m.Factor = 16;
            m.Offset = 0;
            return Conv3x3(b, m);
        }

        // Sharpen Filter
        public static bool Sharpen(Bitmap b, int nWeight = 11)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.TopRight = m.BottomLeft = m.BottomRight = 0;
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = -2;
            m.Pixel = nWeight;
            m.Factor = nWeight - 8;
            m.Offset = 0;
            return Conv3x3(b, m);
        }

        // Mean Removal Filter (Enhanced Sharpen)
        public static bool MeanRemoval(Bitmap b, int nWeight = 9)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.TopMid = m.TopRight = -1;
            m.MidLeft = m.MidRight = -1;
            m.BottomLeft = m.BottomMid = m.BottomRight = -1;
            m.Pixel = nWeight;
            m.Factor = 1;
            m.Offset = 0;
            return Conv3x3(b, m);
        }

        // Emboss Laplascian Filter
        public static bool EmbossLaplascian(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.TopRight = -1;
            m.TopMid = 0;
            m.MidLeft = 0;
            m.Pixel = 4;
            m.MidRight = 0;
            m.BottomLeft = -1;
            m.BottomMid = 0;
            m.BottomRight = -1;
            m.Factor = 1;
            m.Offset = 127;
            return Conv3x3(b, m);
        }

        // Emboss Horizontal/Vertical
        public static bool EmbossHorzVert(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = 0;
            m.TopMid = -1;
            m.TopRight = 0;
            m.MidLeft = -1;
            m.Pixel = 4;
            m.MidRight = -1;
            m.BottomLeft = 0;
            m.BottomMid = -1;
            m.BottomRight = 0;
            m.Factor = 1;
            m.Offset = 127;
            return Conv3x3(b, m);
        }

        // Emboss All Directions
        public static bool EmbossAllDirections(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.TopMid = m.TopRight = -1;
            m.MidLeft = m.MidRight = -1;
            m.BottomLeft = m.BottomMid = m.BottomRight = -1;
            m.Pixel = 8;
            m.Factor = 1;
            m.Offset = 127;
            return Conv3x3(b, m);
        }

        // Emboss Horizontal Only
        public static bool EmbossHorizontal(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.TopMid = m.TopRight = 0;
            m.MidLeft = -1;
            m.Pixel = 2;
            m.MidRight = -1;
            m.BottomLeft = m.BottomMid = m.BottomRight = 0;
            m.Factor = 1;
            m.Offset = 127;
            return Conv3x3(b, m);
        }

        // Emboss Vertical Only
        public static bool EmbossVertical(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.TopLeft = 0;
            m.TopMid = -1;
            m.TopRight = 0;
            m.MidLeft = m.Pixel = m.MidRight = 0;
            m.BottomLeft = 0;
            m.BottomMid = 1;
            m.BottomRight = 0;
            m.Factor = 1;
            m.Offset = 127;
            return Conv3x3(b, m);
        }
    }
}