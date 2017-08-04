using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseStationDotNet;
using Newtonsoft.Json;

namespace AnimateBaseStationAdsB
{
    class Program
    {
        static void Main(string[] args)
        {
            //ExportPlaneKeyframes();

            new MainWindow().Run(10);
        }

        private static void ExportPlaneKeyframes()
        {
            var messages = new List<PlaneTrack>();

            using (var sr = new StreamReader("planelog.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var m = BaseStation.Parse(sr.ReadLine());

                    if (messages.All(track => track.HexId != m.HexId))
                        messages.Add(new PlaneTrack(m));

                    switch (m.MessageType)
                    {
                        case BsTypeCode.NewAircraft:
                            messages.First(track => track.HexId == m.HexId).Start = m.DateTimeGenerated;
                            break;
                        case BsTypeCode.StatusChange:
                            switch (((StatusChangeMessage) m).Status)
                            {
                                case BsStatus.PositionLost:
                                case BsStatus.SignalLost:
                                case BsStatus.Remove:
                                case BsStatus.Delete:
                                    messages.First(track => track.HexId == m.HexId).End = m.DateTimeGenerated;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case BsTypeCode.TransmissionMessage:
                            if (m.TransmissionType == TransmissionTypes.AirbornePosition)
                            {
                                var posMsg = (TransmissionMessage) m;
                                messages.First(track => track.HexId == m.HexId)
                                    .Keyframes.Add(new LatLon(posMsg.Latitude, posMsg.Longitude));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            File.WriteAllText("keyframes.json", JsonConvert.SerializeObject(messages));
            Console.ReadKey();
        }
    }

    public class PlaneTrack
    {
        public PlaneTrack(TelemetryMessage telemetryMessage)
        {
            HexId = telemetryMessage.HexId;
        }

        public PlaneTrack()
        {
        }

        public string HexId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<LatLon> Keyframes { get; set; } = new List<LatLon>();
        public Spline2D Spline { get; set; }

        public void CreateSpline()
        {
            Spline = new Spline2D(Keyframes);
        }

    }

    public class LatLon
    {
        public LatLon(double posMsgLatitude, double posMsgLongitude)
        {
            Lat = posMsgLatitude;
            Lon = posMsgLongitude;
        }

        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}
