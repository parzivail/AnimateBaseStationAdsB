﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace AnimateBaseStationAdsB
{
    public class Spline3D
    {
        public Spline SplineX;
        public Spline SplineY;
        public Spline SplineZ;
        /**
         * Total length tracing the points on the spline
         */
        private double _length;

        /**
         * Creates a new Spline2D.
         *
         * @param points
         */
        public Spline3D(IReadOnlyList<LatLon> points)
        {
            var x = new double[points.Count];
            var y = new double[points.Count];
            var z = new double[points.Count];

            for (var i = 0; i < points.Count; i++)
            {
                x[i] = points[i].Lon;
                y[i] = points[i].Lat;
                z[i] = points[i].Alt;
            }

            Init(x, y, z);
        }

        private void Init(double[] x, double[] y, double[] z)
        {
            if (x.Length != y.Length)
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
            var t = new double[x.Length];
            t[0] = 0.0; // start point is always 0.0

            // Calculate the partial proportions of each section between each set
            // of points and the total length of sum of all sections
            for (var i = 1; i < t.Length; i++)
            {
                var lx = x[i] - x[i - 1];
                var ly = y[i] - y[i - 1];
                var lz = z[i] - z[i - 1];
                t[i] = Math.Sqrt(lx * lx + ly * ly + lz * lz);

                _length += t[i];
                t[i] += t[i - 1];
            }

            for (var i = 1; i < (t.Length) - 1; i++)
            {
                t[i] = t[i] / _length;
            }

            t[(t.Length) - 1] = 1.0; // end point is always 1.0

            SplineX = new Spline(t, x);
            SplineY = new Spline(t, y);
            SplineZ = new Spline(t, z);
        }

        /**
         * @param t 0 <= t <= 1
         */
        public Vector3 GetPoint(double t)
        {
            return new Vector3((float)SplineX.GetValue(t), (float)SplineY.GetValue(t), (float)SplineZ.GetValue(t));
        }

        /**
         * Used to check the correctness of this spline
         */
        public bool CheckValues()
        {
            return SplineX.CheckValues() && SplineY.CheckValues() && SplineZ.CheckValues();
        }

        public double GetDx(double t)
        {
            return SplineX.GetDx(t);
        }

        public double GetDy(double t)
        {
            return SplineY.GetDx(t);
        }

        public Spline GetSplineX()
        {
            return SplineX;
        }

        public Spline GetSplineY()
        {
            return SplineY;
        }

        public double GetLength()
        {
            return _length;
        }
    }
}