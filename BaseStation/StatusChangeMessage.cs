using System.IO;

namespace BaseStationDotNet
{
    public class StatusChangeMessage : TelemetryMessage
    {
        public BsStatus Status;

        public StatusChangeMessage(string[] parts) : base(BsTypeCode.StatusChange, parts)
        {
            switch (Util.Get(parts, 10))
            {
                case "PL":
                    Status = BsStatus.PositionLost;
                    break;
                case "SL":
                    Status = BsStatus.SignalLost;
                    break;
                case "RM":
                    Status = BsStatus.Remove;
                    break;
                case "AD":
                    Status = BsStatus.Delete;
                    break;
                case "OK":
                    Status = BsStatus.Ok;
                    break;
                default:
                    throw new InvalidDataException(string.Format(Lang.UnknownStatus, Util.Get(parts, 0)));
            }
        }

        public StatusChangeMessage(BinaryReader reader)
        {
            Deserialize(reader);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Status = (BsStatus) reader.ReadByte();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write((byte)Status);
        }
    }
}