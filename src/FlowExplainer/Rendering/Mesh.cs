using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class Mesh : IDisposable
    {
        public Geometry Geometry;

        public Vertex[] Vertices
        {
            get => Geometry.Vertices;
            set => Geometry.Vertices = value;
        }

        public uint[] Indices
        {
            get => Geometry.Indices;
            set => Geometry.Indices = value;
        }

        public ReadOnlySpan<IVertexAttributes> AdditionalAttributes => additionalAttributes.AsSpan();
        public PrimitiveType PrimitiveType = PrimitiveType.Triangles;
        public bool DynamicVerticies = false;
        public bool DynamicIndicies = false;

        public readonly int VertexArrayObject;
        public readonly int IndexBufferObject;
        public readonly int VertexBufferObject;

        private IVertexAttributes[] additionalAttributes;
        public int IndexCount { get; private set; }
        private readonly Dictionary<IVertexAttributes, int> additionalAttributeHandles = new();

        private static Queue<int> availiableVertexArrayIds = new();

        public Mesh(Geometry geometry, bool dynamicVertices = false, bool dynamicIndicies = false, params IVertexAttributes[]? additionalAttributes)
        {
            DynamicVerticies = dynamicVertices;
            DynamicIndicies = dynamicIndicies;
            Geometry = geometry;
            this.additionalAttributes = additionalAttributes ?? Array.Empty<IVertexAttributes>();


            VertexArrayObject = GetUnclaimedVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            IndexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);

            foreach (var attr in this.additionalAttributes)
            {
                var handle = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
                additionalAttributeHandles.Add(attr, handle);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

            int offset = 0;
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Stride, offset); //position
            offset += sizeof(float) * 3;

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.Stride, offset); //texcoords
            offset += sizeof(float) * 2;

            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, Vertex.Stride, offset);  //normal
            offset += sizeof(float) * 3;

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, Vertex.Stride, offset); //color
            offset += sizeof(float) * 4;

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);

            for (int i = 0; i < this.additionalAttributes.Length; i++)
            {
                var attr = this.additionalAttributes[i];
                var handle = additionalAttributeHandles[attr];
                int loc = i + 4;

                GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
                attr.CreateAttributeArray(loc, handle);
                GL.EnableVertexAttribArray(loc);
                GL.VertexAttribDivisor(loc, attr.Divisor);
            }

            Upload();
        }

        private int GetUnclaimedVertexArray()
        {
            if (availiableVertexArrayIds.Count == 0)
            {
                int[] v = new int[32];
                GL.GenVertexArrays(32, v);
                foreach (var id in v)
                    availiableVertexArrayIds.Enqueue(id);
            }
            return availiableVertexArrayIds.Dequeue();
        }

        public static Mesh FromArrays(Vertex[] verts, uint[] indices, bool dynamicVertices, bool dynamicIndicies, params IVertexAttributes[]? additionalAttributes)
            => new(new Geometry(verts, indices), dynamicVertices, dynamicIndicies, additionalAttributes);

        public void Upload(UploadFlags target = UploadFlags.All)
        {
            if (target.HasFlag(UploadFlags.Vertices) && Vertices == null && !DynamicVerticies)
                throw new Exception("This mesh has already been discarded. If you wish to upload the changed vertices to the GPU, mark the mesh as dynamicVertices.");

            var hint = DynamicVerticies ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

            //upload vertices
            if (target.HasFlag(UploadFlags.Vertices))
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * Vertex.Stride), Vertices, hint);
            }

            //upload indices
            if (target.HasFlag(UploadFlags.Indices))
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(uint)), Indices, hint);
            }

            //upload other attributes
            if (target.HasFlag(UploadFlags.AdditionalAttributes))
            {
                foreach (var a in additionalAttributeHandles)
                    a.Key.UploadData(a.Value);
            }
            if (!DynamicVerticies)
            {
                IndexCount = Indices.Length;
                Vertices = null;
                Indices = null;
            }

        }

        public T GetAttribute<T>() where T : class, IVertexAttributes
            => additionalAttributes.First(a => a is T) as T ??
               throw new Exception("There is no additional attribute array of type " + typeof(T));

        public void UploadAdditionalAttributes(IVertexAttributes attr)
        {
            var handle = GetAttributeHandle(attr);
            attr.UploadData(handle);
        }

        public void UploadAdditionalAttributes(int index)
        {
            var attr = additionalAttributes[index];
            UploadAdditionalAttributes(attr);
        }

        public int GetAttributeHandle(IVertexAttributes attr)
        {
            if (additionalAttributeHandles.TryGetValue(attr, out var handle))
                return handle;
            throw new Exception("Given attribute array does not exist in this mesh");
        }

        public void Draw()
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
            if (DynamicVerticies)
                GL.DrawElements(PrimitiveType, Indices.Length, DrawElementsType.UnsignedInt, 0);
            else
                GL.DrawElements(PrimitiveType, IndexCount, DrawElementsType.UnsignedInt, 0);
        }
        
        public void DrawInstanced(int count)
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
            GL.DrawElementsInstanced(PrimitiveType.Triangles,IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, count); 
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(IndexBufferObject);
            GL.DeleteBuffer(VertexBufferObject);
            foreach (var item in additionalAttributeHandles)
            {
                GL.DeleteBuffer(item.Value);
            }
        }
    }

    [Flags]
    public enum UploadFlags : byte
    {
        None = 0,
        Vertices = 2,
        Indices = 4,
        AdditionalAttributes = 8,
        All = byte.MaxValue
    }
}