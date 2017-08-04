using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace AnimateBaseStationAdsB
{
    internal class MainWindow : GameWindow
    {
        public List<PlaneTrack> Planes { get; set; }
        public int Index { get; set; }

        public BitmapFont.BitmapFont Font { get; set; }

        private static int _texMap;

        private static float _mapWidth;
        private static float _mapHeight;

        public Vector2 WindowSize { get; set; }

        public double MapMinX;
        public double MapMaxX;
        public double MapMinY;
        public double MapMaxY;
        public Vector2 MinVector { get; set; }
        public Vector2 MaxVector { get; set; }

        public bool NewData { get; set; }
        public int Frame { get; set; }

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
            CurrentTime = EndTime;//CurrentTime.AddMinutes(5);
            if (CurrentTime > EndTime)
                //Environment.Exit(0);
                CurrentTime = StartTime;
            else
                NewData = true;
            Title = "Planes over Georgia\n" +
                    "@parzivail/cnewmanJax2012\n" +
                    $"Time: {CurrentTime}";
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 100, -100);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void MainWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            //GL.PushMatrix();
            //GL.Translate(0, 0, 0);
            //GL.Enable(EnableCap.Texture2D);
            //GL.Color3(Color.White);
            //GL.BindTexture(TextureTarget.Texture2D, _texMap);
            //GL.Begin(PrimitiveType.Quads);
            //GL.TexCoord2(0, 0);
            //GL.Vertex2(0, 0);
            //GL.TexCoord2(1, 0);
            //GL.Vertex2(_mapWidth, 0);
            //GL.TexCoord2(1, 1);
            //GL.Vertex2(_mapWidth, _mapHeight);
            //GL.TexCoord2(0, 1);
            //GL.Vertex2(0, _mapHeight);
            //GL.End();
            //GL.Disable(EnableCap.Texture2D);

            GL.Color4(0f, 0f, 0f, 0.08f);

            GL.PushMatrix();
            GL.Scale(1, 1, 0.001f);
            foreach (var plane in Planes)
            {
                if (plane.Start > CurrentTime)// || plane.End < CurrentTime)
                    continue;

                var curTimePercent = (CurrentTime - plane.Start).TotalMilliseconds / (plane.End - plane.Start).TotalMilliseconds;
                //var trans = plane.Spline.GetPoint(curTimePercent);
                //var trans2 = plane.Spline.GetPoint(curTimePercent + 0.01);
                //GL.PushMatrix();
                //GL.Translate(trans.X, trans.Y, 0);
                //GL.Rotate(Math.Atan2(trans2.Y - trans.Y, trans2.X - trans.X) / Math.PI * 180, 0, 0, 1);
                //GL.Scale(2, 2, 2);
                //GL.Begin(PrimitiveType.LineStrip);
                //GL.Vertex2(0.007f, 0);
                //GL.Vertex2(0, -0.007f);
                //GL.Vertex2(0.007f, 0);
                //GL.Vertex2(0, 0.007f);
                //GL.End();
                //GL.PopMatrix();

                GL.Translate(0, 0, -1);

                GL.Begin(PrimitiveType.LineStrip);
                var l = (float)plane.Spline.SplineX.Xx.Length;
                for (var i = 0; i <= l && i / l < curTimePercent; i++)
                    GL.Vertex2(plane.Spline.GetPoint(i / l).Remap(MinVector, MaxVector, Vector2.Zero, WindowSize));
                GL.End();
            }
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Color4(0f, 0f, 0f, 1f);
            GL.Translate(10, 10, -10);
            GL.Enable(EnableCap.Texture2D);
            Font.RenderString(Title);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            GL.PopMatrix();
            SwapBuffers();

            if (!NewData) return;

            //SaveScreen($"frames/{Frame++}.png");
            //NewData = false;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PointSize(4);
            Fx.D3.Init();

            var pair = new Bitmap("map.png").LoadGlTexture();
            _texMap = pair.Key;
            _mapWidth = pair.Value.Width;
            _mapHeight = pair.Value.Height;
            
            Planes = JsonConvert.DeserializeObject<List<PlaneTrack>>(File.ReadAllText("keyframes.json"))
                .Where(track => track.Keyframes.Count > 1 && track.Start != DateTime.MinValue)
                .ToList();
            var keyframes = Planes.SelectMany(kf => kf.Keyframes).ToArray();

            var avgX = keyframes.Select(ll => ll.Lon).Average();
            var avgY = keyframes.Select(ll => ll.Lat).Average();

            keyframes = keyframes.Where(ll => Distance(ll.Lon, ll.Lat, avgX, avgY) < 10).ToArray();

            MapMinX = keyframes.Select(ll => ll.Lon).Min();
            MapMaxX = keyframes.Select(ll => ll.Lon).Max();
            MapMaxY = keyframes.Select(ll => ll.Lat).Min();
            MapMinY = keyframes.Select(ll => ll.Lat).Max();

            MinVector = new Vector2((float)MapMinX, (float)MapMinY);
            MaxVector = new Vector2((float)MapMaxX, (float)MapMaxY);
            
            StartTime = CurrentTime = Planes.Select(track => track.Start).Min();
            EndTime = Planes.Select(track => track.End).Max();

            var size = 900;
            var ratio = (MapMinY - MapMaxY) / (MapMaxX - MapMinX);
            Width = (int) (size / ratio);
            Height = size;

            WindowSize = new Vector2(Width, Height);

            Console.WriteLine("Bounds:\n" +
                              $"Lat: {MapMinY} {MapMaxY}\n" +
                              $"Lon: {MapMinX} {MapMaxX}");

            foreach (var planeTrack in Planes)
                planeTrack.CreateSpline();

            Font = BitmapFont.BitmapFont.LoadBinaryFont("dina", Assets.FntDina, Assets.PageDina);

            Directory.CreateDirectory("frames");
        }

        public double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public void SaveScreen(string filename)
        {
            using (var bmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height))
            {
                var data = bmp.LockBits(ClientRectangle, ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(0, 0, ClientRectangle.Width, ClientRectangle.Height, PixelFormat.Bgr,
                    PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                bmp.Save(filename, ImageFormat.Png);
            }
        }
    }
}