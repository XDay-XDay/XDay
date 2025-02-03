

using System;

namespace XDay
{
    public struct FixedAngle
    {
        public int Value;
        public uint Scale;

        public static FixedAngle HalfPi = new FixedAngle(15708, FixedAcosTable.Scale);
        public static FixedAngle PI = new FixedAngle(31416, FixedAcosTable.Scale);
        public static FixedAngle TWOPI = new FixedAngle(62832, FixedAcosTable.Scale);
        public static FixedAngle Zero = new FixedAngle(0, FixedAcosTable.Scale);

        public FixedAngle(int value, uint scale)
        {
            Value = value;
            Scale = scale;
        }

        public float ToRadian()
        {
            return Value / (float)Scale;
        }

        public int ToAngle()
        {
            float radian = ToRadian();
            return (int)Math.Round(radian * 180.0f / Math.PI);
        }

        public static bool operator >(FixedAngle a, FixedAngle b)
        {
            if (a.Scale != b.Scale)
            {
                throw new System.Exception("Scale not equal");
            }
            return a.Value > b.Value;
        }

        public static bool operator <(FixedAngle a, FixedAngle b)
        {
            if (a.Scale != b.Scale)
            {
                throw new System.Exception("Scale not equal");
            }
            return a.Value < b.Value;
        }

        public static bool operator >=(FixedAngle a, FixedAngle b)
        {
            if (a.Scale != b.Scale)
            {
                throw new System.Exception("Scale not equal");
            }
            return a.Value >= b.Value;
        }

        public static bool operator <=(FixedAngle a, FixedAngle b)
        {
            if (a.Scale != b.Scale)
            {
                throw new System.Exception("Scale not equal");
            }
            return a.Value <= b.Value;
        }

        public static bool operator ==(FixedAngle a, FixedAngle b)
        {
            if (a.Scale != b.Scale)
            {
                throw new System.Exception("Scale not equal");
            }
            return a.Value == b.Value;
        }

        public static bool operator !=(FixedAngle a, FixedAngle b)
        {
            if (a.Scale != b.Scale)
            {
                throw new System.Exception("Scale not equal");
            }
            return a.Value != b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj != null || obj is not FixedAngle a) 
            {
                return false;
            }

            return a.Value == Value && a.Scale == Scale;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Value: {Value}, Scale: {Scale}";
        }
    }
}
