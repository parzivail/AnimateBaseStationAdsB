using System.IO;
using System.Linq;

namespace BaseStationDotNet
{
    public class BaseStation
    {
        public static TelemetryMessage Parse(string data)
        {
            var parts = data.Split(',').Select(s => s.Trim().ToUpper()).ToArray();

            if (parts.Length < 10)
                return null;

            switch (Util.Get(parts, 0))
            {
                case "SEL":
                    return new SelectionChangeMessage(parts);
                case "ID":
                    return new NewIdMessage(parts);
                case "AIR":
                    return new NewAircraftMessage(parts);
                case "STA":
                    return new StatusChangeMessage(parts);
                case "CLK":
                    return new ClickMessage(parts);
                case "MSG":
                    return new TransmissionMessage(parts);
                default:
                    throw new InvalidDataException(string.Format(Lang.UnknownMessageType, Util.Get(parts, 0)));
            }
        }
    }
}