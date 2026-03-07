using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;

namespace UziSport.Service
{
    public static class EscPosImage
    {
        public static byte[] RasterImage(Bitmap bmp)
        {
            using var mono = ToMono(bmp);

            int width = mono.Width;
            int height = mono.Height;
            int widthBytes = (width + 7) / 8;

            var data = new List<byte>();

            // GS v 0
            data.AddRange(new byte[]
            {
            0x1D, 0x76, 0x30, 0x00,
            (byte)(widthBytes % 256), (byte)(widthBytes / 256),
            (byte)(height % 256), (byte)(height / 256)
            });

            for (int y = 0; y < height; y++)
            {
                for (int xb = 0; xb < widthBytes; xb++)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int x = xb * 8 + bit;
                        if (x < width)
                        {
                            var c = mono.GetPixel(x, y);
                            bool black = c.R == 0;
                            if (black) b |= (byte)(0x80 >> bit);
                        }
                    }
                    data.Add(b);
                }
            }

            return data.ToArray();
        }

        private static Bitmap ToMono(Bitmap src)
        {
            // copy sang 24bpp
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.White);
                g.DrawImage(src, 0, 0, src.Width, src.Height);
            }

            // threshold -> black/white
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    int gray = (c.R * 30 + c.G * 59 + c.B * 11) / 100;
                    bmp.SetPixel(x, y, gray < 160 ? System.Drawing.Color.Black : System.Drawing.Color.White);
                }

            return bmp;
        }
    }
}
