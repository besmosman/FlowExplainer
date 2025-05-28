using System.Numerics;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.ES20;
using Vector2 = System.Numerics.Vector2;

namespace FlowExplainer;

public struct Particle
{
    public Vector2 Position;
    public float Heat;
    public float tag;
    public float HeatFlux;
}

public class Utils
{
    public static T Lerp<T, TC>(T a, T b, TC c) where T : IMultiplyOperators<T, TC, T>, IAdditionOperators<T, T, T>
        where TC : INumber<TC>
    {
        return a * (TC.One - c) + b * c;
    }
}

public class Gradient<T> where T : IMultiplyOperators<T, float, T>, IAdditionOperators<T, T, T>
{
    private (float time, T value)[] entries;
    private T[] Cached;

    public Gradient((float, T)[] entries)
    {
        this.entries = entries;

        Cached = new T[255];
        for (int i = 0; i < 255; i++)
        {
            Cached[i] = Get(i / 255f);
        }
    }


    public T GetCached(float t)
    {
        return Cached[int.Clamp((int)float.Round(t * 255f), 0, 254)];
    }

    public T Get(float t)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Item1 <= t && entries[i + 1].Item1 >= t)
            {
                var lt = (t - entries[i].time) / (entries[i + 1].time - entries[i].time);
                var pre = entries[i].Item2;
                var next = entries[i + 1].Item2;
                return Utils.Lerp(pre, next, lt);
            }
        }

        throw new Exception();
    }
}

public class TestService : VisualisationService
{
    public override void Initialize()
    {
        sph.Setup();
    }

    public float time = 0;

    private Material material = Material.NewDefaultUnlit;

    private Sph sph = new Sph();

    public override void Draw(RenderTexture rendertarget, View view)
    {
        view.Camera2D.Scale = 1500;
        view.Camera2D.Position = -new Vector2(.5f, .25f);

        float dt = (float)FlowExplainer.DeltaTime.TotalSeconds / 4;
        var velocityField = new SpeetjensVelocityField();


        time += dt;
        sph.Update(velocityField, time, dt);

        //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

        for (int x = 0; x <= 20; x++)
        {
            for (int y = 0; y <= 10; y++)
            {
                // var line = StreamLineGenerator.Generate(integrator, velocityField, new Vector2(x / 10f, y / 10f), time, time + 0.5f, 64);

                //Gizmos2D.StreamTube(view.Camera2D, line.points, new Vector4(0, 1, 0, 1));
            }
        }

        var gradient = new Gradient<Color>([
            (0.00f, new(0, 0, .5f)),
            (0.1f, new(0, 0, 1f)),
            (0.4f, new(0, 1, 1)),
            (0.6f, new(1, 1, 0)),
            (0.9f, new(1f, 0, 0f)),
            (1.00f, new(.5f, 0, 0f)),
        ]);
        var camera = view.Camera2D;
        material.Use();
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        foreach (ref var p in sph.Particles.AsSpan())
        {
           // var color = gradient.GetCached(float.Clamp(p.HeatFlux*-400+.5f, 0,1));
           //var color = gradient.GetCached(float.Clamp(p.Heat, 0,1));
           var color = gradient.GetCached(float.Clamp(p.tag, 0,1));

            var center = p.Position;
            var radius = .004f;
            material.SetUniform("tint", color);
            material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(center.X, center.Y, 0));
            Gizmos2D.circleMesh.Draw();
        }

        var a = sph.Particles[40].Position;
        var b = sph.Particles[30].Position;
        var c = sph.Particles[94].Position;
        var col = new Vector4(1, 0, 1, 1);
        var th = .007f;
        var area = MathF.Abs(0.5f * ((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)));
       // Gizmos2D.Line(view.Camera2D, a,b,col,th );
       // Gizmos2D.Line(view.Camera2D, b,c,col,th );
       // Gizmos2D.Line(view.Camera2D, a,c,col,th );
       // Gizmos2D.Text(view.Camera2D, (a+b+c)/3, .08f,  new Vector4(1f, 0f, 1f, 1), float.Round(area*1000,1).ToString(), centered:true);


        /*float dt = (float)FlowExplainer.DeltaTime.TotalSeconds * 0.3f;
        Parallel.ForEach(particles, p => { p.Position = RungeKutta4Integrator.Integrate((f, p) => Velocity(p, f), p.Position, time, dt); });
        /*
        for (int i = 0; i < particles.Count; i++)
        {
            var p = particles[i];
            particles[i].Position = ;
        }
        #1#

        var camera = view.Camera2D;
        material.Use();
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        foreach (var p in particles)
        {
            var center = p.Position;
            var radius = .003f;
            var color = new Vector4(p.StartPos.X / 2, 0, 1f - p.StartPos.X / 2, 1f);
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
            #1#


            row++;
            for (float x = cellSize / 2; x < 2f; x += cellSize)
            {
                var pos = new Vector2(x, y);
                var vel = Velocity(pos, time) * .004f;

                var hue = vel.X - vel.Y * 5000;

                var col = new Vector4(Abs(vel.X * 100), Abs(vel.Y * 100), vel.Length() * 10, 1);
                //Gizmos2D.Circle(view.Camera2D, pos, col, .003f);
                //Gizmos2D.Circle(view.Camera2D, pos + vel, col, .004f);
                var v = (streamFunction(x, y, time) + 1) / 3;
                if (Math.Abs(v - time % 2f) < .01f)
                {
                    Gizmos2D.RectCenter(view.Camera2D, pos, new Vector2(cellSize), new Vector4(0, v, v, 1));
                }

                Gizmos2D.Line(view.Camera2D, pos, pos + vel, col, .006f);
            }
        }*/

        // Gizmos2D.RectCenter(view.Camera2D, new Vector2(1f, .5f), new Vector2(2, 1), new Vector4(0, 1, 1, 1));
    }
    public static float PolygonArea(IList<Vector2> vertices)
    {
        int n = vertices.Count;
        if (n < 3) return 0f;

        float area = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 current = vertices[i];
            Vector2 next = vertices[(i + 1) % n];
            area += (current.X * next.Y) - (next.X * current.Y);
        }
        return 0.5f * MathF.Abs(area);
    }
    public override bool HasImGuiEditElements { get; }
}