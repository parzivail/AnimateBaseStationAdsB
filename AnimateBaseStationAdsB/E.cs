using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AnimateBaseStationAdsB
{
    public static class E
    {
        public static string ReadNullTermString(this BinaryReader reader)
        {
            var list = new List<byte>();
            while (true)
            {
                var b = reader.ReadByte();
                if (b == 0)
                    break;
                list.Add(b);
            }
            return list.ToArray().GetString();
        }

        public static string GetString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static KeyValuePair<int, Bitmap> LoadGlTexture(this Bitmap bitmap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out int tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return new KeyValuePair<int, Bitmap>(tex, bitmap);
        }

        public static double GetEpoch()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Vector3 Remap(this Vector3 value, Vector3 from1, Vector3 to1, Vector3 from2, Vector3 to2)
        {
            return Vector3.Divide(value - from1, to1 - from1) * (to2 - from2) + from2;
        }

        public static Color Lerp(Color color1, Color color2, double fraction)
        {
            var r = Lerp(color1.R, color2.R, fraction);
            var g = Lerp(color1.G, color2.G, fraction);
            var b = Lerp(color1.B, color2.B, fraction);
            return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        public static double Lerp(double d1, double d2, double fraction)
        {
            return d1 + (d2 - d1) * fraction;
        }

        public static float Clamp(this float n, float min, float max)
        {
            return Math.Min(Math.Max(n, min), max);
        }
    }
}
