

namespace XDay
{
    public class PhysicalMaterial
    {
        public FixedPoint Density
        {
            set
            {
                if (value < PhysicsWorld.MinDensity)
                {
                    Log.Instance?.Error($"Density is too small. Min density is {PhysicsWorld.MinDensity}");
                    return;
                }

                if (value > PhysicsWorld.MaxDensity)
                {
                    Log.Instance?.Error($"Density is too large. Max density is {PhysicsWorld.MaxDensity}");
                    return;
                }

                m_Density = value;
            }
            get => m_Density;
        }

        public FixedPoint Restitution
        {
            set => m_Restitution = FixedMath.Clamp(value, FixedPoint.Zero, FixedPoint.One);
            get => m_Restitution;
        }

        public FixedPoint StaticFriction
        {
            set => m_StaticFriction = FixedMath.Clamp(value, FixedPoint.Zero, FixedPoint.One);
            get => m_StaticFriction;
        }

        public FixedPoint DynamicFriction
        {
            set => m_DynamicFriction = FixedMath.Clamp(value, FixedPoint.Zero, FixedPoint.One);
            get => m_DynamicFriction;
        }

        private FixedPoint m_Density = 1;
        private FixedPoint m_Restitution = new(0.5f);
        private FixedPoint m_StaticFriction = new(0.6f);
        private FixedPoint m_DynamicFriction = new(0.4f);
    }
}
