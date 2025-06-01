using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class UintVertexAttributes : IVertexAttributes<uint>
    {
        public int Stride => sizeof(uint);
        public Span<uint> Data => data;
        public int Count { get; set; }

        public BufferUsageHint Usage { get; set; } = BufferUsageHint.StaticDraw;
        public bool Dynamic { get; set; }

        public int Divisor => 0;

        private uint[] data;

        public UintVertexAttributes(uint[] data, bool dynamic)
        {
            this.data = data;
            Dynamic = dynamic;
            Count = data.Length;
        }

        public void CreateAttributeArray(int location, int handle)
        {
            GL.VertexAttribIPointer(location, 1, VertexAttribIntegerType.UnsignedInt, Stride, 0);
        }

        public void UploadData(int handle)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle);

            if (data != null)
                GL.BufferData(BufferTarget.ArrayBuffer, Stride * Count, data, Usage);

            if (!Dynamic && data != null)
                data = null;
        }
    }

    public class Vec3VertexAttributes : IVertexAttributes<Vec3>
    {
        public int Stride => sizeof(float) * 3;
        public Span<Vec3> Data => data;
        public int Count { get; set; }

        public BufferUsageHint Usage { get; set; } = BufferUsageHint.StaticDraw;
        public bool Dynamic { get; set; }

        public virtual int Divisor => 0;

        private Vec3[] data;

        public Vec3VertexAttributes(Vec3[] data, bool dynamic = false)
        {
            this.data = data;
            Dynamic = dynamic;
            Count = data.Length;
        }

        public void CreateAttributeArray(int location, int handle)
        {
            GL.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, Stride, 0);
        }

        public void UploadData(int handle)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle);

            if (data != null)
                GL.BufferData(BufferTarget.ArrayBuffer, Stride * Count, data, Usage);

            if (!Dynamic && data != null)
                data = null;
        }
    }
}