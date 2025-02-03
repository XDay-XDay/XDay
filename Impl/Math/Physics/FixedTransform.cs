
namespace XDay
{
    internal readonly struct FixedTransform
    {
        public readonly static FixedTransform Zero = new FixedTransform(0, 0, 0);
        public readonly FixedPoint PositionX;
        public readonly FixedPoint PositionY;
        public readonly FixedPoint Sin;
        public readonly FixedPoint Cos;

        public FixedTransform(FixedVector2 position, FixedPoint angleInRadian)
        {
            PositionX = position.X;
            PositionY = position.Y;
            Sin = FixedMath.Sin(angleInRadian);
            Cos = FixedMath.Cos(angleInRadian);
        }

        public FixedTransform(FixedPoint x, FixedPoint y, FixedPoint angleInRadian)
        {
            PositionX = x;
            PositionY = y;
            Sin = FixedMath.Sin(angleInRadian);
            Cos = FixedMath.Cos(angleInRadian);
        }
    }
}
