using System.IO;

namespace BaseStationDotNet
{
    public class NewAircraftMessage : TelemetryMessage
    {
        public NewAircraftMessage(string[] parts) : base(BsTypeCode.NewAircraft, parts)
        {
        }

        public NewAircraftMessage(BinaryReader reader)
        {
            Deserialize(reader);
        }
    }
}