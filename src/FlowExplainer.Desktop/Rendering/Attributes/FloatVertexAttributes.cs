using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class FloatVertexAttributes : IVertexAttributes<float>
    {
        public int Stride => sizeof(float);
        public Span<float> Data => data;
        public int Count { get; set; }

        public BufferUsageHint Usage { get; set; } = BufferUsageHint.StaticDraw;
        public bool Dynamic { get; set; }

        public int Divisor => 0;

        private float[] data;

        public FloatVertexAttributes(float[] data, bool dynamic)
        {
            this.data = data;
            Count = data.Length;
            Dynamic = dynamic;
        }

        public void CreateAttributeArray(int location, int handle)
        {
            GL.VertexAttribPointer(location, 1, VertexAttribPointerType.Float, false, Stride, 0);
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