using System.IO;

namespace BaseStationDotNet
{
    public class ClickMessage : TelemetryMessage
    {
        public ClickMessage(string[] parts) : base(BsTypeCode.Click, parts)
        {
            if (Util.Get(parts, 3) != "-1" || Util.Get(parts, 5) != "-1")
                throw new InvalidDataException(Lang.ClickFieldsWereNotNull);
        }

        public ClickMessage(BinaryReader reader)
        {
            Deserialize(reader);
        }
    }
}