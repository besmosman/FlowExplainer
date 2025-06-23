using System.Numerics;
using FlowExplainer.Msdf;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

using System;

public class AutoExpandStorageBuffer<TData> where TData : struct
{
    private StorageBuffer<TData> buffer = new(64);
    private int cur = 0;

    public AutoExpandStorageBuffer()
    {
    }

    public int GetCurrentIndex()
    {
        return cur;
    }

    public void Use()
    {
        buffer.Use();
    }

    public void Upload()
    {
        buffer.Upload();
    }


    public void Register(TData data)
    {
        buffer.Data[cur] = data;
        cur++;
        if (cur >= buffer.Data.Length)
        {
            var old = buffer.Data;
            buffer.Resize(buffer.Length * 2);
            Array.Copy(old, buffer.Data, old.Length);
        }
    }

    public void Reset()
    {
        cur = 0;
    }
}

public static class Gizmos2D
{
    private static Material material = Material.NewDefaultUnlit;
    public static Gizmos2DInstanced Instanced { get; } = new();
    
    public static Material texturedMat = new Material(
        Shader.DefaultWorldSpaceVertex,
        new Shader("Assets/Shaders/textured.frag", ShaderType.FragmentShader));

    private static Mesh quadMeshCentered;
    public static Mesh imageQuad;
    private static Mesh imageQuadInvertedY;
    public static Mesh circleMesh;
    private static Mesh debugTria;
    private static Mesh streamtube;
    private static Matrix4x4 view;
    private static Matrix4x4 projection;

    static Gizmos2D()
    {
        quadMeshCentered = new Mesh(new Geometry(
        [
            new Vertex(new Vec3(-.5f, -.5f, 0), Vec4.One),
            new Vertex(new Vec3(.5f, -.5f, 0), Vec4.One),
            new Vertex(new Vec3(.5f, .5f, 0), Vec4.One),
            new Vertex(new Vec3(-.5f, .5f, 0), Vec4.One),
        ], [0, 1, 2, 0, 2, 3]));

        imageQuad = new Mesh(new Geometry(
        [
            new Vertex(new Vec3(0f, 0f, 0), new Vec2(0, 1), Vec4.One),
            new Vertex(new Vec3(1f, 0f, 0), new Vec2(1, 1), Vec4.One),
            new Vertex(new Vec3(1f, 1f, 0), new Vec2(1, 0), Vec4.One),
            new Vertex(new Vec3(0f, 1f, 0), new Vec2(0, 0), Vec4.One),
        ], [0, 1, 2, 0, 2, 3]));


        imageQuadInvertedY = new Mesh(new Geometry(
        [
            new Vertex(new Vec3(0f, 0f, 0), new Vec2(0, 0), Vec4.One),
            new Vertex(new Vec3(1f, 0f, 0), new Vec2(1, 0), Vec4.One),
            new Vertex(new Vec3(1f, 1f, 0), new Vec2(1, 1), Vec4.One),
            new Vertex(new Vec3(0f, 1f, 0), new Vec2(0, 1), Vec4.One),
        ], [0, 1, 2, 0, 2, 3]));

        var v = new Vertex(default, default, new Vec4(1, 1, 1, 1));
        debugTria = new Mesh(new Geometry([v, v, v], [0, 1, 2]), dynamicVertices: true);

        var circleVerts = new List<Vertex>();
        var circleIndicies = new List<uint>();

        {
            int segments = 64;
            for (uint i = 0; i < segments + 1; i++)
            {
                circleVerts.Add(new Vertex(new Vec3(
                    MathF.Sin((float)(i) / segments * 2 * MathF.PI),
                    MathF.Cos((float)(i) / segments * 2 * MathF.PI),
                    0)));

                if (i != 0)
                {
                    circleIndicies.Add(0);
                    circleIndicies.Add((uint)circleVerts.Count);
                    circleIndicies.Add((uint)circleVerts.Count - 1);
                }
            }

            circleMesh = new Mesh(new Geometry(circleVerts.ToArray(), circleIndicies.ToArray()));
        }

        {
            var tubeVerts = new List<Vertex>();
            var indicies = new List<uint>();
            int segments = 32;
            for (uint i = 0; i < segments; i++)
            {
                tubeVerts.Add(new Vertex(new Vec3(i / (float)segments, -1f, 0)));
                tubeVerts.Add(new Vertex(new Vec3(i / (float)segments, 1f, 0)));
            }

            for (uint i = 1; i < segments; i++)
            {
                var cur = i * 2;
                indicies.Add(cur);
                indicies.Add(cur + 1);
                indicies.Add(cur - 1);

                indicies.Add(cur);
                indicies.Add(cur - 1);
                indicies.Add(cur - 2);
            }

            streamtube = new Mesh(new Geometry(tubeVerts.ToArray(), indicies.ToArray()), dynamicVertices: true);
        }
    }

    public static void Circle(ICamera camera, Vec2 center, Color color, float radius)
    {
        material.Use();
        material.SetUniform("tint", color);
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(center.X, center.Y, 0));
        circleMesh.Draw();
    }

    public static void Circles<T>(ICamera camera, IEnumerable<T> ts, Func<T, Vec2> getCenter, Func<T, Color> getColor, float radius)
    {
        material.Use();
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        foreach (var entry in ts)
        {
            var color = getColor(entry);
            var pos = getCenter(entry);
            material.SetUniform("tint", color);
            material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(pos.X, pos.Y, 0));
            circleMesh.Draw();
        }
    }


    public static void StreamTube(ICamera camera, List<Vec2> centers, Color color, float thickness)
    {
        if (centers.Count != streamtube.Vertices.Length / 2)
            throw new NotImplementedException();

        material.Use();
        material.SetUniform("tint", color);
        var view = camera.GetViewMatrix();
        var project = camera.GetProjectionMatrix();
        material.SetUniform("view", view);
        material.SetUniform("projection", project);
        /*
        0 => 0
        c => 1

        1 => 0
         */
        var total = 0f;
        for (int i = 1; i < centers.Count; i++)
        {
            total += Vec2.Distance(centers[i], centers[i - 1]);
        }

        if (total * 1f < thickness / 8)
        {
            /*for (int i = 1; i < centers.Count; i++)
            {
                centers[i] = new Vec2(centers[0].X + float.Lerp(-thickness, thickness, i / (float)centers.Count), centers[0].Y);
            }*/
            // Gizmos2D.Circle(camera, centers.Last(), color, thickness);
            return;
        }

        for (int i = 0; i < centers.Count; i++)
        {
            var dir = Vec2.Zero;
            if (i != 0)
                dir = Vec2.Normalize(centers[i] - centers[i - 1]);
            var normal = new Vec2(dir.Y, -dir.X);

            float c = (i / (float)centers.Count);
            var length = thickness * c * (thickness - c * c);
            length = MathF.Sqrt(1 - (c * 2 - 1) * (c * 2 - 1) * c) * c * thickness;
            streamtube.Vertices[i * 2 + 0].Position = new Vec3(centers[i] - normal * length, 0);
            streamtube.Vertices[i * 2 + 0].Colour.Y = MathF.Sqrt(c);
            streamtube.Vertices[i * 2 + 1].Position = new Vec3(centers[i] + normal * length, 0);
            streamtube.Vertices[i * 2 + 1].Colour.W = MathF.Sqrt(c);
        }

        streamtube.Upload(UploadFlags.Vertices);
        material.SetUniform("model", Matrix4x4.Identity);
        streamtube.Draw();
        //  Gizmos2D.Circle(camera, centers.Last(), color, 1f);
    }

    public static void ImageOld(ICamera camera, ImageTexture texture, float scale)
    {
        texturedMat.Use();
        texturedMat.SetUniform("tint", Vec4.One);
        texturedMat.SetUniform("view", camera.GetViewMatrix());
        texturedMat.SetUniform("projection", camera.GetProjectionMatrix());
        scale /= texture.Size.X;
        Vec2 lt = new Vec2(1200, 500);
        texturedMat.SetUniform("mainTex", texture);
        texturedMat.SetUniform("model", Matrix4x4.CreateScale(texture.Size.X * scale, texture.Size.Y * scale, .4f) *
                                        Matrix4x4.CreateTranslation(lt.X, lt.Y, 0));
        imageQuad.Draw();
    }

    public static void ImageCentered(ICamera camera, Texture texture, Vec2 center, float width, float alpha = 1)
    {
        texturedMat.Use();
        texturedMat.SetUniform("tint", new Vec4(1, 1, 1, alpha));
        texturedMat.SetUniform("view", camera.GetViewMatrix());
        texturedMat.SetUniform("projection", camera.GetProjectionMatrix());
        texturedMat.SetUniform("mainTex", texture);
        float height = (texture.Size.Y / (float)texture.Size.X) * width;
        texturedMat.SetUniform("model", Matrix4x4.CreateScale(width, height, .4f) * Matrix4x4.CreateTranslation(center.X - width / 2, center.Y - height / 2, 0));
        imageQuad.Draw();
    }

    public static void ImageCentered(ICamera camera, Texture texture, Vec2 center, Vec2 size, float alpha = 1)
    {
        texturedMat.Use();
        texturedMat.SetUniform("tint", new Vec4(1, 1, 1, alpha));
        texturedMat.SetUniform("view", camera.GetViewMatrix());
        texturedMat.SetUniform("projection", camera.GetProjectionMatrix());
        texturedMat.SetUniform("mainTex", texture);
        texturedMat.SetUniform("model", Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(center.X - size.X / 2, center.Y - size.Y / 2, 0));
        imageQuad.Draw();
    }

    public static void ImageCentered(ICamera camera, Texture texture, Vec2 center, Vec2 size, Vec4 tint)
    {
        texturedMat.Use();
        texturedMat.SetUniform("tint", tint);
        texturedMat.SetUniform("view", camera.GetViewMatrix());
        texturedMat.SetUniform("projection", camera.GetProjectionMatrix());
        texturedMat.SetUniform("mainTex", texture);
        texturedMat.SetUniform("model", Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(center.X - size.X / 2, center.Y - size.Y / 2, 0));
        imageQuad.Draw();
    }


    public static void ImageCenteredInvertedY(ICamera camera, Texture texture, Vec2 center, Vec2 size, float alpha = 1)
    {
        texturedMat.Use();
        texturedMat.SetUniform("tint", new Vec4(1, 1, 1, alpha));
        texturedMat.SetUniform("view", camera.GetViewMatrix());
        texturedMat.SetUniform("projection", camera.GetProjectionMatrix());
        texturedMat.SetUniform("mainTex", texture);
        texturedMat.SetUniform("model", Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(center.X - size.X / 2, center.Y - size.Y / 2, 0));
        imageQuadInvertedY.Draw();
    }


    public static float lineSpacing = 3;

    public static void AdvText(ICamera camera, Vec2 pos, float lh, Color color, string text, float t = 1, bool centered = false)
    {
        void SetCharColor(int i, Color col)
        {
            for (int j = 0; j < 6; j++)
            {
                MsdfRenderer.textMesh.Vertices[i * 6 + j].Colour = new Vec4(col.R,col.G,col.B,col.A);
            }
        }

        List<(Action<int, int>, int, int)> tasks = new();
        var splitted = text.Split("\n");
        var globalT = t;

        var sum = (float)text.Length;
        var cur = 0f;
        for (int l = 0; l < splitted.Length; l++)
        {
            tasks.Clear();
            var startPos = cur / sum;
            var endPos = (cur + splitted[l].Length) / sum;
            var localT = 0f;

            if (t <= startPos)
                localT = 0f;
            else if (t >= endPos)
                localT = 1f;
            else
                localT = (t - startPos) / (endPos - startPos);

            string? line = splitted[l].ReplaceLineEndings("");

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '@')
                {
                    var valueS = line.IndexOf('[', i) + 1;
                    var tag = line[(i + 1)..(valueS - 1)];
                    var valueE = line.IndexOf(']', i);
                    line = line[.. i] + line[valueS..valueE] + line[(valueE + 1)..];
                    int start = i;
                    int leng = valueE - valueS;
                    var colored = (Color col, int s, int e) =>
                    {
                        for (int j = s; j < e; j++)
                            SetCharColor(j, col);
                    };

                    Action<int, int> action = null;

                    if (tag == "red")
                        action = (s, e) => colored(new Color(.8f, .0f, .0f, 1), s, e);

                    if (tag == "green")
                        action = (s, e) => colored(new Color(.0f, .65f, .0f, 1), s, e);


                    if (tag.StartsWith("#"))
                    {
                        var col = Color.FromHexString(tag[1..]);
                        action = (s, e) => colored(col, s, e);
                    }

                    if (action != null)
                        tasks.Add((action, start, start + leng))
                            ;
                }
            }

            MsdfRenderer.UpdateMesh(line, camera, centered);
            for (int i = 0; i < MsdfRenderer.textMesh.Vertices.Length; i++)
            {
                MsdfRenderer.textMesh.Vertices[i].Colour =  color.ToVec4();
            }

            foreach (var task in tasks)
                task.Item1(task.Item2, task.Item3);

            MsdfRenderer.textMesh.Upload();
            MsdfRenderer.Material.Use();
            MsdfRenderer.Material.SetUniform("t", localT);
            MsdfRenderer.Material.SetUniform("line", (float)l);
            MsdfRenderer.Material.SetUniform("lines", (float)splitted.Length);
            MsdfRenderer.Material.SetUniform("tint", new Vec4(1, 1, 1, 1));
            MsdfRenderer.Material.SetUniform("screenPxRange", 4f);
            MsdfRenderer.Material.SetUniform("mainTex", MsdfRenderer.font.Texture);
            MsdfRenderer.Material.SetUniform("view", camera.GetViewMatrix());
            MsdfRenderer.Material.SetUniform("projection", camera.GetProjectionMatrix());
            MsdfRenderer.Material.SetUniform("model", Matrix4x4.CreateScale(lh, lh, 1) * Matrix4x4.CreateTranslation(pos.X, pos.Y - l * (lh + lineSpacing), 0));
            MsdfRenderer.Render();
            cur += line.Length;
        }
    }


    public static void Text(ICamera camera, Vec2 pos, float lh, Color color, string text, float t = 1, bool centered = false)
    {
        var splitted = text.Split("\n");
        var globalT = t;

        var sum = (float)text.Length;
        var cur = 0f;
        for (int l = 0; l < splitted.Length; l++)
        {
            var startPos = cur / sum;
            var endPos = (cur + splitted[l].Length) / sum;
            var localT = 0f;

            if (t <= startPos)
                localT = 0f;
            else if (t >= endPos)
                localT = 1f;
            else
                localT = (t - startPos) / (endPos - startPos);

            string? line = splitted[l].ReplaceLineEndings("").Trim();
            MsdfRenderer.UpdateMesh(line, camera, centered);
            MsdfRenderer.Material.Use();
            MsdfRenderer.Material.SetUniform("t", localT);
            MsdfRenderer.Material.SetUniform("line", (float)l);
            MsdfRenderer.Material.SetUniform("lines", (float)splitted.Length);
            MsdfRenderer.Material.SetUniform("tint", color);
            MsdfRenderer.Material.SetUniform("screenPxRange",1.2f);
            MsdfRenderer.Material.SetUniform("mainTex", MsdfRenderer.font.Texture);
            MsdfRenderer.Material.SetUniform("view", camera.GetViewMatrix());
            MsdfRenderer.Material.SetUniform("projection", camera.GetProjectionMatrix());
            MsdfRenderer.Material.SetUniform("model", Matrix4x4.CreateScale(lh, lh, 1) * Matrix4x4.CreateTranslation(pos.X, pos.Y - l * lh, 0));
            MsdfRenderer.Render();
            cur += splitted[l].Length;
        }
    }

    public static void DrawTria(ICamera cam, Vec2 p1, Vec2 p2, Vec2 p3, Vec4 color)
    {
        material.Use();
        debugTria.Vertices[0].Position = new Vec3(p1, 0f);
        debugTria.Vertices[1].Position = new Vec3(p2, 0f);
        debugTria.Vertices[2].Position = new Vec3(p3, 0f);
        debugTria.Upload(UploadFlags.Vertices);
        material.SetUniform("tint", color);
        material.SetUniform("view", cam.GetViewMatrix());
        material.SetUniform("projection", cam.GetProjectionMatrix());
        material.SetUniform("model", Matrix4x4.Identity);
        debugTria.Draw();
    }

    public static void Rect(ICamera cam, Vec2 start, Vec2 end, Vec4 color)
    {
        view = cam.GetViewMatrix();
        projection = cam.GetProjectionMatrix();
        var size = end - start;
        material.Use();
        material.SetUniform("tint", color);
        material.SetUniform("view", view);
        material.SetUniform("projection", projection);
        var model = Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(start.X, start.Y, 0);
        material.SetUniform("model", model);
        imageQuad.Draw();
    }

    
    public static void RectCenter(ICamera cam, Vec2 center, Vec2 size, Color color)
    {
        view = cam.GetViewMatrix();
        projection = cam.GetProjectionMatrix();

        material.Use();
        material.SetUniform("tint", color);
        material.SetUniform("view", view);
        material.SetUniform("projection", projection);
        var model = Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(center.X, center.Y, 0);
        material.SetUniform("model", model);
        quadMeshCentered.Draw();
    }

    //source claude
    public static void SetScissorPresiView(Vec2 center, Vec2 size)
    {
        var model = Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(center.X, center.Y, 0);

        Vec3[] corners = new Vec3[4]
        {
            Vec3.Transform(new Vec3(-0.5f, -0.5f, 0), model), // bottom-left
            Vec3.Transform(new Vec3(0.5f, -0.5f, 0), model), // bottom-right
            Vec3.Transform(new Vec3(0.5f, 0.5f, 0), model), // top-right
            Vec3.Transform(new Vec3(-0.5f, 0.5f, 0), model) // top-left
        };

// Project these corners to screen space
        Vec2[] screenCorners = new Vec2[4];
        for (int i = 0; i < 4; i++)
        {
            Vec4 clipSpace = Vec4.Transform(corners[i].Up(1.0f), view * projection);
            Vec3 ndcSpace = new Vec3(
                clipSpace.X / clipSpace.W,
                clipSpace.Y / clipSpace.W,
                clipSpace.Z / clipSpace.W);

            // Convert to screen space
            screenCorners[i] = new Vec2(
                (ndcSpace.X + 1.0f) * 0.5f * WindowService.SWindow.ClientSize.X,
                (ndcSpace.Y + 1f) * 0.5f * WindowService.SWindow.ClientSize.Y); // Flip Y for screen coordinates
        }

// Find the bounding rectangle in screen space
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var corner in screenCorners)
        {
            minX = Math.Min(minX, corner.X);
            minY = Math.Min(minY, corner.Y);
            maxX = Math.Max(maxX, corner.X);
            maxY = Math.Max(maxY, corner.Y);
        }

// Set up scissor rectangle
        int scissorX = (int)minX;
        int scissorY = (int)minY;
        int scissorWidth = (int)(maxX - minX);
        int scissorHeight = (int)(maxY - minY);

// Enable scissor test
        // GL.Enable(EnableCap.ScissorTest);
        GL.Scissor(scissorX - 1, scissorY - 1, scissorWidth + 2, scissorHeight + 2);
    }


    public static void LineCentered(ICamera cam, Vec2 center, Vec2 dir, Color color, float thickness)
    {
        material.Use();
        material.SetUniform("tint", color);
        material.SetUniform("view", cam.GetViewMatrix());
        material.SetUniform("projection", cam.GetProjectionMatrix());

        material.SetUniform("model",
            Matrix4x4.CreateScale(dir.Length(), thickness, 1) *
            Matrix4x4.CreateRotationZ(MathF.Atan2(dir.Y, dir.X)) *
            Matrix4x4.CreateTranslation(center.X, center.Y, 0));

        quadMeshCentered.Draw();
    }

    public static void Line(ICamera cam, Vec2 start, Vec2 end, Color color, float thickness)
    {
        Vec2 dir = Vec2.Normalize(end - start);
        float length = Vec2.Distance(start, end);
        material.Use();
        material.SetUniform("tint", color);
        material.SetUniform("view", cam.GetViewMatrix());
        material.SetUniform("projection", cam.GetProjectionMatrix());

        var s2 = start + dir / 2 * length * 1;
        material.SetUniform("model",
            Matrix4x4.CreateScale(length, thickness, 1) *
            Matrix4x4.CreateRotationZ(MathF.Atan2(dir.Y, dir.X)) *
            Matrix4x4.CreateTranslation(s2.X, s2.Y, 0));

        quadMeshCentered.Draw();
    }
}