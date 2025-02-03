

namespace XDay
{
    public readonly struct FixedManifold
    {
        public readonly Rigidbody BodyA;
        public readonly Rigidbody BodyB;
        public readonly FixedVector2 Normal;
        public readonly FixedPoint Depth;
        public readonly FixedVector2 Contact1;
        public readonly FixedVector2 Contact2;
        public readonly int ContactCount;

        public FixedManifold(
            Rigidbody bodyA, Rigidbody bodyB, 
            FixedVector2 normal, FixedPoint depth, 
            FixedVector2 contact1, FixedVector2 contact2, int contactCount)
        {
            BodyA = bodyA;
            BodyB = bodyB;
            Normal = normal;
            Depth = depth;
            Contact1 = contact1;
            Contact2 = contact2;
            ContactCount = contactCount;
        }
    }
}
