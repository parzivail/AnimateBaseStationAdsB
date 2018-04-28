using System;
using System.Globalization;
using System.IO;

namespace BaseStationDotNet
{
    public class TelemetryMessage : IBinarySerializable
    {
        public BsTypeCode MessageType;
        public int TransmissionTypeId;
        public TransmissionTypes TransmissionType;
        public int SessionId;
        public int AircraftId;
        public string HexId;
        public int FlightId;
        public DateTime DateTimeGenerated;
        public DateTime DateTimeLogged;

        public TelemetryMessage()
        {
        }

        public TelemetryMessage(BsTypeCode typeMessageType, string[] parts)
        {
            MessageType = typeMessageType;

            int.TryParse(Util.Get(parts, 1), out TransmissionTypeId);
            int.TryParse(Util.Get(parts, 2), out SessionId);
            int.TryParse(Util.Get(parts, 3), out AircraftId);
            HexId = Util.Get(parts, 4);
            int.TryParse(Util.Get(parts, 5), out FlightId);

            DateTime.TryParse($"{Util.Get(parts, 6)} {Util.Get(parts, 7)}", out DateTimeGenerated);
            DateTime.TryParse($"{Util.Get(parts, 8)} {Util.Get(parts, 9)}", out DateTimeLogged);

            if (TransmissionTypeId != 0)
                TransmissionType = (TransmissionTypes)TransmissionTypeId;
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            MessageType = (BsTypeCode) reader.ReadByte();
            TransmissionTypeId = reader.ReadByte();
            if (TransmissionTypeId != 0)
                TransmissionType = (TransmissionTypes)TransmissionTypeId;
            SessionId = reader.ReadInt32();
            AircraftId = reader.ReadInt32();
            HexId = reader.ReadString();
            FlightId = reader.ReadInt32();
            DateTimeGenerated = DateTime.FromBinary(reader.ReadInt64());
            DateTimeLogged = DateTime.FromBinary(reader.ReadInt64());
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)MessageType);
            writer.Write((byte)TransmissionTypeId);
            writer.Write(SessionId);
            writer.Write(AircraftId);
            writer.Write(HexId);
            writer.Write(FlightId);
            writer.Write(DateTimeGenerated.ToBinary());
            writer.Write(DateTimeLogged.ToBinary());
        }
    }
}