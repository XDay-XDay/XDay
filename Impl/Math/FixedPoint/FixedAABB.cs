
namespace XDay
{
    public readonly struct FixedAABB
    {
        public readonly FixedVector2 Min;
        public readonly FixedVector2 Max;

        public FixedAABB(FixedVector2 min, FixedVector2 max)
        {
            Min = min;
            Max = max;
        }

        public FixedAABB(FixedPoint minX, FixedPoint minY, FixedPoint maxX, FixedPoint maxY)
        {
            Min = new FixedVector2(minX, minY);
            Max = new FixedVector2(maxX, maxY);
        }
    }
}