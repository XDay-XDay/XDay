using System;

namespace XDay
{
    public sealed class Rigidbody
    {
        public readonly bool IsStatic;
        public readonly FixedPoint InvMass;
        public readonly FixedPoint InvInertia;
        public bool EnableRotation { get => m_EnableRotation; set => m_EnableRotation = value; }
        public FixedVector2 Position => m_Position;
        public FixedVector2 LinearVelocity
        {
            get => m_LinearVelocity;
            set => m_LinearVelocity = value;
        }
        public FixedPoint Angle => m_AngleInRadian;
        public FixedPoint AngularVelocity
        {
            get => m_AngularVelocity;
            set => m_AngularVelocity = value;
        }
        public PhysicalMaterial Material => m_Material;
        public Collider Collider => m_Collider;
        public bool IsTransformDirty { get => m_TransformDirty; set => m_TransformDirty = value; }
        public bool EnableGravity { get => m_EnableGravity; set=>m_EnableGravity = value; }
        public bool IsKinematic { get => m_Kinematic; set=> m_Kinematic = value; }

        private Rigidbody(PhysicalMaterial material, FixedPoint mass, FixedPoint inertia, bool isStatic, Collider collider)
        {
            m_Material = material;
            m_Collider = collider;
            InvMass = mass > 0 ? FixedPoint.One / mass : 0;
            InvInertia = inertia > 0 ? FixedPoint.One / inertia : 0;
            IsStatic = isStatic;
            m_TransformDirty = true;
            m_AABBUpdateRequired = true;
        }

        public FixedAABB GetAABB()
        {
            if (m_AABBUpdateRequired)
            {
                FixedPoint minX = FixedPoint.Max;
                FixedPoint minY = FixedPoint.Max;
                FixedPoint maxX = FixedPoint.Min;
                FixedPoint maxY = FixedPoint.Min;

                if (m_Collider is BoxCollider boxCollider)
                {
                    FixedVector2[] vertices = boxCollider.GetTransformedVertices(this);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        FixedVector2 v = vertices[i];

                        if (v.X < minX) { minX = v.X; }
                        if (v.X > maxX) { maxX = v.X; }
                        if (v.Y < minY) { minY = v.Y; }
                        if (v.Y > maxY) { maxY = v.Y; }
                    }
                }
                else if (m_Collider is SphereCollider sphereCollider)
                {
                    minX = m_Position.X - sphereCollider.Radius;
                    minY = m_Position.Y - sphereCollider.Radius;
                    maxX = m_Position.X + sphereCollider.Radius;
                    maxY = m_Position.Y + sphereCollider.Radius;
                }
                else
                {
                    throw new Exception("Unknown collider type.");
                }

                m_AABB = new FixedAABB(minX, minY, maxX, maxY);
            }

            m_AABBUpdateRequired = false;
            return m_AABB;
        }

        internal void Step(FixedPoint time, int iterations)
        {
            if(IsStatic)
            {
                return;
            }

            time /= new FixedPoint(iterations);

            if (!m_Kinematic)
            {
                FixedVector2 acceleration = m_Force * InvMass;
                m_LinearVelocity += acceleration * time;
                m_AngleInRadian += m_AngularVelocity * time;
            }

            m_Position += m_LinearVelocity * time;
            m_Force = FixedVector2.Zero;
            m_TransformDirty = true;
            m_AABBUpdateRequired = true;
        }

        public void Move(FixedVector2 amount)
        {
            m_Position += amount;
            m_TransformDirty = true;
            m_AABBUpdateRequired = true;
        }

        public void MoveTo(FixedVector2 position)
        {
            m_Position = position;
            m_TransformDirty = true;
            m_AABBUpdateRequired = true;
        }

        public void Rotate(FixedPoint amount)
        {
            m_AngleInRadian += amount;
            m_TransformDirty = true;
            m_AABBUpdateRequired = true;
        }

        public void RotateTo(FixedPoint angle)
        {
            m_AngleInRadian = angle;
            m_TransformDirty = true;
            m_AABBUpdateRequired = true;
        }

        public void AddForce(FixedVector2 amount)
        {
            if (IsStatic)
            {
                return;
            }
            m_Force += amount;
            if (m_Kinematic)
            {
                Log.Instance?.Warning($"Body is kinematic, force is ignored!");
            }
        }

        public void AddImpulse(FixedVector2 amount)
        {
            if (IsStatic)
            {
                return;
            }

            m_Force += amount / InvMass;
            if (m_Kinematic)
            {
                Log.Instance?.Warning($"Body is kinematic, impulse is ignored!");
            }
        }

        public static Rigidbody CreateCircleBody(FixedPoint radius, bool isStatic, PhysicalMaterial material)
        {
            FixedPoint mass = 0;
            FixedPoint inertia = 0;

            var collider = new SphereCollider(radius);

            if (!isStatic)
            {
                mass = collider.Area * material.Density;
                inertia = new FixedPoint(0.5f) * mass * radius * radius;
            }

            var body = new Rigidbody(material, mass, inertia, isStatic, collider);
            return body;
        }

        public static Rigidbody CreateBoxBody(FixedPoint width, FixedPoint height, bool isStatic, PhysicalMaterial material)
        {
            var collider = new BoxCollider(width, height);

            FixedPoint mass = 0;
            FixedPoint inertia = 0;

            if (!isStatic)
            {
                mass = collider.Area * material.Density;
                inertia = new FixedPoint(1) / new FixedPoint(12) * mass * (width * width + height * height);
            }

            return new Rigidbody(material, mass, inertia, isStatic, collider);
        }

        private FixedAABB m_AABB;
        private FixedVector2 m_Position;
        private FixedVector2 m_LinearVelocity;
        private FixedPoint m_AngleInRadian;
        private FixedPoint m_AngularVelocity;
        private FixedVector2 m_Force;
        private readonly PhysicalMaterial m_Material;
        private readonly Collider m_Collider;
        private bool m_EnableRotation = true;
        private bool m_TransformDirty;
        private bool m_AABBUpdateRequired;
        private bool m_EnableGravity = true;
        private bool m_Kinematic = false;
    }
}
