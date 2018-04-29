using System;
using System.Collections.Generic;
using System.Linq;
using AnimateBaseStationAdsB.Util;
using OpenTK;

namespace AnimateBaseStationAdsB
{
    internal class TimedSpline : Spline3D
    {
        public TimedSpline(List<KeyValuePair<DateTime, Vector3>> points)
        {
            Init(points);
        }

        protected void Init(List<KeyValuePair<DateTime, Vector3>> points)
        {
            var startTime = points.First().Key;
            var endTime = points.Last().Key;

            var times = points
                .Select(p => (p.Key - startTime).TotalMinutes / (endTime - startTime).TotalMinutes)
                .ToArray();
            var x = points.Select(p => (double)p.Value.X).ToArray();
            var y = points.Select(p => (double)p.Value.Y).ToArray();
            var z = points.Select(p => (double)p.Value.Z).ToArray();

            if (x.Length != y.Length || x.Length != z.Length)
            {
                throw new ArgumentException("Arrays must have the same length.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("Spline edges must have at least two points.");
            }

            /*
          Array representing the relative proportion of the total distance
          of each point in the line ( i.e. first point is 0.0, end point is
          1.0, a point halfway on line is 0.5 ).
         */
            var t = times;

            t[0] = 0.0; // start point is always 0.0
            t[t.Length - 1] = 1.0; // end point is always 1.0

            SplineX = new Spline(t, x);
            SplineY = new Spline(t, y);
            SplineZ = new Spline(t, z);
        }
    }
}