using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class FlowDirectionVisualization : WorldService
{
    private int posPer = 64;
    private int amount = 1600;
    private Vec2[] centers;

    public float opacity = .18f;

    struct Data
    {
        public float TimeAlive;
    }

    public override ToolCategory Category => ToolCategory.Flow;
    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Opacity", ref opacity, 0, 1);
        base.DrawImGuiEdit();
    }


    private Data[] PerData;
    public override void Initialize()
    {
        PerData = new Data[amount];
        centers = new Vec2[amount * posPer];
        var dat = GetRequiredWorldService<DataService>();
        var velField = dat.VelocityField;
        for (int i = 0; i < amount; i++)
        {
            var span = centers.AsSpan(i * posPer, posPer);
            var pos = velField.Domain.Boundary.Reduce<Vec2>().Relative(new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()));

            span.Fill(pos);
            PerData[i].TimeAlive = -Random.Shared.NextSingle() * 5;
        }
        var tubeVerts = new List<Vertex>();
        var indicies = new List<uint>();
        int segments = posPer;
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

    float end = 2;
    public float dt = .001f;

    public override void Update()
    {
        var dat = GetRequiredWorldService<DataService>();
        var velField = dat.VelocityField;
        var instantField = new InstantFieldVersion<Vec3, Vec2, Vec2>(velField, dat.SimulationTime);
        for (int i = 0; i < amount; i++)
        {
            var span = centers.AsSpan(i * posPer, posPer);
            if (PerData[i].TimeAlive > end + 2f + ((i*17 + i*1535 + i) % 1000)/1000f )
            {
                var pos = velField.Domain.Boundary.Reduce<Vec2>().Relative(new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                span.Fill(pos);
                PerData[i].TimeAlive = -Random.Shared.NextSingle();
            }

            var lastPos = span[posPer - 1];
            var newPos = lastPos;
            if (instantField.TryEvaluate(lastPos, out var vel))
            {
                if (instantField.Domain.IsWithinPhase(lastPos))
                    newPos += vel * dt * 30;
            }
            for (int j = 0; j < span.Length - 1; j++)
            {
                span[j] = span[j + 1];
            }
            span[posPer - 1] = newPos;
            PerData[i].TimeAlive += FlowExplainer.DeltaTime;
        }
        base.Update();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        var grid = GetRequiredWorldService<GridVisualizer>();
        for (int i = 0; i < amount; i++)
        {
            var span = centers.AsSpan(i * posPer, posPer);

            //grid.ScaleScaler(dat.TempratureField.Evaluate(span[posPer - 1].Up(dat.SimulationTime)));
            var color = new Color(opacity, opacity,opacity);
            var a = 0f;
            var t = PerData[i].TimeAlive;

            if (t < 1 && t > .0f)
                a = t * 1;
            if (t >= 1 && t < end)
                a = 1;
            if (t >= end)
                a = float.Max(0, 1f - ((t - end) * .5f));

            color.A = a;

            if (color.A > 0)
            {

                float distanceSquared = Vec2.DistanceSquared(span[0], span[span.Length - 1]);
                if (distanceSquared < .00005f)
                {
                    color.A *= distanceSquared / .00005f;
                }
                    StreamTube(view.Camera2D, span, color, .003f);
                
                /*else
                    Gizmos2D.Circle(view.Camera2D, span[^1],color, .003f/2);*/
            }
            a = 0;
        }

          GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private static Mesh streamtube;
    private static Material material = Material.NewDefaultUnlit;

    public static void StreamTube(ICamera camera, Span<Vec2> centers, Color color, float thickness)
    {

        /*foreach (var center in centers)
        {
            Gizmos2D.Circle(camera, center,Color.White, .001f);
        }
        return;*/
        if (centers.Length != streamtube.Vertices.Length / 2)
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
        for (int i = 1; i < centers.Length; i++)
        {
            total += Vec2.Distance(centers[i], centers[i - 1]);
        }

        for (int i = 0; i < centers.Length; i++)
        {
            var dir = Vec2.Zero;
            if (i != 0)
                dir = Vec2.Normalize(centers[i] - centers[i - 1]);
            var normal = new Vec2(dir.Y, -dir.X);

            float c = (i / (float)centers.Length);
            var length = thickness * c * (thickness - c * c);
            length = MathF.Sqrt(1 - (c * 2 - 1) * (c * 2 - 1) * c) * c * thickness;
            streamtube.Vertices[i * 2 + 0].Position = new Vec3(centers[i] - normal * length, 0);
            streamtube.Vertices[i * 2 + 0].Colour.Y = 1;
            streamtube.Vertices[i * 2 + 1].Position = new Vec3(centers[i] + normal * length, 0);
            streamtube.Vertices[i * 2 + 1].Colour.W = 1;
        }

        streamtube.Upload(UploadFlags.Vertices);
        material.SetUniform("model", Matrix4x4.Identity);
        streamtube.Draw();
        //  Gizmos2D.Circle(camera, centers.Last(), color, 1f);
    }
}