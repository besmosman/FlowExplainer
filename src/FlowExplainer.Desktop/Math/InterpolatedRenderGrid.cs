using System.Linq.Expressions;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public abstract class InterpolatedRenderGrid
{
    protected Material material;
    public bool BilinearInterpolation { get; set; } = false;

    public abstract void UploadData();
    public abstract void Draw(ICamera camera, Vec2 start, Vec2 size);
    public abstract void Resize(Vec2i newSize);

    public void UploadColorGradient(ColorGradient gradient)
    {
        material.SetUniform("colorgradient", gradient.Texture.Value);
    }

    public abstract void RebuildMaterial();
}

public class InterpolatedRenderGrid<T> : InterpolatedRenderGrid where T : struct
{
    public Vec2i GridSize { get; private set; }

    private StorageBuffer<T> buffer;
    private T[] Data => buffer.Data;

    public Expression<Func<GlobalGPUData, Color>> toColor;

    public InterpolatedRenderGrid(Vec2i gridSize)
    {
        GridSize = gridSize;
        buffer = new StorageBuffer<T>(gridSize.X * gridSize.Y);
        toColor = (data) => new Color(1, 1, 1, 1);
        RebuildMaterial();
    }

    public sealed override void RebuildMaterial()
    {
        var glsl = ExpressionTreeShaderGen<T>.ToGlsl(File.ReadAllText("Assets/Shaders/grid-gen.frag"), this.toColor.Body);
        var shader = Shader.FromSource(glsl, ShaderType.FragmentShader);
        material = new Material(Shader.DefaultWorldSpaceVertex, shader);
    }

    public void SetColorFunction(Expression<Func<GlobalGPUData, Color>> colorExpression)
    {
        if (this.toColor.ToString() != colorExpression.ToString())
        {
            this.toColor = colorExpression;
            RebuildMaterial();
        }
    }

    public ref T AtCoords(int x, int y) => ref AtCoords(new Vec2i(x, y));

    public ref T AtCoords(Vec2i p)
    {
        var coords = p.Y * GridSize.X + p.X;
        return ref Data[coords];
    }


    public override void UploadData()
    {
        buffer.Upload();
    }

    public override void Draw(ICamera camera, Vec2 start, Vec2 size)
    {
        material.Use();
        material.SetUniform("gridSize", GridSize.ToVec2());
        material.SetUniform("tint", new Color(1, 1, 0, 1));
        material.SetUniform("interpolate", BilinearInterpolation);
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        material.SetUniform("model", Matrix4x4.CreateScale((float)size.X, (float)size.Y, .4f) * Matrix4x4.CreateTranslation((float)start.X, (float)start.Y, 0));
        buffer.Use();

        Gizmos2D.imageQuadInvertedY.Draw();
    }

    public override void Resize(Vec2i newSize)
    {
        GridSize = newSize;
        buffer.Resize(newSize.X * newSize.Y);
    }

    public class GlobalGPUData
    {
        public Vec2 GridSize;
        public T[] data;

        public double Multiplier => throw new Exception();

        public T Dat => throw new Exception();

        public Color ColorGradient(double v) => throw new Exception();

        public int GetIndex(Vec2 coords)
        {
            throw new Exception();
        }

        public T Raw<T>(string text) => throw new Exception();

        public T Interpolate(Vec2 uv)
        {
            throw new Exception();
        }

        public int GetCellAt(Vec2 uv)
        {
            throw new Exception();
        }

        public double floor(double x)
        {
            throw new Exception();
        }

        public double length(Vec2 x)
        {
            throw new Exception();
        }

        public double dot(Vec2 x, Vec2 y)
        {
            throw new Exception();
        }

        public double sqrt(Vec2 x)
        {
            throw new Exception();
        }
    }
}