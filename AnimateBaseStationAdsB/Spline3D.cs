using System;
using System.Collections.Generic;
using OpenTK;

namespace AnimateBaseStationAdsB
{
    public class Spline3D
    {
        private Spline _splineX;
        private Spline _splineY;
        private Spline _splineZ;
        /**
         * Total length tracing the points on the spline
         */
        private double _length;

        public int Points { get; set; }
        public IReadOnlyList<Vector3> OriginalPoints { get; set; }

        /**
         * Creates a new Spline3D.
         *
         * @param points
         */
        public Spline3D(IReadOnlyList<Vector3> points)
        {
            OriginalPoints = points;

            var x = new double[points.Count];
            var y = new double[points.Count];
            var z = new double[points.Count];

            Points = points.Count;

            for (var i = 0; i < points.Count; i++)
            {
                x[i] = points[i].X;
                y[i] = points[i].Y;
                z[i] = points[i].Z;
            }

            Init(x, y, z);
        }

        /**
         * Creates a new Spline2D.
         *
         * @param x
         * @param y
         */
        public Spline3D(double[] x, double[] y, double[] z)
        {
            Init(x, y, z);
        }

        private void Init(double[] x, double[] y, double[] z)
        {
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

            for (var i = 1; i < t.Length - 1; i++)
            {
                t[i] = t[i] / _length;
            }

            t[t.Length - 1] = 1.0; // end point is always 1.0

            _splineX = new Spline(t, x);
            _splineY = new Spline(t, y);
            _splineZ = new Spline(t, z);
        }

        /**
         * @param t 0 <= t <= 1
         */
        public Vector3 GetPoint(double t)
        {
            return new Vector3((float)_splineX.GetValue(t), (float)_splineY.GetValue(t), (float)_splineZ.GetValue(t));
        }

        /**
         * Used to check the correctness of this spline
         */
        public bool CheckValues()
        {
            return _splineX.CheckValues() && _splineY.CheckValues() && _splineZ.CheckValues();
        }

        public double GetDx(double t)
        {
            return _splineX.GetDx(t);
        }

        public double GetDy(double t)
        {
            return _splineY.GetDx(t);
        }

        public double GetDz(double t)
        {
            return _splineZ.GetDx(t);
        }

        public Spline GetSplineX()
        {
            return _splineX;
        }

        public Spline GetSplineY()
        {
            return _splineY;
        }

        public Spline GetSplineZ()
        {
            return _splineZ;
        }

        public double GetLength()
        {
            return _length;
        }

    }
}
