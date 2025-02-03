

using System;

namespace XDay
{
    public struct FixedPoint
    {
        public readonly float RawFloat => m_ScaledValue / (float)m_ScaleFactor;
        public readonly int RawInt
        {
            get
            {
                if (m_ScaledValue >= 0)
                {
                    return (int)(m_ScaledValue >> m_ShiftBit);
                }
                return -(int)(-m_ScaledValue >> m_ShiftBit);
            }
        }
        public static readonly FixedPoint Zero = new(0);
        public static readonly FixedPoint One = new(1);
        public static readonly FixedPoint Max = new FixedPoint(int.MaxValue);
        public static readonly FixedPoint Min = new FixedPoint(int.MinValue);

        public FixedPoint(int value)
        {
            m_ScaledValue = (long)value * m_ScaleFactor;
        }

        public FixedPoint(float value)
        {
            m_ScaledValue = (long)Math.Round(value * m_ScaleFactor);
        }

        private FixedPoint(long scaledValued)
        {
            m_ScaledValue = scaledValued;
        }

        public static implicit operator FixedPoint(int v)
        {
            return new FixedPoint(v);
        }

        public static explicit operator FixedPoint(float v)
        {
            return new FixedPoint(v);
        }

        public static FixedPoint operator + (FixedPoint v1, FixedPoint v2)
        {
            return new FixedPoint(v1.m_ScaledValue + v2.m_ScaledValue);
        }

        public static FixedPoint operator -(FixedPoint v1, FixedPoint v2)
        {
            return new FixedPoint(v1.m_ScaledValue - v2.m_ScaledValue);
        }

        public static FixedPoint operator -(FixedPoint v1)
        {
            return new FixedPoint(-v1.m_ScaledValue);
        }

        public static FixedPoint operator *(FixedPoint v1, FixedPoint v2)
        {
            long val = v1.m_ScaledValue * v2.m_ScaledValue;
            if (val >= 0)
            {
                val >>= m_ShiftBit;
            }
            else
            {
                val = -(-val >> m_ShiftBit);
            }
            return new FixedPoint(val);
        }

        public static FixedPoint operator /(FixedPoint v1, FixedPoint v2)
        {
            if (v2.m_ScaledValue == 0)
            {
                throw new Exception("divide by zero!");
            }
            return new FixedPoint((v1.m_ScaledValue << m_ShiftBit) / v2.m_ScaledValue);
        }

        public static bool operator == (FixedPoint v1, FixedPoint v2)
        {
            return v1.m_ScaledValue == v2.m_ScaledValue;
        }

        public static bool operator !=(FixedPoint v1, FixedPoint v2)
        {
            return v1.m_ScaledValue != v2.m_ScaledValue;
        }

        public static bool operator > (FixedPoint v1, FixedPoint v2)
        {
            return v1.m_ScaledValue > v2.m_ScaledValue;
        }

        public static bool operator <(FixedPoint v1, FixedPoint v2)
        {
            return v1.m_ScaledValue < v2.m_ScaledValue;
        }

        public static bool operator >=(FixedPoint v1, FixedPoint v2)
        {
            return v1.m_ScaledValue >= v2.m_ScaledValue;
        }

        public static bool operator <=(FixedPoint v1, FixedPoint v2)
        {
            return v1.m_ScaledValue <= v2.m_ScaledValue;
        }

        public static FixedPoint operator >>(FixedPoint v1, int bit)
        {
            if (v1.m_ScaledValue >= 0)
            {
                return new FixedPoint(v1.m_ScaledValue >> bit);
            }
            return new FixedPoint(-((-v1.m_ScaledValue) >> bit));
        }

        public static FixedPoint operator <<(FixedPoint v1, int bit)
        {
            return new FixedPoint(v1.m_ScaledValue << bit);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not FixedPoint o)
            {
                return false;
            }

            return o.m_ScaledValue == m_ScaledValue;
        }

        public override int GetHashCode()
        {
            return m_ScaledValue.GetHashCode();
        }

        public override string ToString()
        {
            return RawFloat.ToString();
        }

        public readonly FixedPoint Abs()
        {
            return m_ScaledValue >= 0 ? new FixedPoint(m_ScaledValue) : new FixedPoint(-m_ScaledValue);
        }

        private long m_ScaledValue = 0;
        private const int m_ShiftBit = 16;
        private const int m_ScaleFactor = 1 << m_ShiftBit;
    }
}
