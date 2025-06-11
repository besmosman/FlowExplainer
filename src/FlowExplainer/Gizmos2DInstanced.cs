using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Gizmos2DInstanced
{
    [StructLayout(LayoutKind.Sequential)]
    struct CircleRenderInfo
    {
        public Vec2 Position;
        public float Radius;
        public float Padding;
        public Color Color;
    }
    
    private Mesh circleMesh => Gizmos2D.circleMesh;
    private AutoExpandStorageBuffer<CircleRenderInfo> circleStorage = new();
    private Material instanceMat = new Material(new Shader("Assets/Shaders/instanced.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);


    public void RegisterCircle(Vec2 center, float radius, Color color)
    {
        circleStorage.Register(new CircleRenderInfo()
        {
            Position = center,
            Radius = radius,
            Color = color
        });
    }

    public void RenderCircles(ICamera camera)
    {
        instanceMat.Use();
        instanceMat.SetUniform("view", camera.GetViewMatrix());
        instanceMat.SetUniform("tint",new Color(1,1,1));
        instanceMat.SetUniform("projection", camera.GetProjectionMatrix());
        
        circleStorage.Use();
        circleStorage.Upload();
        
        GL.BindVertexArray(Gizmos2D.circleMesh.VertexArrayObject);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Gizmos2D.circleMesh.IndexBufferObject);
        GL.DrawElementsInstanced(PrimitiveType.Triangles, Gizmos2D.circleMesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, circleStorage.GetCurrentIndex()); 
        circleStorage.Reset();
    }
}