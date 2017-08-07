namespace AnimateBaseStationAdsB
{
    public class LatLon
    {
        public LatLon(double posMsgLatitude, double posMsgLongitude, double posMsgAltitude)
        {
            Lat = posMsgLatitude;
            Lon = posMsgLongitude;
            Alt = posMsgAltitude;
        }

        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
    }
}