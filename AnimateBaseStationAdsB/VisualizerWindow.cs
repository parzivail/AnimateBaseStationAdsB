using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimateBaseStationAdsB.Util;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace AnimateBaseStationAdsB
{
    internal class VisualizerWindow : GameWindow
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
        /// Percent through the animation
        /// </summary>
        public double PercentDone { get; set; }

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
         * Window bounds in lat/lon, vector format for easy math
         */
        public Vector3 MinVector { get; set; }
        public Vector3 MaxVector { get; set; }
        public Vector3 MapSize { get; set; }

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

        public VisualizerWindow() : base(960, 540)
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
            CurrentTime = CurrentTime.AddSeconds(15);
            if (CurrentTime > EndTime)
                Environment.Exit(0);
            //            else
            //                SaveScreen($"frames/{Frame++:D5}.png");

            // Rotations/update in degrees
            PercentDone = (CurrentTime - StartTime).TotalHours / (EndTime - StartTime).TotalHours;

            Title = $"{Frame} frames saved";
            Text = "@parzivail/cnewmanJax2012\n" +
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

            // Draw the into overlay
            GL.PushMatrix();
            GL.Color4(0f, 0f, 0f, 1f);

            GL.Translate(16, 0, -10);

            GL.Color4(0f, 0f, 0f, 1f);
            GL.LineWidth(1);
            GL.PointSize(5);
            GL.Disable(EnableCap.LineSmooth);
            // Draw the scrubber indicator
            Fx.D2.DrawLine(3, 2, 3, 13);
            Fx.D2.DrawLine(183, 2, 183, 13);
            Fx.D2.DrawLine(3, 7, 183, 7);
            
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2((float)(3 + 180 * PercentDone), 8);
            GL.End();

            GL.Enable(EnableCap.LineSmooth);

            GL.Translate(0, 16, 0);
            GL.Enable(EnableCap.Texture2D);
            Font.RenderString(Text);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            // Rotate the map
            GL.Translate(Width / 2f, Height / 2f, 0);
            GL.Rotate(60, 1, 0, 0); // Tilt towards the camera
            GL.Rotate(PercentDone * 2 * 360, 0, 0, 1); // Rotate around the middle
            GL.Translate(-MapSize.X / 2f, -MapSize.Y / 2f, 0);

            // Draw the map
            GL.PushMatrix();
            GL.PushAttrib(AttribMask.EnableBit);
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);
            GL.BindTexture(TextureTarget.Texture2D, _texMap);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(1, 1, 1, 0.5f);
            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex2(MapSize.X, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex2(MapSize.X, MapSize.Y);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, MapSize.Y);
            GL.End();
            GL.PopAttrib();
            GL.PopMatrix();

            GL.PushMatrix();
            foreach (var plane in Planes)
            {
                // Skip the plane if it's not flying at the time of the playhead
                if (plane.Start > CurrentTime || plane.End < CurrentTime)
                    continue;

                // Calculate the plane's distance along it's route at the current playhead position
                var curTimePercent = (CurrentTime - plane.Start).TotalMinutes / (plane.End - plane.Start).TotalMinutes;

                GL.PushAttrib(AttribMask.EnableBit);
                GL.Disable(EnableCap.DepthTest);
                GL.LineWidth(4);
                GL.PointSize(6);
                DrawPlaneWithTrail(curTimePercent, plane, true);

                GL.LineWidth(2);
                GL.PointSize(4);
                DrawPlaneWithTrail(curTimePercent, plane);
                GL.PopAttrib();

                GL.Color4(0f, 0f, 0f, 0.2f);
                GL.LineWidth(1);
                var pt = plane.Spline.GetPoint(curTimePercent);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(pt);
                pt.Z = 0;
                GL.Vertex3(pt);
                GL.End();
            }
            GL.PopMatrix();

            GL.PopMatrix();
            SwapBuffers();
        }

        private void DrawPlaneWithTrail(double curTimePercent, PlaneTrack plane, bool outline = false)
        {
            GL.Begin(PrimitiveType.LineStrip);
            var d = 0.3f;

            // Draw the tail in 1/100th increments to create a smooth curve
            for (var i = curTimePercent - d; i < curTimePercent; i += d / 50f)
            {
                var point = plane.Spline.GetPoint(Math.Max(i, double.Epsilon));

                // Color the segment based on it's distance from the plane and it's altitude
                var distance = 1 - (curTimePercent - i) / d;
                var altColor = point.Z.Remap(0, MapSize.Z, 0, 1).Clamp(0, 1);
                //GL.Color4(0, altColor, 1 - altColor, distance);
                GL.Color4(0, outline ? 0 : 1, 0, distance);

                GL.Vertex3(point);
            }

            GL.Vertex3(plane.Spline.GetPoint(curTimePercent));
            GL.End();

            // Draw the plane dot
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(plane.Spline.GetPoint(curTimePercent));
            GL.End();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Setup OpenGL data
            GL.ClearColor(Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PointSize(4);
            GL.LineWidth(2);
            Lumberjack.Log("Loaded OpenGL settings");

            // Load the font
            Font = BitmapFont.BitmapFont.LoadBinaryFont("dina", Assets.FntDina, Assets.PageDina);

            // Load the map
            if (!File.Exists("map.png"))
                Lumberjack.Kill("Unable to locate 'map.png'", Util.ErrorCode.FnfMap);
            var pair = new Bitmap("map.png").LoadGlTexture();
            _texMap = pair.Key;
            _mapWidth = pair.Value.Width;
            _mapHeight = pair.Value.Height;
            Lumberjack.Log("Loaded textures");

            // Set the window size according to the map size and
            // make the map fit the whole diagonal of the map while rotating
            Width = (int)Math.Sqrt(Math.Pow(_mapWidth, 2) + Math.Pow(_mapHeight, 2));
            Height = (int)_mapHeight;

            // Load the planes into a list from the keyframe file
            if (!File.Exists("keyframes.json"))
                Lumberjack.Kill("Unable to locate 'keyframes.json'", Util.ErrorCode.FnfKeyframes);
            try
            {
                Planes = JsonConvert.DeserializeObject<List<PlaneTrack>>(File.ReadAllText("keyframes.json"))
                        // ...but only the ones that have 2+ keyframes
                        .Where(track => track.Keyframes.Count > 1)
                    .ToArray();
            }
            catch (Exception)
            {
                Lumberjack.Kill("The keyframes were not in the correct format", Util.ErrorCode.InvalidKeyframes);
            }
            Lumberjack.Log($"Loaded {Planes.Length} plane{(Planes.Length == 1 ? "" : "s")}");

            // Get just the keyframes so we can do some math
            var keyframes = Planes.SelectMany(kf => kf.Keyframes).ToArray();

            // Get the extremes for map bounding
            var mapMinX = keyframes.Min(ll => ll.Lon);
            var mapMaxX = keyframes.Max(ll => ll.Lon);

            var mapMinY = keyframes.Min(ll => ll.Lat);
            var mapMaxY = keyframes.Max(ll => ll.Lat);

            var mapMaxZ = keyframes.Max(ll => ll.Alt);
            var mapMinZ = keyframes.Min(ll => ll.Alt);

            // Create vectors for them, so we can do math quicker
            MinVector = new Vector3((float)mapMinX, (float)mapMinY, (float)mapMinZ);
            MaxVector = new Vector3((float)mapMaxX, (float)mapMaxY, (float)mapMaxZ);

            // Create a vector that describes the window boundaries
            // The Z component is the maximum interpolated height of the planes that OpenGL should render
            MapSize = new Vector3(_mapWidth, _mapHeight, 100);

            // Find the extreme time boundaries
            StartTime = CurrentTime = Planes.Select(track => track.Start).Min();
            EndTime = Planes.Select(track => track.End).Max();
            Lumberjack.Log("Created boundaries");

            // Display the bounds
            Lumberjack.Log($"Window: {Width}x{Height}", LogLevel.Warn);
            Lumberjack.Log($"Bounds: lon({mapMinX},{mapMaxX}) lat({mapMinY},{mapMaxY}) alt({mapMinZ},{mapMaxZ})", LogLevel.Warn);
            Lumberjack.Log($"Bounds (TileMill): {mapMinX},{mapMinY},{mapMaxX},{mapMaxY}", LogLevel.Warn);

            // Pipe each plane's data into a spline
            foreach (var planeTrack in Planes)
                planeTrack.Spline =
                    new TimedSpline(
                        planeTrack.Keyframes.Select(
                            ll =>
                                new KeyValuePair<DateTime, Vector3>(ll.Time, new Vector3((float)ll.Lon, (float)ll.Lat, (float)ll.Alt).Remap(MinVector, MaxVector,
                                    Vector3.Zero, MapSize))).ToList());
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
