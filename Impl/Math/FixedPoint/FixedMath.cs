using System;

namespace XDay
{
    public static class FixedMath
    {
        public static FixedPoint Pi = new FixedPoint(MathF.PI);
        public static FixedPoint TwoPi = new FixedPoint(MathF.PI) * 2;

        //v is between [-1, 1]
        public static FixedAngle ACos(FixedPoint v)
        {
            var index = v * FixedAcosTable.HalfCount + FixedAcosTable.HalfCount;
            index = Clamp(index, 0, FixedAcosTable.Count - 1);
            var value = FixedAcosTable.Value[index.RawInt];
            return new FixedAngle(value, FixedAcosTable.Scale);
        }

        //radian between [0, 2pi]
        public static FixedPoint Cos(FixedPoint radian)
        {
            while (radian < 0)
            {
                radian += TwoPi;
            }

            while (radian > TwoPi)
            {
                radian -= TwoPi;
            }
            return FixedCosTable.Sample(radian / TwoPi);
        }

        //radian between [0, 2pi]
        public static FixedPoint Sin(FixedPoint radian)
        {
            while (radian < 0)
            {
                radian += TwoPi;
            }

            while (radian > TwoPi)
            {
                radian -= TwoPi;
            }
            return FixedSinTable.Sample(radian / TwoPi);
        }

        public static FixedPoint Sqrt(FixedPoint v, int iterateCount = 10)
        {
            if (v < FixedPoint.Zero)
            {
                throw new Exception("Invalid value");
            }

            if (v == FixedPoint.Zero)
            {
                return 0;
            }

            int n = 0;
            FixedPoint result = v;
            FixedPoint prevValue;
            do
            {
                prevValue = result;
                result = (result + v / result) >> 1;
                ++n;
            } while (n < iterateCount && result != prevValue);

            return result;
        }

        public static FixedPoint Clamp(FixedPoint v, FixedPoint min, FixedPoint max)
        {
            if (v < min)
            {
                return min;
            }
            if (v > max)
            {
                return max;
            }
            return v;
        }

        public static FixedPoint Min(FixedPoint a, FixedPoint b)
        {
            return a < b ? a : b;
        }

        public static FixedPoint Max(FixedPoint a, FixedPoint b)
        {
            return a > b ? a : b;
        }

        public static FixedPoint Abs(FixedPoint v)
        {
            return v.Abs();
        }
    }
}
