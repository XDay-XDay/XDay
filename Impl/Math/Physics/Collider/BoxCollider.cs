

namespace XDay
{
    public class BoxCollider : Collider
    {
        public readonly FixedPoint Width;
        public readonly FixedPoint Height;

        public BoxCollider(FixedPoint width, FixedPoint height)
            : base(width * height)
        {
            Width = width;
            Height = height;
            m_Vertices = CreateBoxVertices(width, height);
            m_TransformedVertices = new FixedVector2[m_Vertices.Length];
        }

        public FixedVector2[] GetTransformedVertices(Rigidbody body)
        {
            if (body.IsTransformDirty)
            {
                FixedTransform transform = new FixedTransform(body.Position, body.Angle);

                for (int i = 0; i < m_Vertices.Length; i++)
                {
                    FixedVector2 v = m_Vertices[i];
                    m_TransformedVertices[i] = Transform(v, transform);
                }
                body.IsTransformDirty = false;
            }

            return m_TransformedVertices;
        }

        private FixedVector2 Transform(FixedVector2 v, FixedTransform transform)
        {
            return new FixedVector2(
                transform.Cos * v.X - transform.Sin * v.Y + transform.PositionX,
                transform.Sin * v.X + transform.Cos * v.Y + transform.PositionY);
        }

        private FixedVector2[] CreateBoxVertices(FixedPoint width, FixedPoint height)
        {
            FixedPoint left = -width / new FixedPoint(2);
            FixedPoint right = left + width;
            FixedPoint bottom = -height / new FixedPoint(2);
            FixedPoint top = bottom + height;

            FixedVector2[] vertices = new FixedVector2[4];
            vertices[0] = new FixedVector2(left, top);
            vertices[1] = new FixedVector2(right, top);
            vertices[2] = new FixedVector2(right, bottom);
            vertices[3] = new FixedVector2(left, bottom);

            return vertices;
        }

        private readonly FixedVector2[] m_Vertices;
        private readonly FixedVector2[] m_TransformedVertices;
    }
}
