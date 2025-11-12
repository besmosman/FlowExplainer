using System.Numerics;
using FlowExplainer;

namespace FlowExplainer
{
    public static class GeometryGen
    {
        public static Geometry WireCube(Vec3 o, Vec3 s, Vec4 color)
        {
            var vertices = new Vertex[8];
            var indices = new uint[]
            {
                0, 1, 1, 2, 2, 3, 3, 0, // face 1
                4, 5, 5, 6, 6, 7, 7, 4, // face 2

                0, 4, 1, 5, 2, 6, 3, 7 // bridge
            };
            var extents = s / 2;

            vertices[0] = new Vertex(new Vec3(0, 0, 0) * s - extents + o, color);
            vertices[1] = new Vertex(new Vec3(1, 0, 0) * s - extents + o, color);
            vertices[2] = new Vertex(new Vec3(1, 0, 1) * s - extents + o, color);
            vertices[3] = new Vertex(new Vec3(0, 0, 1) * s - extents + o, color);

            vertices[4] = new Vertex(new Vec3(0, 1, 0) * s - extents + o, color);
            vertices[5] = new Vertex(new Vec3(1, 1, 0) * s - extents + o, color);
            vertices[6] = new Vertex(new Vec3(1, 1, 1) * s - extents + o, color);
            vertices[7] = new Vertex(new Vec3(0, 1, 1) * s - extents + o, color);

            return new Geometry(vertices, indices);
        }

        public static Geometry Quad(Vec3 o, Vec2 s, Vec4 color)
        {
            var vertices = new Vertex[6];
            var indices = new uint[]
            {
                0, 1, 2,
                2, 3, 0
            };

            var s3 = new Vec3(s, 0);
            vertices[0] = new Vertex(new Vec3(0, 0, 0) * s3, new Vec2(0, 0), color);
            vertices[1] = new Vertex(new Vec3(1, 0, 0) * s3, new Vec2(1, 0), color);
            vertices[2] = new Vertex(new Vec3(1, 1, 0) * s3, new Vec2(1, 1), color);
            vertices[3] = new Vertex(new Vec3(0, 1, 0) * s3, new Vec2(0, 1), color);
            return new Geometry(vertices, indices);
        }

        public static Geometry TriangleCubeNoExtends(Vec3 o, Vec3 s, Vec4 color)
        {
            var vertices = new Vertex[8];
            var indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3,

                4, 5, 7,
                5, 6, 7,

                0, 1, 4,
                1, 4, 5,

                3, 2, 6,
                3, 6, 7,

                0, 3, 7,
                0, 4, 7,

                1, 5, 6,
                1, 2, 6
            };

            vertices[0] = new Vertex(new Vec3(0, 0, 0) * s + o, color);
            vertices[1] = new Vertex(new Vec3(1, 0, 0) * s + o, color);
            vertices[2] = new Vertex(new Vec3(1, 0, 1) * s + o, color);
            vertices[3] = new Vertex(new Vec3(0, 0, 1) * s + o, color);

            vertices[4] = new Vertex(new Vec3(0, 1, 0) * s + o, color);
            vertices[5] = new Vertex(new Vec3(1, 1, 0) * s + o, color);
            vertices[6] = new Vertex(new Vec3(1, 1, 1) * s + o, color);
            vertices[7] = new Vertex(new Vec3(0, 1, 1) * s + o, color);

            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];

                vert.Normal = Vec3.Normalize(vert.Position - o);

                vertices[i] = vert;
            }

            return new Geometry(vertices, indices);
        }


        public static Geometry TriangleCube(Vec3 o, Vec3 s, Vec4 color)
        {
            var vertices = new Vertex[8];
            var indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3,

                4, 5, 7,
                5, 6, 7,

                0, 1, 4,
                1, 4, 5,

                3, 2, 6,
                3, 6, 7,

                0, 3, 7,
                0, 4, 7,

                1, 5, 6,
                1, 2, 6
            };
            var extents = s / 2;

            vertices[0] = new Vertex(new Vec3(0, 0, 0) * s - extents + o, color);
            vertices[1] = new Vertex(new Vec3(1, 0, 0) * s - extents + o, color);
            vertices[2] = new Vertex(new Vec3(1, 0, 1) * s - extents + o, color);
            vertices[3] = new Vertex(new Vec3(0, 0, 1) * s - extents + o, color);

            vertices[4] = new Vertex(new Vec3(0, 1, 0) * s - extents + o, color);
            vertices[5] = new Vertex(new Vec3(1, 1, 0) * s - extents + o, color);
            vertices[6] = new Vertex(new Vec3(1, 1, 1) * s - extents + o, color);
            vertices[7] = new Vertex(new Vec3(0, 1, 1) * s - extents + o, color);

            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];

                vert.Normal = Vec3.Normalize(vert.Position - o);

                vertices[i] = vert;
            }

            return new Geometry(vertices, indices);
        }

        public static Geometry WireGrid(int resolution, Vec4 color)
        {
            var vertices = new Vertex[resolution * 2 * 2 + 4]; //+4 for the final edges.
            var indices = new uint[resolution * 2 * 2 + 4];
            uint n = 0;
            for (int x = 0; x <= resolution; x++)
            {
                indices[n] = n;
                vertices[n++] = new Vertex(new Vec3(x, 0, 0), color);

                indices[n] = n;
                vertices[n++] = new Vertex(new Vec3(x, resolution, 0), color);
            }

            for (int z = 0; z <= resolution; z++)
            {
                indices[n] = n;
                vertices[n++] = new Vertex(new Vec3(0, z, 0), color);

                indices[n] = n;
                vertices[n++] = new Vertex(new Vec3(resolution, z, 0), color);
            }

            return new Geometry(vertices, indices);
        }

        public static Geometry WireTriangle(Vec3 a, Vec3 b, Vec3 c, Vec4 color)
        {
            var vertices = new[]
            {
                new Vertex(a, color),
                new Vertex(b, color),
                new Vertex(c, color)
            };

            var indices = new uint[] { 0, 1, 2, 0 };
            return new Geometry(vertices, indices);
        }

        public static Geometry WireCircle(uint resolution, float radius, Quaternion rotation, Vec4 color)
        {
            resolution = Math.Max(3, resolution);

            var vertices = new Vertex[resolution];
            var indices = new uint[resolution + 1];

            int ii = 0;
            for (uint i = 0; i < resolution; i++)
            {
                float th = float.Tau * (i / (float)(resolution - 1));
                var vert = new Vertex(
                    Vec3.Transform(new Vec3(float.Cos(th) * radius, float.Sin(th) * radius, 0), rotation),
                    color
                );

                vertices[i] = vert;
                indices[ii++] = i;
            }

            indices[ii++] = resolution;
            return new Geometry(vertices, indices);
        }

        //https://danielsieger.com/blog/2021/03/27/generating-spheres.html
        public static Geometry UVSphere(uint nSlices, uint nStacks)
        {
            nSlices = Math.Max(3, nSlices);
            nStacks = Math.Max(2, nStacks);

            var vertices = new List<Vertex>();
            var indices = new List<uint>();

            // Add top vertex
            vertices.Add(new Vertex(new Vec3(0, 1, 0), Vec4.One));

            // Generate vertices per stack / slice
            for (uint i = 0; i < nStacks - 1; i++)
            {
                float phi = MathF.PI * (i + 1) / nStacks;
                for (uint j = 0; j < nSlices; j++)
                {
                    float theta = 2.0f * MathF.PI * j / nSlices;
                    float x = MathF.Sin(phi) * MathF.Cos(theta);
                    float y = MathF.Cos(phi);
                    float z = MathF.Sin(phi) * MathF.Sin(theta);
                    vertices.Add(new Vertex(new Vec3(x, y, z), Vec4.One));
                }
            }

            // Add bottom vertex
            vertices.Add(new Vertex(new Vec3(0, -1, 0), Vec4.One));

            uint vTop = 0;
            uint vBottom = (uint)vertices.Count - 1;

            // Add top triangles
            for (uint i = 0; i < nSlices; i++)
            {
                uint i0 = i + 1;
                uint i1 = (i + 1) % nSlices + 1;
                indices.Add(vTop);
                indices.Add(i1);
                indices.Add(i0);
            }

            // Add bottom triangles
            for (uint i = 0; i < nSlices; i++)
            {
                uint i0 = i + nSlices * (nStacks - 2) + 1;
                uint i1 = (i + 1) % nSlices + nSlices * (nStacks - 2) + 1;
                indices.Add(vBottom);
                indices.Add(i0);
                indices.Add(i1);
            }

            // Add quads as two triangles per stack / slice
            for (uint j = 0; j < nStacks - 2; j++)
            {
                uint j0 = j * nSlices + 1;
                uint j1 = (j + 1) * nSlices + 1;
                for (uint i = 0; i < nSlices; i++)
                {
                    uint i0 = j0 + i;
                    uint i1 = j0 + (i + 1) % nSlices;
                    uint i2 = j1 + (i + 1) % nSlices;
                    uint i3 = j1 + i;

                    indices.Add(i0);
                    indices.Add(i1);
                    indices.Add(i2);
                    indices.Add(i0);
                    indices.Add(i2);
                    indices.Add(i3);
                }
            }

            return new Geometry(vertices.ToArray(), indices.ToArray());
        }
    }
}