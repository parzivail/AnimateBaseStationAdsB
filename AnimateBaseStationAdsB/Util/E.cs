using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AnimateBaseStationAdsB.Util
{
    public static class E
    {
        /// <summary>
        /// Loads a bitmap into OpenGL
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return new KeyValuePair<int, Bitmap>(tex, bitmap);
        }

        /// <summary>
        /// Reads a null-terminated string from a binary stream
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Turns a byte array into a string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Returns the current epoch
        /// </summary>
        /// <returns></returns>
        public static double GetEpoch()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// Remaps a float from one range to another
        /// See https://processing.org/reference/map_.html
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start1"></param>
        /// <param name="stop1"></param>
        /// <param name="start2"></param>
        /// <param name="stop2"></param>
        /// <returns></returns>
        public static float Remap(this float value, float start1, float stop1, float start2, float stop2)
        {
            return (value - start1) / (stop1 - start1) * (stop2 - start2) + start2;
        }

        /// <summary>
        /// Remap for Vector3's
        /// </summary>
        /// <param name="value"></param>
        /// <param name="from1"></param>
        /// <param name="to1"></param>
        /// <param name="from2"></param>
        /// <param name="to2"></param>
        /// <returns></returns>
        public static Vector3 Remap(this Vector3 value, Vector3 from1, Vector3 to1, Vector3 from2, Vector3 to2)
        {
            return Vector3.Divide(value - from1, to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Linear interpolation for colors
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public static Color Lerp(Color color1, Color color2, double fraction)
        {
            var r = Lerp(color1.R, color2.R, fraction);
            var g = Lerp(color1.G, color2.G, fraction);
            var b = Lerp(color1.B, color2.B, fraction);
            return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        /// <summary>
        /// Linear interpolation for doubles
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public static double Lerp(double d1, double d2, double fraction)
        {
            return d1 + (d2 - d1) * fraction;
        }

        /// <summary>
        /// Clamps a value between a min and a max
        /// </summary>
        /// <param name="n"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(this float n, float min, float max)
        {
            return Math.Min(Math.Max(n, min), max);
        }
    }
}
