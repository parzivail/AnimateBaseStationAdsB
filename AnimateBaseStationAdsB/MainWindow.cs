using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace AnimateBaseStationAdsB
{
    internal class MainWindow : GameWindow
    {
        public List<PlaneTrack> Planes { get; set; }
        public int Index { get; set; }

        private static int _texMap;

        private static float _mapWidth;
        private static float _mapHeight;
        public double MapMinX;
        public double MapMaxX;
        public double MapMinY;
        public double MapMaxY;

        public DateTime StartTime;
        public DateTime EndTime;
        public DateTime CurrentTime;

        public MainWindow() : base(960, 540)
        {
            Resize += MainWindow_Resize;
            Load += MainWindow_Load;
            RenderFrame += MainWindow_RenderFrame;
            UpdateFrame += MainWindow_UpdateFrame;
        }

        private void MainWindow_UpdateFrame(object sender, FrameEventArgs e)
        {
            CurrentTime = CurrentTime.AddSeconds(30);
            Title = $"{CurrentTime}";
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(MapMinX, MapMaxX, MapMaxY, MapMinY, 1, -1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void MainWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            GL.PushMatrix();
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, _texMap);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex2(_mapWidth, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex2(_mapWidth, _mapHeight);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, _mapHeight);
            GL.End();
            GL.Disable(EnableCap.Texture2D);

            GL.Color3(Color.Black);

            foreach (var plane in Planes)
            {
                if (plane.Start > CurrentTime || plane.End < CurrentTime)
                    continue;

                var pointsPer = (float)plane.Spline.SplineX.Xx.Length;
                GL.Begin(PrimitiveType.Points);
                var curTimePercent = (CurrentTime - plane.Start).TotalMilliseconds / (plane.End - plane.Start).TotalMilliseconds;
                GL.Vertex2(plane.Spline.GetPoint(curTimePercent));
                GL.End();
            }

            GL.PopMatrix();
            SwapBuffers();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.PointSize(4);
            Fx.D3.Init();

            var pair = LoadTexture("map.png");
            _texMap = pair.Key;
            _mapWidth = pair.Value.Width;
            _mapHeight = pair.Value.Height;

            var scaleCoefficient = 1;
            Width = (int)(_mapWidth / scaleCoefficient);
            Height = (int)(_mapHeight / scaleCoefficient);

            Planes = JsonConvert.DeserializeObject<List<PlaneTrack>>(File.ReadAllText("keyframes.json"))
                .Where(track => track.Keyframes.Count > 1 && track.Start != DateTime.MinValue)
                .ToList();
            var keyframes = Planes.SelectMany(kf => kf.Keyframes).ToArray();

            var avgX = keyframes.Select(ll => ll.Lon).Average();
            var avgY = keyframes.Select(ll => ll.Lat).Average();

            keyframes = keyframes.Where(ll => Distance(ll.Lon, ll.Lat, avgX, avgY) < 2).ToArray();

            MapMinX = keyframes.Select(ll => ll.Lon).Min();
            MapMaxX = keyframes.Select(ll => ll.Lon).Max();
            MapMaxY = keyframes.Select(ll => ll.Lat).Min();
            MapMinY = keyframes.Select(ll => ll.Lat).Max();

            StartTime = CurrentTime = Planes.Select(track => track.Start).Min();
            EndTime = Planes.Select(track => track.End).Max();

            foreach (var planeTrack in Planes)
                planeTrack.CreateSpline();
        }

        public double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public KeyValuePair<int, Bitmap> LoadTexture(string file)
        {
            var bitmap = new Bitmap(file);

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return new KeyValuePair<int, Bitmap>(tex, bitmap);
        }
    }
}