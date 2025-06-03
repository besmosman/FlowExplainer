using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public interface IAddDimension<TIn, TOut>
{
    TOut Up(float f);
}

public interface IVec
{
    public float Get(int i);
}

public struct Vec1
{
    public float X;
}

/*public class AverageAlongTrajectory<TInput, TOutput>
{
    public void Calc(IVectorField<TInput, TOutput> v, IIntegrator<TInput, TOutput> integrator, TInput x_start, TInput x_end, int steps)
    {
        integrator.Integrate(v.Evaluate, x_start, dt);
    }
}*/


/*
public class Test1Service : WorldService
{
    public override ToolCategory Category => ToolCategory.Simulation;

    public float time;
    public float timeMultiplier = 1;
    private Material material = Material.NewDefaultUnlit;
    private Sph sph = new Sph();

    public override void DrawImGuiEdit()
    {
        if (ImGui.Button("Reset"))
        {
            sph = new();
            time = 0;
            sph.Setup();
            timeMultiplier = 1;
        }

        ImGui.SliderFloat("Time Multiplier", ref timeMultiplier, .1f, 10);

        base.DrawImGuiEdit();
    }

    public override void Initialize()
    {
        sph.Setup();
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Position = new Vec2(i / (float)particles.Length, 1 / 4f);
            particles[i].points = new List<Vec2>();
        }
    }

    private Particle[] particles = new Particle[64];
    private float pt = 0f;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        view.Camera2D.Scale = 1500;
        view.Camera2D.Position = -new Vec2(.5f, .25f);

        float dt = (float)FlowExplainer.DeltaTime.TotalSeconds / 10f;
        var velocityField = new SpeetjensAdaptedVelocityField();

        time += dt;
        sph.Update(velocityField, time, dt);

        //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        var integrator = new RungeKutta4Integrator();
        for (int x = 0; x <= 30; x++)
        {
            for (int y = 0; y <= 20; y++)
            {
                var line = StreamLineGenerator.Generate(velocityField, integrator, new Vec2(x / 15f, y / 20f), time, 0.06f, 64);
                // Gizmos2D.StreamTube(view.Camera2D, line.points, new Vec4(0, .6f, 1, 1));
            }
        }

        var gradient = new Gradient<Color>([
            (0.00f, new(0, 0, .4f)),
            (0.02f, new(0, 0, 1f)),
            (0.4f, new(0, 1f, 1)),
            (0.6f, new(1, 1, 0)),
            (0.7f, new(1f, .4f, 0f)),
            (0.9f, new(1f, 0, 0f)),
            (1.00f, new(.4f, 0, 0f)),
        ]);
        var camera = view.Camera2D;
        material.Use();
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        foreach (ref var p in sph.Particles.AsSpan())
        {
            //var color = gradient.Get(float.Clamp(p.DiffusionHeatFlux / dt * -.7f + .5f, 0, 1));
            var color = gradient.Get(float.Clamp(p.Heat, 0, 1));
            //var color = gradient.Get(float.Clamp(p.tag, 0,1));

            var center = p.Position;
            var radius = .002f;
            material.SetUniform("tint", color);
            material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(center.X, center.Y, 0));
            Gizmos2D.circleMesh.Draw();
        }

        var col = new Vec4(1, 1, 1, 1/1f);
        material.SetUniform("tint", col);
        pt += dt;
        foreach (ref var p in particles.AsSpan())
        {
           // p.points.Add(p.Position);
           for (int i = 0; i < 400; i++)
           {
            p.Position = integrator.Integrate(velocityField.Evaluate, p.Position.Up(time), (1/4f) / 400f);
           }
            var radius = .002f;
            var center = p.Position;

          //  if (pt > .1f)
            {
                material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(center.X, center.Y, 0));
                Gizmos2D.circleMesh.Draw();
            }
            /*foreach (var pos in p.points)
            {
                var center = pos;
            }#1#
        }
        
        /*var a = sph.Particles[40].Position;
        var b = sph.Particles[30].Position;
        var c = sph.Particles[94].Position;
        var col = new Vec4(1, 0, 1, 1);
        var th = .007f;
        var area = MathF.Abs(0.5f * ((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)));#1#
        // Gizmos2D.Line(view.Camera2D, a,b,col,th );
        // Gizmos2D.Line(view.Camera2D, b,c,col,th );
        // Gizmos2D.Line(view.Camera2D, a,c,col,th );
        // Gizmos2D.Text(view.Camera2D, (a+b+c)/3, .08f,  new Vec4(1f, 0f, 1f, 1), float.Round(area*1000,1).ToString(), centered:true);


        /*float dt = (float)FlowExplainer.DeltaTime.TotalSeconds * 0.3f;
        Parallel.ForEach(particles, p => { p.Position = RungeKutta4Integrator.Integrate((f, p) => Velocity(p, f), p.Position, time, dt); });
        /*
        for (int i = 0; i < particles.Count; i++)
        {
            var p = particles[i];
            particles[i].Position = ;
        }
        #2#

        var camera = view.Camera2D;
        material.Use();
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        foreach (var p in particles)
        {
            var center = p.Position;
            var radius = .003f;
            var color = new Vec4(p.StartPos.X / 2, 0, 1f - p.StartPos.X / 2, 1f);
            material.SetUniform("tint", color);
            material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(center.X, center.Y, 0));
            Gizmos2D.circleMesh.Draw();
        }

        time += dt;
        int row = 0;
        float cellSize = .01f;
        for (float y = cellSize / 2; y < 1f; y += cellSize)
        {
            /*
            var start = 0cellSize;
            if (row % 2 == 1)
            {
                start = 0.04f;
            }
            #2#


            row++;
            for (float x = cellSize / 2; x < 2f; x += cellSize)
            {
                var pos = new Vec2(x, y);
                var vel = Velocity(pos, time) * .004f;

                var hue = vel.X - vel.Y * 5000;

                var col = new Vec4(Abs(vel.X * 100), Abs(vel.Y * 100), vel.Length() * 10, 1);
                //Gizmos2D.Circle(view.Camera2D, pos, col, .003f);
                //Gizmos2D.Circle(view.Camera2D, pos + vel, col, .004f);
                var v = (streamFunction(x, y, time) + 1) / 3;
                if (Math.Abs(v - time % 2f) < .01f)
                {
                    Gizmos2D.RectCenter(view.Camera2D, pos, new Vec2(cellSize), new Vec4(0, v, v, 1));
                }

                Gizmos2D.Line(view.Camera2D, pos, pos + vel, col, .006f);
            }
        }#1#

        // Gizmos2D.RectCenter(view.Camera2D, new Vec2(1f, .5f), new Vec2(2, 1), new Vec4(0, 1, 1, 1));
    }

    public static float PolygonArea(IList<Vec2> vertices)
    {
        int n = vertices.Count;
        if (n < 3) return 0f;

        float area = 0f;
        for (int i = 0; i < n; i++)
        {
            Vec2 current = vertices[i];
            Vec2 next = vertices[(i + 1) % n];
            area += (current.X * next.Y) - (next.X * current.Y);
        }

        return 0.5f * MathF.Abs(area);
    }
}*/