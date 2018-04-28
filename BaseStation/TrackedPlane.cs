namespace BaseStationDotNet
{
    public class TrackedPlane
    {
        public int AircraftId;
        public string HexIdent;
        public int FlightId;
        public string Callsign;

        public double Altitude;
        public double GroundSpeed;
        public double GroundTrackAngle;
        public double Latitude;
        public double Longitude;
        public double VerticalRate;
        public double Squawk;
        public bool Alert;
        public bool Emergency;
        public bool SpecialPositionIndicator;
        public bool IsOnGround;

        public TrackedPlane(TelemetryMessage message)
        {
            AircraftId = message.AircraftId;
            HexIdent = message.HexId;
            FlightId = message.FlightId;

            var transmissionMessage = message as TransmissionMessage;
            if (transmissionMessage != null)
                LoadMessage(transmissionMessage);
        }

        public void LoadMessage(TransmissionMessage message)
        {
            if (message.HasCallsign)
                Callsign = message.Callsign;
            if (message.HasAltitude)
                Altitude = message.Altitude;
            if (message.HasGroundSpeed)
                GroundSpeed = message.GroundSpeed;
            if (message.HasGroundTrackAngle)
                GroundTrackAngle = message.GroundTrackAngle;
            if (message.HasLatitude)
                Latitude = message.Latitude;
            if (message.HasLongitude)
                Longitude = message.Longitude;
            if (message.HasVerticalRate)
                VerticalRate = message.VerticalRate;
            if (message.HasSquawk)
                Squawk = message.Squawk;
            if (message.HasAlert)
                Alert = message.Alert;
            if (message.HasEmergency)
                Emergency = message.Emergency;
            if (message.HasSpecialPositionIndicator)
                SpecialPositionIndicator = message.SpecialPositionIndicator;
            if (message.HasIsOnGround)
                IsOnGround = message.IsOnGround;
        }
    }
}
