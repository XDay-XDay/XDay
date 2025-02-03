
namespace XDay
{
    public static class FixedCollision
    {
        public static void PointSegmentDistance(FixedVector2 p, FixedVector2 a, FixedVector2 b, 
            out FixedPoint distanceSquared, out FixedVector2 cp)
        {
            FixedVector2 ab = b - a;
            FixedVector2 ap = p - a;

            FixedPoint proj = FixedVector2.Dot(ap, ab);
            FixedPoint abLenSq = ab.SqrMagnitude;
            FixedPoint d = proj / abLenSq;

            if(d <= FixedPoint.Zero)
            {
                cp = a;
            }
            else if(d >= FixedPoint.One)
            {
                cp = b;
            }
            else
            {
                cp = a + ab * d;
            }

            distanceSquared = FixedVector2.DistanceSqr(p, cp);
        }

        public static bool IntersectAABBs(FixedAABB a, FixedAABB b)
        {
            if(a.Max.X <= b.Min.X || b.Max.X <= a.Min.X ||
                a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y)
            {
                return false;
            }

            return true;
        }

        public static void FindContactPoints(
            Rigidbody bodyA, Rigidbody bodyB, 
            out FixedVector2 contact1, out FixedVector2 contact2, 
            out int contactCount)
        {
            contact1 = FixedVector2.Zero;
            contact2 = FixedVector2.Zero;
            contactCount = 0;

            if (bodyA.Collider is BoxCollider boxColliderA)
            {
                if (bodyB.Collider is BoxCollider boxColliderB)
                {
                    FindPolygonsContactPoints(boxColliderA.GetTransformedVertices(bodyA), boxColliderB.GetTransformedVertices(bodyB),
                        out contact1, out contact2, out contactCount);
                }
                else if (bodyB.Collider is SphereCollider sphereColliderB)
                {
                    FindCirclePolygonContactPoint(bodyB.Position, sphereColliderB.Radius, bodyA.Position, boxColliderA.GetTransformedVertices(bodyA), out contact1);
                    contactCount = 1;
                }
            }
            else if (bodyA.Collider is SphereCollider sphereColliderA)
            {
                if (bodyB.Collider is BoxCollider boxColliderB)
                {
                    FindCirclePolygonContactPoint(bodyA.Position, sphereColliderA.Radius, bodyB.Position, boxColliderB.GetTransformedVertices(bodyB), out contact1);
                    contactCount = 1;
                }
                else if (bodyB.Collider is SphereCollider sphereColliderB)
                {
                    FindCirclesContactPoint(bodyA.Position, sphereColliderA.Radius, bodyB.Position, out contact1);
                    contactCount = 1;
                }
            }
        }

        private static void FindPolygonsContactPoints(
            FixedVector2[] verticesA, FixedVector2[] verticesB, 
            out FixedVector2 contact1, out FixedVector2 contact2, out int contactCount)
        {
            contact1 = FixedVector2.Zero;
            contact2 = FixedVector2.Zero;
            contactCount = 0;

            FixedPoint minDistSq = FixedPoint.Max;

            for(int i = 0; i < verticesA.Length; i++)
            {
                FixedVector2 p = verticesA[i];

                for(int j = 0; j < verticesB.Length; j++)
                {
                    FixedVector2 va = verticesB[j];
                    FixedVector2 vb = verticesB[(j + 1) % verticesB.Length];

                    PointSegmentDistance(p, va, vb, out var distSq, out FixedVector2 cp);

                    if(distSq == minDistSq)
                    {
                        if (cp != contact1)
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if(distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FixedVector2 p = verticesB[i];

                for (int j = 0; j < verticesA.Length; j++)
                {
                    FixedVector2 va = verticesA[j];
                    FixedVector2 vb = verticesA[(j + 1) % verticesA.Length];

                    PointSegmentDistance(p, va, vb, out var distSq, out FixedVector2 cp);

                    if (distSq == minDistSq)
                    {
                        if (cp != contact1)
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }
        }

        private static void FindCirclePolygonContactPoint(
            FixedVector2 circleCenter, FixedPoint circleRadius, 
            FixedVector2 polygonCenter, FixedVector2[] polygonVertices, 
            out FixedVector2 cp)
        {
            cp = FixedVector2.Zero;

            FixedPoint minDistSq = FixedPoint.Max;

            for(int i = 0; i < polygonVertices.Length; i++)
            {
                FixedVector2 va = polygonVertices[i];
                FixedVector2 vb = polygonVertices[(i + 1) % polygonVertices.Length];

                PointSegmentDistance(circleCenter, va, vb, out var distSq, out FixedVector2 contact);

                if(distSq < minDistSq)
                {
                    minDistSq = distSq;
                    cp = contact;
                }
            }
        }

        private static void FindCirclesContactPoint(FixedVector2 centerA, FixedPoint radiusA, FixedVector2 centerB, out FixedVector2 cp)
        {
            FixedVector2 ab = centerB - centerA;
            ab.Normalize();
            cp = centerA + ab * radiusA;
        }

        public static bool Collide(Rigidbody bodyA, Rigidbody bodyB, out FixedVector2 normal, out FixedPoint depth)
        {
            normal = FixedVector2.Zero;
            depth = 0;

            if (bodyA.Collider is BoxCollider boxColliderA)
            {
                if (bodyB.Collider is BoxCollider boxColliderB)
                {
                    return IntersectPolygons(
                        bodyA.Position, boxColliderA.GetTransformedVertices(bodyA),
                        bodyB.Position, boxColliderB.GetTransformedVertices(bodyB),
                        out normal, out depth);
                }
                else if (bodyB.Collider is SphereCollider sphereColliderB)
                {
                    bool result = IntersectCirclePolygon(
                        bodyB.Position, sphereColliderB.Radius,
                        bodyA.Position, boxColliderA.GetTransformedVertices(bodyA),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if (bodyA.Collider is SphereCollider sphereColliderA)
            {
                if (bodyB.Collider is BoxCollider boxColliderB)
                {
                    return IntersectCirclePolygon(
                        bodyA.Position, sphereColliderA.Radius,
                        bodyB.Position, boxColliderB.GetTransformedVertices(bodyB),
                        out normal, out depth);
                }
                else if (bodyB.Collider is SphereCollider sphereColliderB)
                {
                    return IntersectCircles(
                        bodyA.Position, sphereColliderA.Radius,
                        bodyB.Position, sphereColliderB.Radius,
                        out normal, out depth);
                }
            }

            return false;
        }

        public static bool IntersectCirclePolygon(FixedVector2 circleCenter, FixedPoint circleRadius,
                                                    FixedVector2 polygonCenter, FixedVector2[] vertices,
                                                    out FixedVector2 normal, out FixedPoint depth)
        {
            normal = FixedVector2.Zero;
            depth = FixedPoint.Max;

            FixedVector2 axis = FixedVector2.Zero;
            FixedPoint axisDepth = 0;
            FixedPoint minA, maxA, minB, maxB;

            for (int i = 0; i < vertices.Length; i++)
            {
                FixedVector2 va = vertices[i];
                FixedVector2 vb = vertices[(i + 1) % vertices.Length];

                FixedVector2 edge = vb - va;
                axis = new FixedVector2(-edge.Y, edge.X);
                axis.Normalize();

                ProjectVertices(vertices, axis, out minA, out maxA);
                ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = FixedMath.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            int cpIndex = FindClosestPointOnPolygon(circleCenter, vertices);
            FixedVector2 cp = vertices[cpIndex];

            axis = cp - circleCenter;
            axis.Normalize();

            ProjectVertices(vertices, axis, out minA, out maxA);
            ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }

            axisDepth = FixedMath.Min(maxB - minA, maxA - minB);

            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            FixedVector2 direction = polygonCenter - circleCenter;

            if (FixedVector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        private static int FindClosestPointOnPolygon(FixedVector2 circleCenter, FixedVector2[] vertices)
        {
            int result = -1;
            FixedPoint minDistance = FixedPoint.Max;

            for(int i = 0; i < vertices.Length; i++)
            {
                FixedVector2 v = vertices[i];
                var distance = FixedVector2.Distance(v, circleCenter);

                if(distance < minDistance)
                {
                    minDistance = distance;
                    result = i;
                }
            }

            return result;
        }

        private static void ProjectCircle(FixedVector2 center, FixedPoint radius, FixedVector2 axis, out FixedPoint min, out FixedPoint max)
        {
            FixedVector2 direction = axis;
            direction.Normalize();
            FixedVector2 directionAndRadius = direction * radius;

            FixedVector2 p1 = center + directionAndRadius;
            FixedVector2 p2 = center - directionAndRadius;

            min = FixedVector2.Dot(p1, axis);
            max = FixedVector2.Dot(p2, axis);

            if(min > max)
            {
                // swap the min and max values.
                (min, max) = (max, min);
            }
        }

        public static bool IntersectPolygons(FixedVector2 centerA, FixedVector2[] verticesA, FixedVector2 centerB, FixedVector2[] verticesB, out FixedVector2 normal, out FixedPoint depth)
        {
            normal = FixedVector2.Zero;
            depth = FixedPoint.Max;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FixedVector2 va = verticesA[i];
                FixedVector2 vb = verticesA[(i + 1) % verticesA.Length];

                FixedVector2 edge = vb - va;
                FixedVector2 axis = new FixedVector2(-edge.Y, edge.X);
                axis.Normalize();

                ProjectVertices(verticesA, axis, out var minA, out var maxA);
                ProjectVertices(verticesB, axis, out var minB, out var maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                FixedPoint axisDepth = FixedMath.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FixedVector2 va = verticesB[i];
                FixedVector2 vb = verticesB[(i + 1) % verticesB.Length];

                FixedVector2 edge = vb - va;
                FixedVector2 axis = new FixedVector2(-edge.Y, edge.X);
                axis.Normalize();

                ProjectVertices(verticesA, axis, out var minA, out var maxA);
                ProjectVertices(verticesB, axis, out var minB, out var maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                FixedPoint axisDepth = FixedMath.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            FixedVector2 direction = centerB - centerA;

            if (FixedVector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        private static void ProjectVertices(FixedVector2[] vertices, FixedVector2 axis, out FixedPoint min, out FixedPoint max)
        {
            min = FixedPoint.Max;
            max = FixedPoint.Min;

            for(int i = 0; i < vertices.Length; i++)
            {
                FixedVector2 v = vertices[i];
                FixedPoint proj = FixedVector2.Dot(v, axis);

                if(proj < min) { min = proj; }
                if(proj > max) { max = proj; }
            }
        }

        public static bool IntersectCircles(
            FixedVector2 centerA, FixedPoint radiusA, 
            FixedVector2 centerB, FixedPoint radiusB, 
            out FixedVector2 normal, out FixedPoint depth)
        {
            normal = FixedVector2.Zero;
            depth = 0;

            FixedPoint distance = FixedVector2.Distance(centerA, centerB);
            FixedPoint radii = radiusA + radiusB;

            if(distance >= radii)
            {
                return false;
            }

            normal = centerB - centerA;
            normal.Normalize();
            depth = radii - distance;

            return true;
        }
    }
}
