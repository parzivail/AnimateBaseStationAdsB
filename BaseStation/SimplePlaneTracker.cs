using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseStationDotNet
{
    public class SimplePlaneTracker
    {
        public List<TrackedPlane> Planes { get; set; } = new List<TrackedPlane>();

        public void Consume(string message)
        {
            Consume(BaseStation.Parse(message));
        }

        public void Consume(TelemetryMessage message)
        {
            switch (message.MessageType)
            {
                case BsTypeCode.NewAircraft:
                    Planes.Add(new TrackedPlane(message));
                    Console.Clear();
                    break;
                case BsTypeCode.NewId:
                    var foundId = Planes.Where(plane => plane.HexIdent == message.HexId).ToArray();
                    switch (foundId.Length)
                    {
                        case 0:
                            Planes.Add(new TrackedPlane(message) { Callsign = ((NewIdMessage)message).Callsign });
                            break;
                        case 1:
                            foundId[0].Callsign = ((NewIdMessage)message).Callsign;
                            break;
                        default:
                            break;
                    }
                    break;
                case BsTypeCode.StatusChange:
                    var foundStatus = Planes.Where(plane => plane.HexIdent == message.HexId).ToArray();
                    if (foundStatus.Length == 0 &&
                        ((StatusChangeMessage) message).Status == BsStatus.Ok)
                        Planes.Add(new TrackedPlane(message));
                    else if (foundStatus.Length == 1)
                        switch (((StatusChangeMessage) message).Status)
                        {
                            case BsStatus.PositionLost:
                            case BsStatus.SignalLost:
                            case BsStatus.Remove:
                            case BsStatus.Delete:
                                Planes.RemoveAll(plane => plane.HexIdent == message.HexId);
                                Console.Clear();
                                break;
                            default:
                                break;
                        }
                    break;
                case BsTypeCode.TransmissionMessage:
                    var foundTm = Planes.Where(plane => plane.HexIdent == message.HexId).ToArray();
                    switch (foundTm.Length)
                    {
                        case 0:
                            Planes.Add(new TrackedPlane(message));
                            break;
                        case 1:
                            foundTm[0].LoadMessage((TransmissionMessage)message);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
