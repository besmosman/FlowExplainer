using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public interface IVertexAttributes
    {
        public int Stride { get; }
        public BufferUsageHint Usage { get; set; }
        public bool Dynamic { get; set; }
        public void CreateAttributeArray(int location, int handle);
        public void UploadData(int handle);
        public int Count { get; }
        public int Divisor { get; }
    }

    public interface IVertexAttributes<T> : IVertexAttributes where T : struct
    {
        public Span<T> Data { get; }
    }
}