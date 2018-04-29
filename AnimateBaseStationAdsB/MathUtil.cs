using System;
using System.Linq;
using AnimateBaseStationAdsB;

static class MathUtil
{
    public static double ComputeFlightLength(PlaneTrack track)
    {
        return track.Keyframes.Select((t, i) => i == 0 ? 0 : DistanceTo(track.Keyframes[i - 1].Lat, track.Keyframes[i - 1].Lon, t.Lat, t.Lon)).Sum();
    }

    public static double DistanceTo(double lat1, double lon1, double lat2, double lon2)
    {
        var rlat1 = Math.PI * lat1 / 180;
        var rlat2 = Math.PI * lat2 / 180;
        var theta = lon1 - lon2;
        var rtheta = Math.PI * theta / 180;
        var dist =
            Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
            Math.Cos(rlat2) * Math.Cos(rtheta);
        dist = Math.Acos(dist);
        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;

        // miles
        return dist;
    }
}