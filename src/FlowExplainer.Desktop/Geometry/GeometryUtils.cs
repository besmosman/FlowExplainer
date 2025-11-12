using System.Numerics;
using FlowExplainer;

namespace FlowExplainer
{
    public static class GeometryUtils
    {
        public static Geometry Join(params Geometry[] geometry) => Join((IList<Geometry>)geometry);

        public static Geometry Join(IList<Geometry> geometry)
        {
            var final = new Geometry(
                new Vertex[geometry.Sum(static g => g.Vertices.Length)],
                new uint[geometry.Sum(static g => g.Indices.Length)]
                );

            int lastVertexIndex = 0;
            int lastElemIndex = 0;
            for (int i = 0; i < geometry.Count; i++)
            {
                var g = geometry[i];
                g.Vertices.CopyTo(final.Vertices, lastVertexIndex);
                g.Indices.CopyTo(final.Indices, lastElemIndex);
                for (int j = lastElemIndex; j < lastElemIndex + g.Indices.Length; j++) //offset index buffer 
                    final.Indices[j] += (uint)(lastVertexIndex);

                lastVertexIndex += g.Vertices.Length;
                lastElemIndex += g.Indices.Length;
            }

            return final;
        }

        public static void Transform(Geometry geometry, Matrix4x4 transformation)
        {
            for (int i = 0; i < geometry.Vertices.Length; i++)
                geometry.Vertices[i].Position = Vec3.Transform(geometry.Vertices[i].Position, transformation);
        }

        public static Rect<Vec3> GetBounds(Geometry geometry)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;

            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < geometry.Vertices.Length; i++)
            {
                var v = geometry.Vertices[i];

                minX = float.Min(minX, v.Position.X);
                minY = float.Min(minY, v.Position.Y);
                minZ = float.Min(minZ, v.Position.Z);

                maxX = float.Max(maxX, v.Position.X);
                maxY = float.Max(maxY, v.Position.Y);
                maxZ = float.Max(maxZ, v.Position.Z);
            }

            return new Rect<Vec3>(new Vec3(minX, minY, minZ), new Vec3(maxX, maxY, maxZ));
        }

        public static void CenterOrigin(Geometry geometry)
        {
            var bounds = GetBounds(geometry);
            var center = (bounds.Min + bounds.Max) * 0.5f;
            var offset = -center;

            for (int i = 0; i < geometry.Vertices.Length; i++)
                geometry.Vertices[i].Position += offset;
        }
    }
}