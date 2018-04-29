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
                new VisualizerWindow().Run(20, 60); // 20 updates/second, 60 FPS (+ vsync)
            }
        }

        private static void ExportPlaneKeyframes()
        {
            // Where we'll store our keyframes while we're processing the BS data
            var tempMessages = new Dictionary<string, PlaneTrack>();

            // Where we'll store our keyframes before we write them out
            var compiledMessages = new List<PlaneTrack>();

            var consolidated = 0;
            
            // Keep track of all of the planes we've seen so far

            if (!File.Exists("planelog.txt"))
                Lumberjack.Kill("Unable to locate 'planelog.txt'", ErrorCode.FnfPlaneLog);

            Lumberjack.Log("Loading messages");

            // Load the 'placelog.txt' file handle
            var lines = File.ReadAllLines("planelog.txt");
            var bsmsgs = lines.Select(BaseStation.Parse);
            lines = null;

            Lumberjack.Log("Processing messages");
            foreach (var m in bsmsgs)
            {
                // Add a new plane to the keyframe list if all of the planes we've seen before aren't this one
                if (!tempMessages.ContainsKey(m.HexId))
                    tempMessages.Add(m.HexId, new PlaneTrack(m));
                else if (tempMessages[m.HexId].Keyframes.Count > 0)
                {
                    var thisTime = m.DateTimeGenerated;
                    var previousTime = tempMessages[m.HexId].Keyframes.Last().Time;
                    if (thisTime - previousTime > TimeSpan.FromMinutes(1))
                    {
                        consolidated++;
                        var oldMessages = tempMessages[m.HexId];
                        compiledMessages.Add(oldMessages);
                        tempMessages.Remove(m.HexId);
                        tempMessages.Add(m.HexId, new PlaneTrack(m));
                    }
                }

                switch (m.MessageType)
                {
                    case BsTypeCode.NewAircraft:
                        // Add the start time if we encounter it
                        tempMessages[m.HexId].Start = m.DateTimeGenerated;
                        break;
                    case BsTypeCode.StatusChange:
                        switch (((StatusChangeMessage)m).Status)
                        {
                            case BsStatus.PositionLost:
                            case BsStatus.SignalLost:
                            case BsStatus.Remove:
                            case BsStatus.Delete:
                                // Add the end time if we encounter it
                                tempMessages[m.HexId].End = m.DateTimeGenerated;
                                break;
                        }
                        break;
                    case BsTypeCode.TransmissionMessage:
                        if (m.TransmissionType == TransmissionTypes.AirbornePosition)
                        {
                            // If we see a message with a position, keyframe it
                            var posMsg = (TransmissionMessage)m;
                            tempMessages[m.HexId].Keyframes.Add(new LatLon(m.DateTimeGenerated, posMsg.Latitude, posMsg.Longitude, posMsg.Altitude));
                        }
                        break;
                }
            }

            foreach (var id in tempMessages.Keys)
                compiledMessages.Add(tempMessages[id]);

            compiledMessages.RemoveAll(track => track.Keyframes.Count < 2);

            Lumberjack.Log("Computing time constraints");
            foreach (var msg in compiledMessages)
            {
                msg.Start = msg.Keyframes.First().Time;
                msg.End = msg.Keyframes.Last().Time;
            }

            Lumberjack.Log($"Consolidated {consolidated} planes");

            Lumberjack.Log("Sorting messages");
            var allKeyframes = tempMessages.Values.OrderBy(track => track.Start);

            Lumberjack.Log("Saving messages");
            // Write out the keyframe file
            try
            {
                File.WriteAllText("keyframes.json", JsonConvert.SerializeObject(allKeyframes));
            }
            catch (Exception e)
            {
                Lumberjack.Kill($"Unable to write 'keyframes.json': {e.Message}", ErrorCode.CouldntWriteKeyframes);
            }

            Lumberjack.Log("Done.");
        }
    }
}
