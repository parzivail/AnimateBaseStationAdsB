using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnimateBaseStationAdsB.Util;
using BaseStationDotNet;
using Newtonsoft.Json;
using OpenTK;

namespace AnimateBaseStationAdsB
{
    class Program
    {
        static void Main(string[] args)
        {
            var exportMode = args.Contains("--convert") || args.Contains("-c");

            if (exportMode)
            {
                /*
                 * Converts raw BaseStation data to keyframe data.
                 * We want keyframe data instead of BaseStation raw output because, it's smaller, and smaller = faster loading.
                 * You only need to do this once.
                 */
                ExportPlaneKeyframes();
            }
            else
            {
                /*
                 * This is the actual routine for rendering and exporting the frames. Before running
                 * the window, it makes sure './frames/' exists.
                 * More about that in MainWindow.cs
                 */
                Directory.CreateDirectory("frames");
                new MainWindow().Run(20, 60); // 20 updates/second, 60 FPS (+ vsync)
            }
        }

        private static void ExportPlaneKeyframes()
        {
            // Where we'll store our keyframes before we write them out
            var messages = new List<PlaneTrack>();

            if (!File.Exists("planelog.txt"))
                Lumberjack.Kill("Unable to locate 'planelog.txt'", ErrorCode.FnfPlaneLog);

            // Load the 'placelog.txt' file handle
            using (var sr = new StreamReader("planelog.txt"))
            {
                // Read until we hit the end of the file
                while (!sr.EndOfStream)
                {
                    // Read a line and parse it into a BaseStation message
                    var m = BaseStation.Parse(sr.ReadLine());

                    // Add a new plane to the keyframe list if all of the planes we've seen before aren't this one
                    if (messages.All(track => track.HexId != m.HexId))
                        messages.Add(new PlaneTrack(m));

                    switch (m.MessageType)
                    {
                        case BsTypeCode.NewAircraft:
                            // Add the start time if we encounter it
                            messages.First(track => track.HexId == m.HexId).Start = m.DateTimeGenerated;
                            break;
                        case BsTypeCode.StatusChange:
                            switch (((StatusChangeMessage) m).Status)
                            {
                                case BsStatus.PositionLost:
                                case BsStatus.SignalLost:
                                case BsStatus.Remove:
                                case BsStatus.Delete:
                                    // Add the end time if we encounter it
                                    messages.First(track => track.HexId == m.HexId).End = m.DateTimeGenerated;
                                    break;
                            }
                            break;
                        case BsTypeCode.TransmissionMessage:
                            if (m.TransmissionType == TransmissionTypes.AirbornePosition)
                            {
                                // If we see a message with a position, keyframe it
                                var posMsg = (TransmissionMessage) m;
                                messages.First(track => track.HexId == m.HexId)
                                    .Keyframes.Add(new LatLon(posMsg.Latitude, posMsg.Longitude, posMsg.Altitude));
                            }
                            break;
                    }
                }
            }

            // Write out the keyframe file
            try
            {
                File.WriteAllText("keyframes.json", JsonConvert.SerializeObject(messages));
            }
            catch (Exception e)
            {
                Lumberjack.Kill($"Unable to write 'keyframes.json': {e.Message}", ErrorCode.CouldntWriteKeyframes);
            }
        }
    }
}
