using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using AnimateBaseStationAdsB.Util;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ErrorCode = AnimateBaseStationAdsB.Util.ErrorCode;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace AnimateBaseStationAdsB
{
    internal class MainWindow : GameWindow
    {
        /// <summary>
        /// All of the plane keyframes
        /// </summary>
        public PlaneTrack[] Planes { get; set; }
        /// <summary>
        /// Display text
        /// </summary>
        public string Text { get; set; } = "Init...";
        /// <summary>
        /// Current frame, for displaying progress
        /// </summary>
        public int Frame { get; set; }
        /// <summary>
        /// Current rotation of the map
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// Onscreen font
        /// </summary>
        public BitmapFont.BitmapFont Font { get; set; }

        /// <summary>
        /// Texture GL ID
        /// </summary>
        /// <remarks>
        /// If you're looking to put in your own map, here are the steps:
        /// 1) Launch the program with your keyframe data, and it'll spit out the window bounds (in min/max lat/lon) in the console
        /// 2) Obtain a map that is exactly those boundaries, I used TileMill
        /// 3) Replace map.png
        /// </remarks>
        private static int _texMap;

        /*
         * Texture size
         */
        private static float _mapWidth;
        private static float _mapHeight;

        /*
         * Window bounds in lat/lon
         */
        public double MapMinX;
        public double MapMaxX;

        public double MapMinY;
        public double MapMaxY;

        public double MapMinZ;
        public double MapMaxZ;

        /*
         * Window bounds in lat/lon, vector format for easy math
         */
        public Vector3 MinVector { get; set; }
        public Vector3 MaxVector { get; set; }
        public Vector3 WindowSize { get; set; }

        /// <summary>
        /// Time of the start of the animation
        /// </summary>
        public DateTime StartTime;
        /// <summary>
        /// Time of the end of the animation
        /// </summary>
        public DateTime EndTime;
        /// <summary>
        /// Current playhead position within the animation
        /// </summary>
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
            /*
             * How many seconds (or whatever unit you want) pass each frame
             * 
             * Uncomment those bottom lines if you wish to actually save the frames. If you 
             * just want to view in realtime, this slows it down, so it's fine to omit it.
             * We exit after one loop for saving purposes. To loop instead, replace the line with:
             * 
             * CurrentTime = StartTime;
             */
            CurrentTime = CurrentTime.AddSeconds(90);
            if (CurrentTime > EndTime)
                Environment.Exit(0);
            //else
            //    SaveScreen($"frames/{Frame++}.png");

            // Rotations/update in degrees
            Rotation += 0.1f;

            Title = $"{Frame} frames saved";
            Text = "Planes over Georgia\n" +
                    "@parzivail/cnewmanJax2012\n" +
                    $"Time: {CurrentTime}";
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            // Set up the viewport
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1000, 1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void MainWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            /*
             * General note:
             * There's a lot of OpenGL here so
             * I'll gloss over most of the calls.
             */

            GL.PushMatrix();

            // Draw the text
            GL.PushMatrix();
            GL.Color4(0f, 0f, 0f, 1f);
            GL.Translate(10, 10, -10);
            GL.Enable(EnableCap.Texture2D);
            Font.RenderString(Text);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            // Rotate the map
            GL.Translate(Width / 2f, Height / 2f, 0);
            GL.Rotate(60, 1, 0, 0); // Tilt towards the camera
            GL.Rotate(Rotation, 0, 0, 1); // Rotate around the middle
            GL.Translate(-Width / 2f, -Height / 2f, 0);

            // Draw the map
            GL.PushMatrix();
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, _texMap);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(1, 1, 1, 0.5f);
            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex2(Width, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex2(Width, Height);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, Height);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            GL.PushMatrix();
            foreach (var plane in Planes)
            {
                // Skip the plane if it's not flying at the time of the playhead
                if (plane.Start > CurrentTime || plane.End < CurrentTime)
                    continue;

                // Calculate the plane's distance along it's route at the current playhead position
                var curTimePercent = Math.Max((CurrentTime - plane.Start).TotalMilliseconds / (plane.End - plane.Start).TotalMilliseconds, double.Epsilon);

                GL.Begin(PrimitiveType.LineStrip);
                var d = 0.3f;

                // Draw the tail in 1/100th increments to create a smooth curve
                for (var i = curTimePercent - d; i < curTimePercent; i += d / 100)
                {
                    var point = plane.Spline.GetPoint(Math.Max(i, double.Epsilon));

                    // Color the segment based on it's distance from the plane and it's altitude
                    var distance = 1 - (curTimePercent - i) / d;
                    var altColor = point.Z.Remap(0, WindowSize.Z, 0, 1).Clamp(0, 1);
                    GL.Color4(0, altColor, 1 - altColor, distance);

                    GL.Vertex3(point);
                }
                GL.Vertex3(plane.Spline.GetPoint(curTimePercent));
                GL.End();

                // Draw the plane dot
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(plane.Spline.GetPoint(curTimePercent));
                GL.End();
            }
            GL.PopMatrix();

            GL.PopMatrix();
            SwapBuffers();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Setup OpenGL data
            GL.ClearColor(Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PointSize(4);
            GL.LineWidth(2);
            Lumberjack.Log("Loaded OpenGL settings");

            // Load the font
            Font = BitmapFont.BitmapFont.LoadBinaryFont("dina", Assets.FntDina, Assets.PageDina);

            // Load the map
            if (!File.Exists("map.png"))
                Lumberjack.Kill("Unable to locate 'map.png'", ErrorCode.FnfMap);
            var pair = new Bitmap("map.png").LoadGlTexture();
            _texMap = pair.Key;
            _mapWidth = pair.Value.Width;
            _mapHeight = pair.Value.Height;
            Lumberjack.Log("Loaded textures");

            // Set the window size according to the map size
            Width = (int)_mapWidth;
            Height = (int)_mapHeight;

            // Load the planes into a list from the keyframe file
            if (!File.Exists("map.png"))
                Lumberjack.Kill("Unable to locate 'keyframes.json'", ErrorCode.FnfKeyframes);
            try
            {
                Planes = JsonConvert.DeserializeObject<List<PlaneTrack>>(File.ReadAllText("keyframes.json"))
                    // ...but only the ones that have 2+ keyframes
                    .Where(track => track.Keyframes.Count > 1 &&
                                    // ...and have a valid date
                                    track.Start != DateTime.MinValue)
                    .ToArray();
            }
            catch (Exception)
            {
                Lumberjack.Kill("The keyframes were not in the correct format", ErrorCode.InvalidKeyframes);
            }
            Lumberjack.Log($"Loaded {Planes.Length} plane{(Planes.Length == 1 ? "" : "s")}");

            // Get just the keyframes so we can do some math
            var keyframes = Planes.SelectMany(kf => kf.Keyframes).ToArray();

            // Calculate the average plane position so we can cull outliers
            var avgX = keyframes.Select(ll => ll.Lon).Average();
            var avgY = keyframes.Select(ll => ll.Lat).Average();
            var avgZ = keyframes.Select(ll => ll.Alt).Average();
            Lumberjack.Log("Averaged keyframe data");

            // Only keep planes who are <10 degrees from the average lat/lon 
            keyframes = keyframes.Where(ll => Distance(ll.Lon, ll.Lat, avgX, avgY) < 10).ToArray();

            // Get the extremes for map bounding
            MapMinX = keyframes.Min(ll => ll.Lon);
            MapMaxX = keyframes.Max(ll => ll.Lon);
            MapMaxY = keyframes.Min(ll => ll.Lat);
            MapMinY = keyframes.Max(ll => ll.Lat);
            MapMaxZ = keyframes.Max(ll => ll.Alt);
            MapMinZ = keyframes.Min(ll => ll.Alt);

            // Create vectors for them, so we can do math quicker
            MinVector = new Vector3((float)MapMinX, (float)MapMinY, (float)MapMinZ);
            MaxVector = new Vector3((float)MapMaxX, (float)MapMaxY, (float)MapMaxZ);
            MinVector = new Vector3((float)MapMinX, (float)MapMinY, (float)MapMinZ);
            MaxVector = new Vector3((float)MapMaxX, (float)MapMaxY, (float)MapMaxZ);

            // Create a vector that describes the window boundaries
            // The Z component is the maximum interpolated height of the planes that OpenGL should render
            WindowSize = new Vector3(Width, Height, 100);

            // Find the extreme time boundaries
            StartTime = CurrentTime = Planes.Select(track => track.Start).Min();
            EndTime = Planes.Select(track => track.End).Max();
            Lumberjack.Log("Created boundaries");

            // Display the bounds
            Lumberjack.Log($"Window: {Width}x{Height}", LogLevel.Warn);
            Lumberjack.Log($"Bounds: lon({MapMaxX},{MapMinX}) lat({MapMinY},{MapMaxY}) alt({MapMinZ},{MapMaxZ})", LogLevel.Warn);

            // Pipe each plane's data into a spline
            foreach (var planeTrack in Planes)
                planeTrack.Spline =
                    new Spline3D(
                        planeTrack.Keyframes.Select(
                            ll =>
                                new Vector3((float)ll.Lon, (float)ll.Lat, (float)ll.Alt).Remap(MinVector, MaxVector,
                                    Vector3.Zero, WindowSize)).ToList());
            Lumberjack.Log("Calculated splines");
        }

        /// <summary>
        /// Cartesian distance formula
        /// </summary>
        /// <param name="x1">The X of point 1</param>
        /// <param name="y1">The Y of point 1</param>
        /// <param name="x2">The X of point 2</param>
        /// <param name="y2">The Y of point 2</param>
        /// <returns></returns>
        public double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Saves the current screen frame to a PNG
        /// </summary>
        /// <param name="filename">The PNG filename to save</param>
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