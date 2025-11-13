using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class FloatVertexAttributes : IVertexAttributes<double>
    {
        public int Stride => sizeof(double);
        public Span<double> Data => data;
        public int Count { get; set; }

        public BufferUsageHint Usage { get; set; } = BufferUsageHint.StaticDraw;
        public bool Dynamic { get; set; }

        public int Divisor => 0;

        private double[] data;

        public FloatVertexAttributes(double[] data, bool dynamic)
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