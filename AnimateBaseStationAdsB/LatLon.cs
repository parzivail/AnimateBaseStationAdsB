using System;

namespace AnimateBaseStationAdsB
{
    public class LatLon
    {
        public LatLon(DateTime time, double posMsgLatitude, double posMsgLongitude, double posMsgAltitude)
        {
            Time = time;
            Lat = posMsgLatitude;
            Lon = posMsgLongitude;
            Alt = posMsgAltitude;
        }

        public DateTime Time { get; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
    }
}