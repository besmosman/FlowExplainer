using System.Linq.Expressions;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp;
using GL = OpenTK.Graphics.OpenGL.GL;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace FlowExplainer;

public class InterpolatedRenderGrid<T> where T : struct
{
    public class GlobalGPUData
    {
        public Vec2 GridSize;
        public T[] data;

        public int GetIndex(Vec2 coords)
        {
            throw new Exception();
        }

        public int GetCellAt(Vec2 uv)
        {
            throw new Exception();
        }

        public float floor(float x)
        {
            throw new Exception();
        }
    }

    private T[] Data;
    public Vec2i GridSize { get; private set; }

    private StorageBuffer<T> buffer;
    public Material material;

    public Expression<Func<GlobalGPUData, T, Color>> toColor;

    public InterpolatedRenderGrid(Vec2i gridSize)
    {
        GridSize = gridSize;
        buffer = new StorageBuffer<T>(gridSize.X * gridSize.Y);
        Data = buffer.Data;
        toColor = (data, arg2) => new Color(1, 1, 1, 1);
        RebuildMaterial();
    }

    private void RebuildMaterial()
    {
        var glsl = ExpressionTreeShaderGen<T>.ToGlsl(File.ReadAllText("Assets/Shaders/grid-gen.frag"), this.toColor.Body);
        var shader = Shader.FromSource(glsl, ShaderType.FragmentShader);
        material = new Material(Shader.DefaultWorldSpaceVertex, shader);
    }

    public void UpdateColorFunction(Expression<Func<GlobalGPUData, T, Color>> colorExpression)
    {
        if (this.toColor.GetHashCode() != colorExpression.GetHashCode())
        {
            this.toColor = colorExpression;
            RebuildMaterial();
        }
    }

    public ref T AtCoords(Vec2i p)
    {
        var coords = p.Y * GridSize.X + p.X;
        return ref Data[coords];
    }


    public void Draw(ICamera camera, Vec2 start, Vec2 size)
    {
        material.Use();
        material.SetUniform("gridSize", GridSize.ToVec2());
        material.SetUniform("tint", new Color(1, 1, 0, 1));
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        material.SetUniform("model", Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(start.X, start.Y, 0));
        buffer.Use();
        buffer.Upload();
        Gizmos2D.imageQuad.Draw();
    }
}

public class FlowFieldVisualizer : WorldService
{
    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        var domainArea = dat.VelocityField.Domain.Size.X * dat.VelocityField.Domain.Size.Y;


        ImGui.SliderInt("Grid Cells", ref GridCells, 0, 1500);
        ImGui.SliderFloat("Length", ref Length, 0, 1);
        ImGui.SliderFloat("Thickness", ref Thickness, 0, dat.VelocityField.Domain.Size.Length() / 10f);
        ImGui.Checkbox("Auto Resize", ref AutoResize);
        base.DrawImGuiEdit();
    }

    public int GridCells;
    public float Length;
    public float Thickness;
    public bool AutoResize = true;

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();

        var domain = dat.VelocityField.Domain;
        var domainArea = domain.Size.X * domain.Size.Y;
        var spacing = MathF.Sqrt(domainArea / GridCells);
        var maxDirLenght2 = 0f;
        var gridSize = (domain.Size / spacing).CeilInt();
        var cellSize = domain.Size / gridSize.ToVec2();
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                var rel = new Vec2(x + .5f, y + .5f) / gridSize.ToVec2();
                var pos = rel * domain.Size + domain.Min;
                // Gizmos2D.RectCenter(view.Camera2D, pos, cellSize, new Vec4(x % 2 == 0 ? .4f : 0, .4f, y % 2 == 1 ? .8f : 0, 1));
                var endpos = dat.Integrator.Integrate(dat.VelocityField.Evaluate, pos.Up(dat.SimulationTime), .1f);
                var dir = dat.VelocityField.Evaluate(pos.Up(dat.SimulationTime));
                maxDirLenght2 = MathF.Max(maxDirLenght2, dir.LengthSquared());
                var color = new Color(1, 1, 1, 1);

                var traj = IFlowOperator<Vec2, Vec3>.Default.Compute(dat.SimulationTime, dat.SimulationTime + .05f, pos, dat.VelocityField);
                var sum = 0f;
                for (int i = 1; i < traj.Entries.Count; i++)
                {
                    var last = traj.Entries[i - 1];
                    var cur = traj.Entries[i];
                    sum += (cur.Down() - last.Down()).Length() / (cur.Last - last.Last);
                }

                var avgSpeed = traj.AverageAlong((prev, cur) => (cur.XY - prev.XY) / (cur.Last - prev.Last)).Abs();

                color = new Color(0, 0, avgSpeed.LengthSquared(), 1);
                color = Gradients.GetGradient("matlab_jet").GetCached(avgSpeed.LengthSquared());
                //Gizmos2D.Circle(view.Camera2D, pos, color, Thickness / 2 * 1.0f);
                //Gizmos2D.LineCentered(view.Camera2D, pos, dir * Length, color, Thickness);
                var end = pos + dir * Length / 2;
                //Gizmos2D.Circle(view.Camera2D, pos + dir*Length, new Color(1, 1, 1, 1), Thickness/2 * 1.8f);
                //var line = StreamLineGenerator.Generate(dat.VelocityField, dat.Integrator, pos, dat.SimulationTime, 0.3f, 64);
                Gizmos2D.StreamTube(view.Camera2D, traj.Entries.Select(s => s.XY).ToList(), color, Thickness);
            }
        }

        if (AutoResize)
        {
            Length = (spacing / 1) / float.Sqrt(maxDirLenght2);
            Thickness = cellSize.X / 7;
        }
    }
}