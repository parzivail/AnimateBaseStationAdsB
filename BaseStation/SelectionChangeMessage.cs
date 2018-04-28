using System.IO;

namespace BaseStationDotNet
{
    public class SelectionChangeMessage : TelemetryMessage
    {
        public string Callsign;

        public SelectionChangeMessage(string[] parts) : base(BsTypeCode.SelectionChange, parts)
        {
            Callsign = Util.Get(parts, 10);
        }
        
        public SelectionChangeMessage(BinaryReader reader)
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