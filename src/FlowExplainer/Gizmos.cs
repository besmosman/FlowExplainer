using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public static class Gizmos
{
    public static Material debugMat = Material.NewDefaultUnlit;
    public static Mesh sphereMesh;

    static Gizmos()
    {        
        sphereMesh = new Mesh(GeometryGen.UVSphere(6, 6));
    }

    public static void DrawSphere(View view, Vector3 pos, Vector3 size, Color color)
    {
        debugMat.Use();
        debugMat.SetUniform("view", view.Camera.GetViewMatrix());
        debugMat.SetUniform("projection",
            view.Camera.GetProjectionMatrix());
        debugMat.SetUniform("model", Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(pos + size / 2));
        debugMat.SetUniform("tint", color);
        sphereMesh.Draw();
    }

    public static GizmosInstanced Instanced = new();
    
    public class GizmosInstanced
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SphereRenderInfo
        {
            public Vec3 Position;
            public float Radius;
            public Color Color;
        }
        private AutoExpandStorageBuffer<SphereRenderInfo> sphereStorage = new();
        private Material sphereMat = new Material(new Shader("Assets/Shaders/sphere-instanced.vert", 
            ShaderType.VertexShader), Shader.DefaultUnlitFragment);

        public void RegisterSphere(Vec3 center, float radius, Color color)
        {
            sphereStorage.Register(new SphereRenderInfo
            {
                Position = center,
                Color = color,
                Radius = radius,
            });
        }

        public void DrawSpheres(ICamera camera)
        {
            sphereMat.Use();
            sphereMat.SetUniform("view", camera.GetViewMatrix());
            sphereMat.SetUniform("tint",new Color(1,1,1));
            sphereMat.SetUniform("projection", camera.GetProjectionMatrix());
        
            sphereStorage.Use();
            sphereStorage.Upload();
            
            sphereMesh.DrawInstanced(sphereStorage.GetCurrentIndex());
            sphereStorage.Reset();
        }
    }
}