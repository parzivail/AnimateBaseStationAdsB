using System;
using System.IO;

namespace BaseStationDotNet
{
    public class TransmissionMessage : TelemetryMessage
    {
        public bool HasCallsign;
        public string Callsign;

        public bool HasAltitude;
        public double Altitude;

        public bool HasGroundSpeed;
        public double GroundSpeed;

        public bool HasGroundTrackAngle;
        public double GroundTrackAngle;

        public bool HasLatitude;
        public double Latitude;

        public bool HasLongitude;
        public double Longitude;

        public bool HasVerticalRate;
        public double VerticalRate;

        public bool HasSquawk;
        public int Squawk;

        public bool HasAlert;
        public bool Alert;

        public bool HasEmergency;
        public bool Emergency;

        public bool HasSpecialPositionIndicator;
        public bool SpecialPositionIndicator;

        public bool HasIsOnGround;
        public bool IsOnGround;

        public TransmissionMessage(string[] parts) : base(BsTypeCode.TransmissionMessage, parts)
        {
            if (TransmissionType == TransmissionTypes.Invalid)
                throw new InvalidDataException();

            //switch (TransmissionType)
            //{
            //    case TransmissionTypes.IdentityAndCategory:
            //        // 10
            //        break;
            //    case TransmissionTypes.SurfacePosition:
            //        // 11, 12, 13, 14, 15, 21
            //        break;
            //    case TransmissionTypes.AirbornePosition:
            //        // 11, 14, 15, 18, 19, 20, 21
            //        break;
            //    case TransmissionTypes.AirborneVelocity:
            //        // 12, 13, 16
            //        break;
            //    case TransmissionTypes.SurveillanceAltitude:
            //        // 11, 18, 20, 21
            //        break;
            //    case TransmissionTypes.SurveillanceIdentity:
            //        // 11, 17, 18, 19, 20, 21
            //        break;
            //    case TransmissionTypes.AirToAir:
            //        // 11, 21
            //        break;
            //    case TransmissionTypes.AllCallReply:
            //        // 21
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            HasCallsign = Util.Get(parts, 0) != "";
            Callsign = Util.Get(parts, 10);

            HasAltitude = double.TryParse(Util.Get(parts, 11), out Altitude);
            HasGroundSpeed = double.TryParse(Util.Get(parts, 12), out GroundSpeed);
            HasGroundTrackAngle = double.TryParse(Util.Get(parts, 13), out GroundTrackAngle);
            HasLatitude = double.TryParse(Util.Get(parts, 14), out Latitude);
            HasLongitude = double.TryParse(Util.Get(parts, 15), out Longitude);
            HasVerticalRate = double.TryParse(Util.Get(parts, 16), out VerticalRate);
            HasSquawk = int.TryParse(Util.Get(parts, 17), out Squawk);
            HasAlert = Util.Get(parts, 18) != "";
            Alert = Util.Get(parts, 18) == "1";
            HasEmergency = Util.Get(parts, 19) != "";
            Emergency = Util.Get(parts, 19) == "1";
            HasSpecialPositionIndicator = Util.Get(parts, 20) != "";
            SpecialPositionIndicator = Util.Get(parts, 20) == "1";
            HasIsOnGround = Util.Get(parts, 21) != "";
            IsOnGround = Util.Get(parts, 21) == "1";
        }

        public TransmissionMessage(BinaryReader reader)
        {
            Deserialize(reader);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Callsign = reader.ReadString();
            Altitude = reader.ReadDouble();
            GroundSpeed = reader.ReadDouble();
            GroundTrackAngle = reader.ReadDouble();
            Latitude = reader.ReadDouble();
            Longitude = reader.ReadDouble();
            VerticalRate = reader.ReadDouble();
            Squawk = reader.ReadInt32();
            Alert = reader.ReadBoolean();
            Emergency = reader.ReadBoolean();
            SpecialPositionIndicator = reader.ReadBoolean();
            IsOnGround = reader.ReadBoolean();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Callsign);
            writer.Write(Altitude);
            writer.Write(GroundSpeed);
            writer.Write(GroundTrackAngle);
            writer.Write(Latitude);
            writer.Write(Longitude);
            writer.Write(VerticalRate);
            writer.Write(Squawk);
            writer.Write(Alert);
            writer.Write(Emergency);
            writer.Write(SpecialPositionIndicator);
            writer.Write(IsOnGround);
        }
    }
}