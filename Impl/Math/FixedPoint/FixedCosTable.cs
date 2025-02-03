

using System;

namespace XDay
{
    internal static class FixedCosTable
    {
        static FixedCosTable()
        {
            int count = 10000;
            Value = new FixedPoint[count];
            for (var i = 0; i < count; i++)
            {
                float angle = (float)i / (count - 1) * 360.0f;
                Value[i] = new FixedPoint(MathF.Cos(angle / 180.0f * MathF.PI));
            }
        }

        //t between [0, 1]
        public static FixedPoint Sample(FixedPoint t)
        {
            var index = t * (Value.Length - 1);
            return Value[index.RawInt];
        }

        public static FixedPoint[] Value;
    }
}