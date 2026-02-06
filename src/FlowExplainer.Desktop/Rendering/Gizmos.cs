using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public static class Gizmos
{
    public static Material debugMat = Material.NewDefaultUnlit;
    public static Material texturedMat = new Material(
        Shader.DefaultWorldSpaceVertex,
        new Shader("Assets/Shaders/textured.frag", ShaderType.FragmentShader));

    public static Mesh sphereMesh;
    public static Mesh UnitCube;
    public static Mesh Quad;

    static Gizmos()
    {
        sphereMesh = new Mesh(GeometryGen.UVSphere(5, 5));
        UnitCube = new Mesh(GeometryGen.TriangleCube(Vec3.Zero, Vec3.One, Vec4.One));
        UnitCube.PrimitiveType = PrimitiveType.Triangles;
        Quad = new Mesh(GeometryGen.Quad(Vec3.Zero, Vec2.One, Vec4.One));
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


    public static void DrawTexturedQuadXY(ICamera camera, Texture texture, Vec3 pos, Vec2 size)
    {
        texturedMat.Use();
        texturedMat.SetUniform("view", camera.GetViewMatrix());
        texturedMat.SetUniform("projection", camera.GetProjectionMatrix());
        texturedMat.SetUniform("model", Matrix4x4.CreateScale((float)size.X, (float)size.Y, 1) *
                                        Matrix4x4.CreateTranslation(pos.ToNumerics()));
        texturedMat.SetUniform("tint", Color.White);
        texturedMat.SetUniform("mainTex", texture);
        Quad.Draw();
    }

    public static void DrawLine(View view, Vec3 p1, Vec3 p2, double thickness, Color color)
    {
        var dis = Vector3.Distance(p1, p2);
        debugMat.Use();
        debugMat.SetUniform("tint", color);
        debugMat.SetUniform("view", view.Camera.GetViewMatrix());
        debugMat.SetUniform("projection", view.Camera.GetProjectionMatrix());
        debugMat.SetUniform("model",
            Matrix4x4.CreateScale(new Vector3((float)thickness, (float)thickness, dis)) *
            Vec3.LookAtDirection(Vec3.Normalize(p2 - p1)) *
            Matrix4x4.CreateTranslation(p1 + (p2 - p1) / 2));
        UnitCube.Draw();
    }






    public static GizmosInstanced Instanced = new();

    public class GizmosInstanced
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SphereRenderInfo
        {
            public float PositionX;
            public float PositionY;
            public float PositionZ;
            public float Radius;
            public Color Color;
        }

        private AutoExpandStorageBuffer<SphereRenderInfo> sphereStorage = new();
        private Material sphereMat = new Material(new Shader("Assets/Shaders/sphere-instanced.vert",
            ShaderType.VertexShader), Shader.DefaultUnlitFragment);
        private Material sphereMatLit = new Material(new Shader("Assets/Shaders/sphere-instanced.vert",
            ShaderType.VertexShader), Shader.DefaultLitFragment);
        public void RegisterSphere(Vec3 center, double radius, Color color)
        {
            sphereStorage.Register(new SphereRenderInfo
            {
                PositionX = (float)center.X,
                PositionY = (float)center.Y,
                PositionZ = (float)center.Z,
                Radius = (float)radius,
                Color = color,
            });
        }

        public void DrawSpheres(ICamera camera)
        {
            sphereMat.Use();
            sphereMat.SetUniform("view", camera.GetViewMatrix());
            sphereMat.SetUniform("tint", new Color(1, 1, 1));
            sphereMat.SetUniform("projection", camera.GetProjectionMatrix());

            sphereStorage.Use();
            sphereStorage.Upload();

            sphereMesh.DrawInstanced(sphereStorage.GetCurrentIndex());
            sphereStorage.Reset();
        }

        public void DrawSpheresLit(ICamera camera)
        {
            sphereMatLit.Use();
            sphereMatLit.SetUniform("view", camera.GetViewMatrix());
            sphereMatLit.SetUniform("tint", new Color(1, 1, 1));
            sphereMatLit.SetUniform("projection", camera.GetProjectionMatrix());

            sphereStorage.Use();
            sphereStorage.Upload();

            sphereMesh.DrawInstanced(sphereStorage.GetCurrentIndex());
            sphereStorage.Reset();
        }
    }
}