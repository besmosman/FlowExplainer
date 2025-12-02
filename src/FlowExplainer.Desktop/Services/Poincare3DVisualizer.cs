using System.Numerics;
using OpenTK.Graphics.OpenGL;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace FlowExplainer;

public class Poincare3DVisualizer : WorldService
{
    class Path
    {
        public Trajectory<Vec3> Trajectory;
        public Mesh Mesh;
    }

    private List<Path> paths = new();

    public double offset = .4f;
    public double sliceT =0.0;
    public double t;
    public int periods = 1000;
    int stepsPerPeriod = 100;
    public double tubeFactor =0.0;
    public override void Initialize()
    {
        material = new Material(Shader.DefaultWorldSpaceVertex, new Shader("Assets//Shaders//traject.frag", ShaderType.FragmentShader));
    }
    public void SetupTrajects(IEnumerable<Vec2> seeds)
    {
        paths.Clear();
        var dat = GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;
        double period = rect.Size.Z;
        var flowOperator = new IFlowOperator<Vec2, Vec3>.DefaultFlowOperator(stepsPerPeriod * periods);

        foreach (var pos in seeds)
        {
            var traj = flowOperator.Compute(0, periods * period, pos, dat.VectorField);
            var path = new Path
            {
                Trajectory = traj,
                Mesh = new Mesh(new Geometry([], []), true, true)
                {
                    PrimitiveType = PrimitiveType.Lines,
                }
            };
            UpdateLineMesh(path, period, offset);
            paths.Add(path);
        }

    }

    private void UpdateLineMesh(Path path, double period, double offset)
    {
        Vertex[] vertices = new Vertex[path.Trajectory.Entries.Length];
        List<uint> indicies = new List<uint>(path.Trajectory.Entries.Length * 2);
        for (uint i = 0; i < path.Trajectory.Entries.Length; i++)
        {
            var phase = path.Trajectory.Entries[i];
            var pos3d = phase;
            if (tubeFactor > .5f)
                pos3d = PhaseToPos3D(period, offset, phase);
            else
                pos3d.Z %= 1;
            vertices[i] = new Vertex
            {
                //Position = phase,
                Position = pos3d,
                Colour = new Vec4(1, 1, 1, 1),
                TexCoords = new Vec2(phase.Z),
            };
            if (i != 0)
            {
                //wrapped
                if (double.Abs(path.Trajectory.Entries[i - 1].X - path.Trajectory.Entries[i].X) > .4f ||
                    (tubeFactor < .5f && double.Abs(path.Trajectory.Entries[i - 1].Z % 1 - path.Trajectory.Entries[i].Z % 1) > .4f))
                {

                }
                else
                {
                    indicies.Add(i);
                    indicies.Add(i - 1);
                }
            }
        }
        path.Mesh.Vertices = vertices;
        path.Mesh.Indices = indicies.ToArray();
        path.Mesh.Upload();
    }
    private Vec3 PhaseToPos3D(double period, double offset, Vec3 phase)
    {
        phase.Z %= period;
        var matrix = Matrix4x4.CreateRotationY((float)((phase.Last / period) * 2 * double.Pi));
        var pos3d = Vector3.Transform((phase.XY.Up(0) + new Vec3(offset, 0, 0)).ToNumerics(), matrix);
        var pos3dvec = new Vec3(pos3d.X, pos3d.Y, pos3d.Z);
        return pos3dvec;
        return Utils.Lerp(phase, pos3dvec, tubeFactor);
    }

    public double speed;
    private Material material;
    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;
        GL.Enable(EnableCap.DepthTest);

        t += FlowExplainer.DeltaTime * speed;
        //paths.Clear();
        //Initialize();
        var dat = GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;
        double period = rect.Size.Z;
        UpdateLines(1);
        material.Use();
        material.SetUniform("t", t);
        material.SetUniform("tint", new Color(0, .8f, .5f, 1));
        material.SetUniform("view", view.Camera.GetViewMatrix());
        material.SetUniform("projection", view.Camera.GetProjectionMatrix());
        material.SetUniform("model",
            Matrix4x4.CreateScale(1, .5f, 0.04f) *
            Matrix4x4.CreateTranslation(.5f + (tubeFactor > .5f ? (float)offset : 0) , .25f, tubeFactor <.5f ? (float)sliceT : 0) *
            Matrix4x4.CreateRotationY((float)(sliceT * 2 * double.Pi *  tubeFactor)));
        Gizmos.UnitCube.Draw();

        material.SetUniform("tint", Color.White);
        material.SetUniform("view", view.Camera.GetViewMatrix());
        material.SetUniform("projection", view.Camera.GetProjectionMatrix());
        material.SetUniform("model", Matrix4x4.Identity);
        foreach (var path in paths)
        {
            UpdateLineMesh(path, period, offset);
            path.Mesh.Draw();
        }
        GL.Disable(EnableCap.DepthTest);

        foreach (var p in paths)
        {
            var last =0.0;
            for (int i = 1; i < p.Trajectory.Entries.Length; i++)
            {
                var e = p.Trajectory.Entries[i];
                var cur = e.Z % period;
                if (t < e.Z)
                    break;

                if (last <= sliceT && cur >= sliceT)
                {
                    var l = p.Trajectory.Entries[i - 1];
                    var c = p.Trajectory.Entries[i];
                    if (l.X - c.X > .4f)
                        l.X -= 1;

                    if (l.X - c.X < -.4f)
                        l.X += 1;
                    double factor = (sliceT - (l.Last % period)) / ((c - l).Last % period);
                    factor = double.Clamp(factor, 0, 1);
                    var f = Utils.Lerp(l, c, factor);
                    f.X %= period;
                    f.Z %= period;
                    var phaseToPos3D = PhaseToPos3D(period, offset, f);
                    if (tubeFactor < 1)
                        phaseToPos3D = f.XY.Up(sliceT);
                    Gizmos.DrawSphere(view, phaseToPos3D, Vector3.One * .005f, new Color(.2f, .2f, 1));
                }
                last = cur;
            }
        }
    }
    public void UpdateLines(double period)
    {

        foreach (var path in paths)
            UpdateLineMesh(path, period, offset);
    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.SliderFloat("slice t", ref sliceT, 0, 1);
        ImGuiHelpers.SliderFloat("offset", ref offset, 0, 1);
        ImGuiHelpers.SliderFloat("t", ref t, 0,30);
        base.DrawImGuiSettings();
    }
}