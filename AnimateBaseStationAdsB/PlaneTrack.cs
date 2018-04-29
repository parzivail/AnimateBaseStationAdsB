using System;
using System.Collections.Generic;
using AnimateBaseStationAdsB.Util;
using BaseStationDotNet;
using Newtonsoft.Json;

namespace AnimateBaseStationAdsB
{
    public class PlaneTrack
    {
        public PlaneTrack(TelemetryMessage telemetryMessage)
        {
            HexId = telemetryMessage.HexId;
        }

        public PlaneTrack()
        {
        }

        public string HexId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<LatLon> Keyframes { get; set; } = new List<LatLon>();

        [JsonIgnore]
        public Spline3D Spline { get; set; }
    }
}