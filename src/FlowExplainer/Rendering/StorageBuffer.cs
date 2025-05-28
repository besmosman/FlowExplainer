using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace FlowExplainer
{
    public class StorageBuffer<T> : IDisposable where T : struct
    {
        public int Id;
        public T[] Data;

        public StorageBuffer(int size)
        {
            Id = GL.GenBuffer();
            Data = new T[size];
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, Data.Length * Marshal.SizeOf<T>(), Data, BufferUsageHint.DynamicDraw);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Id);
        }

        public void Use()
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, Id); //hard coded 2 for now because I am lazy :)
        }

        public void Upload()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, Data.Length * Marshal.SizeOf<T>(), Data);
        }
    }
}