using System.IO;

namespace BaseStationDotNet
{
    public class NewIdMessage : TelemetryMessage
    {
        public string Callsign;

        public NewIdMessage(string[] parts) : base(BsTypeCode.NewId, parts)
        {
            Callsign = Util.Get(parts, 10);
        }


        public NewIdMessage(BinaryReader reader)
        {
            Deserialize(reader);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Callsign = reader.ReadString();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Callsign);
        }
    }
}