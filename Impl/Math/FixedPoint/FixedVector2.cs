﻿using System;
using System.Diagnostics;

namespace XDay
{
    public struct FixedVector2
    {
        public FixedPoint SqrMagnitude => X * X + Y * Y;
        public FixedPoint Magnitude => FixedMath.Sqrt(SqrMagnitude);
        public FixedVector2 Normalized
        {
            get
            {
                var len = Magnitude;
                if (len == 0)
                {
                    return Zero;
                }

                len = 1 / len;
                return new FixedVector2(X * len, Y * len);
            }
        }
        public FixedPoint X;
        public FixedPoint Y;
        public static readonly FixedVector2 Zero = new FixedVector2(0, 0);
        public static readonly FixedVector2 One = new FixedVector2(1, 1);
        public static readonly FixedVector2 Up = new FixedVector2(0, 1);
        public static readonly FixedVector2 Down = new FixedVector2(0, -1);
        public static readonly FixedVector2 Right = new FixedVector2(1, 0);
        public static readonly FixedVector2 Left = new FixedVector2(-1, 0);

        public FixedVector2(float x, float y)
        {
            X = new FixedPoint(x);
            Y = new FixedPoint(y);
        }

        public FixedVector2(int x, int y)
        {
            X = new FixedPoint(x);
            Y = new FixedPoint(y);
        }

        public FixedVector2(FixedPoint x, FixedPoint y)
        {
            X = x;
            Y = y;
        }

        public void Normalize()
        {
            var len = Magnitude;
            if (len > 0)
            {
                len = 1 / len;
                X *= len;
                Y *= len;
            }
        }

        public FixedPoint this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b)
        {
            return new FixedVector2(a.X + b.X, a.Y + b.Y);
        }

        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b)
        {
            return new FixedVector2(a.X - b.X, a.Y - b.Y);
        }

        public static FixedVector2 operator /(FixedVector2 a, FixedPoint v)
        {
            if (v == FixedPoint.Zero)
            {
                throw new System.Exception("Divided by zero");
            }
            return new FixedVector2(a.X / v, a.Y / v);
        }

        public static FixedVector2 operator *(FixedVector2 a, FixedPoint v)
        {
            return new FixedVector2(a.X * v, a.Y * v);
        }

        public static FixedVector2 operator *(FixedPoint v, FixedVector2 a)
        {
            return new FixedVector2(a.X * v, a.Y * v);
        }

        public static FixedVector2 operator -(FixedVector2 a)
        {
            return new FixedVector2(-a.X, -a.Y);
        }

        public static bool operator ==(FixedVector2 a, FixedVector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(FixedVector2 a, FixedVector2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not FixedVector2 v)
            {
                return false;
            }

            return v == this;
        }

        public static FixedPoint GetSqrMagnitude(FixedVector2 v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static FixedVector2 GetNormalized(FixedVector2 v)
        {
            FixedVector2 result = v;
            result.Normalize();
            return result;
        }

        public static FixedPoint Dot(FixedVector2 v1, FixedVector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static FixedAngle Angle(FixedVector2 v1, FixedVector2 v2)
        {
            var k = v1.Magnitude * v2.Magnitude;
            if (k == 0)
            {
                return FixedAngle.Zero;
            }
            var d = Dot(v1, v2);

            return FixedMath.ACos(d / k);
        }

        public static FixedPoint DistanceSqr(FixedVector2 a, FixedVector2 b)
        {
            return (a - b).SqrMagnitude;
        }

        public static FixedPoint Distance(FixedVector2 a, FixedVector2 b)
        {
            return (a - b).Magnitude;
        }

        public static FixedPoint Cross(FixedVector2 a, FixedVector2 b)
        {
            // cz = ax * by − ay * bx
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
