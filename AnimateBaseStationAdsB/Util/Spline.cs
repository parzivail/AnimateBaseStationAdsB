using System;

namespace AnimateBaseStationAdsB.Util
{
    public class Spline
    {
        public double[] Xx;
        public double[] Yy;

        private double[] _a;
        private double[] _b;
        private double[] _c;
        private double[] _d;

        /**
         * tracks the last index found since that is mostly commonly the next one used
         */
        private int _storageIndex;

        /**
         * Creates a new Spline.
         *
         * @param xx
         * @param yy
         */
        public Spline(double[] xx, double[] yy)
        {
            SetValues(xx, yy);
        }

        /**
         * Set values for this Spline.
         *
         * @param xx
         * @param yy
         */
        public void SetValues(double[] xx, double[] yy)
        {
            Xx = xx;
            Yy = yy;
            if (xx.Length > 1)
            {
                CalculateCoefficients();
            }
        }

        /**
         * Returns an interpolated value.
         *
         * @param x
         * @return the interpolated value
         */
        public double GetValue(double x)
        {
            if (Xx.Length == 0)
                return double.NaN;
            if (Xx.Length == 1)
                return Xx[0] == x ? Yy[0] : double.NaN;

            var index = Array.BinarySearch(Xx, x);
            if (index > 0)
            {
                return Yy[index];
            }

            index = -(index + 1) - 1;
            //TODO linear interpolation or extrapolation
            if (index < 0)
            {
                return Yy[0];
            }

            return _a[index] + _b[index] * (x - Xx[index]) + _c[index] * Math.Pow(x - Xx[index], 2) + _d[index] * Math.Pow(x - Xx[index], 3);
        }

        /**
         * Returns an interpolated value. To be used when a long sequence of values
         * are required in order, but ensure checkValues() is called beforehand to
         * ensure the boundary checks from getValue() are made
         *
         * @param x
         * @return the interpolated value
         */
        public double GetFastValue(double x)
        {
            // Fast check to see if previous index is still valid
            if (_storageIndex > -1 && _storageIndex < Xx.Length - 1 && x > Xx[_storageIndex] && x < Xx[_storageIndex + 1])
            {

            }
            else
            {
                var index = Array.BinarySearch(Xx, x);
                if (index > 0)
                {
                    return Yy[index];
                }
                index = -(index + 1) - 1;
                _storageIndex = index;
            }

            //TODO linear interpolation or extrapolation
            if (_storageIndex < 0)
            {
                return Yy[0];
            }
            var value = x - Xx[_storageIndex];
            return _a[_storageIndex] + _b[_storageIndex] * value + _c[_storageIndex] * (value * value) + _d[_storageIndex] * (value * value * value);
        }

        /**
         * Used to check the correctness of this spline
         */
        public bool CheckValues()
        {
            if (Xx.Length < 2)
            {
                return false;
            }
            return true;
        }

        /**
         * Returns the first derivation at x.
         *
         * @param x
         * @return the first derivation at x
         */
        public double GetDx(double x)
        {
            if (Xx.Length == 0 || Xx.Length == 1)
            {
                return 0;
            }

            var index = Array.BinarySearch(Xx, x);
            if (index < 0)
            {
                index = -(index + 1) - 1;
            }

            return _b[index] + 2 * _c[index] * (x - Xx[index]) + 3 * _d[index] * Math.Pow(x - Xx[index], 2);
        }

        /**
         * Calculates the Spline coefficients.
         */
        private void CalculateCoefficients()
        {
            var n = Yy.Length;
            _a = new double[n];
            _b = new double[n];
            _c = new double[n];
            _d = new double[n];

            if (n == 2)
            {
                _a[0] = Yy[0];
                _b[0] = Yy[1] - Yy[0];
                return;
            }

            var h = new double[n - 1];
            for (var i = 0; i < n - 1; i++)
            {
                _a[i] = Yy[i];
                h[i] = Xx[i + 1] - Xx[i];
                // h[i] is used for division later, avoid a NaN
                if (h[i] == 0.0)
                {
                    h[i] = 0.01;
                }
            }
            _a[n - 1] = Yy[n - 1];

            var a = new double[n - 2, n - 2];
            var y = new double[n - 2];
            for (var i = 0; i < n - 2; i++)
            {
                y[i] = 3 * ((Yy[i + 2] - Yy[i + 1]) / h[i + 1] - (Yy[i + 1] - Yy[i]) / h[i]);

                a[i, i] = 2 * (h[i] + h[i + 1]);

                if (i > 0)
                {
                    a[i, i - 1] = h[i];
                }

                if (i < n - 3)
                {
                    a[i, i + 1] = h[i + 1];
                }
            }
            Solve(a, y);

            for (var i = 0; i < n - 2; i++)
            {
                _c[i + 1] = y[i];
                _b[i] = (_a[i + 1] - _a[i]) / h[i] - (2 * _c[i] + _c[i + 1]) / 3 * h[i];
                _d[i] = (_c[i + 1] - _c[i]) / (3 * h[i]);
            }
            _b[n - 2] = (_a[n - 1] - _a[n - 2]) / h[n - 2] - (2 * _c[n - 2] + _c[n - 1]) / 3 * h[n - 2];
            _d[n - 2] = (_c[n - 1] - _c[n - 2]) / (3 * h[n - 2]);
        }

        /**
         * Solves Ax=b and stores the solution in b.
         */
        public void Solve(double[,] a, double[] b)
        {
            var n = b.Length;
            for (var i = 1; i < n; i++)
            {
                a[i, i - 1] = a[i, i - 1] / a[i - 1, i - 1];
                a[i, i] = a[i, i] - a[i - 1, i] * a[i, i - 1];
                b[i] = b[i] - a[i, i - 1] * b[i - 1];
            }

            b[n - 1] = b[n - 1] / a[n - 1, n - 1];
            for (var i = b.Length - 2; i >= 0; i--)
            {
                b[i] = (b[i] - a[i, i + 1] * b[i + 1]) / a[i, i];
            }
        }
    }
}