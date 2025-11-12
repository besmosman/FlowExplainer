using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace FlowExplainer
{
    public class InstancedArray<T> : IDisposable where T : struct
    {
        private int Id;
        public T[] Data;


        public void Resize(int n)
        {
            Data = new T[n];
        }

        public InstancedArray(int length)
        {
            Data = new T[length];
        }

        public void Use()
        {
            Id = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            GL.BufferData(BufferTarget.ArrayBuffer, Marshal.SizeOf<T>() * Data.Length, Data, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Id);
        }
    }

    public class StorageBuffer<T> : IDisposable where T : struct
    {
        public int Id;
        public int Length;
        public T[] Data;

        public StorageBuffer(int size)
        {
            Id = GL.GenBuffer();
            Data = new T[size];
            Length = Data.Length;
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, Data.Length * Marshal.SizeOf<T>(), Data, BufferUsageHint.DynamicDraw);
        }
        
        public StorageBuffer(T[] data)
        {
            Id = GL.GenBuffer();
            Data = data;
            Length = Data.Length;
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, Data.Length * Marshal.SizeOf<T>(), Data, BufferUsageHint.DynamicDraw);
        }

        public void Resize(int length)
        {
            Data = new T[length];
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, Data.Length * Marshal.SizeOf<T>(), Data, BufferUsageHint.DynamicDraw);
            Length = length;
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Id);
        }

        public void Use()
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, Id); //hard coded 2 for now because I am lazy :)
        }

        public void Upload(int? maxlength = null)
        {
            maxlength ??= Length;
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Id);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, maxlength.Value * Marshal.SizeOf<T>(), Data);
        }
    }
}