

namespace XDay
{
    public class SphereCollider : Collider
    {
        public readonly FixedPoint Radius;

        public SphereCollider(FixedPoint radius)
            : base(radius * radius * FixedMath.Pi)
        {
            Radius = radius;
        }
    }
}
