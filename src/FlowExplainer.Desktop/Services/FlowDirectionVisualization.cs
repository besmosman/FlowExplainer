using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class FlowDirectionVisualization : WorldService, IAxisTitle
{
    private int posPer = 32;
    public int amount = 1600;
    public double thickness = .003f;
    public double speed = 3;
    private Vec2[] centers;

    public double opacity = .21f;
    double end = 2;

    public double dt = .001f;
    public double lastSimTime = -1f;
    private double avgSpeed = 0.0;

    public IVectorField<Vec3, Vec2>? AltVectorField;
    public double? AltTime;

    public override string? Name => "Animated Glyph Flow Visualizer";
    public override string? CategoryN => "Vectorfield";
    public override string? Description => "Visualize flow with glyphs that move along the instantaneous vectorfield.";

    struct Data
    {
        public double TimeAlive;
    }

    public override void DrawImGuiSettings()
    {
        ImGui.SliderInt("Amount", ref amount, 100, 10_000);
        ImGuiHelpers.SliderFloat("Thickness", ref thickness, 0, .01f);
        ImGuiHelpers.SliderFloat("Speed", ref speed, 0, 10);
        ImGuiHelpers.SliderFloat("Opacity", ref opacity, 0, 1);
        base.DrawImGuiSettings();
    }

    public override void DrawImGuiDataSettings()
    {
        var dat = GetRequiredWorldService<DataService>();
        var field = AltVectorField ?? dat.VectorField;
        //ImGui.SameLine();
        ImGuiHelpers.OptionalDoubleSlider("Alt time", ref AltTime, field.Domain.RectBoundary.Min.Last, field.Domain.RectBoundary.Max.Last);
        ImGuiHelpers.OptonalVectorFieldSelector(GetRequiredWorldService<DataService>().LoadedDataset, ref AltVectorField);
        base.DrawImGuiDataSettings();
    }


    private Data[] PerData;
    public override void Initialize()
    {
        Init();
        var tubeVerts = new List<Vertex>();
        var indicies = new List<uint>();
        int segments = posPer;
        for (uint i = 0; i < segments; i++)
        {
            tubeVerts.Add(new Vertex(new Vec3(i / (double)segments, -1f, 0)));
            tubeVerts.Add(new Vertex(new Vec3(i / (double)segments, 1f, 0)));
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
    private void Init()
    {

        PerData = new Data[amount];
        centers = new Vec2[amount * posPer];
        var dat = GetRequiredWorldService<DataService>();
        var velField = dat.VectorField;
        for (int i = 0; i < amount; i++)
        {
            var span = centers.AsSpan(i * posPer, posPer);
            var pos = velField.Domain.RectBoundary.Reduce<Vec2>().FromRelative(new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()));

            span.Fill(pos);
            PerData[i].TimeAlive = Random.Shared.NextSingle() * 5;
        }
    }


    public override void Update()
    {
        var dat = GetRequiredWorldService<DataService>();
        var velField = AltVectorField ?? dat.VectorField;
        double time = AltTime ?? dat.SimulationTime;
        var instantField = new InstantFieldVersionLowerDim<Vec3, Vec2, Vec2>(velField, time);
        var velMag = 0.0;
        if (time != lastSimTime)
        {
            for (int i = 0; i < amount; i++)
            {
                PerData[i].TimeAlive = -((double)i / amount) * 5;
                var pos = velField.Domain.RectBoundary.Reduce<Vec2>().FromRelative(new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                var span = centers.AsSpan(i * posPer, posPer);
                span.Fill(pos);
            }
        }
        lastSimTime = time;
        int c = 0;

        if (amount != PerData.Length)
            Init();


        for (int i = 0; i < amount; i++)
        {
            var span = centers.AsSpan(i * posPer, posPer);
            if (PerData[i].TimeAlive > end + 2f + ((i * 17 + i * 1535 + i) % 1000) / 1000f)
            {
                var pos = velField.Domain.RectBoundary.Reduce<Vec2>().FromRelative(new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                span.Fill(pos);
                PerData[i].TimeAlive = -Random.Shared.NextSingle() * 5;
            }

            if (PerData[i].TimeAlive > 0)
            {
                var lastPos = span[posPer - 1];
                var newPos = lastPos;
                if (instantField.TryEvaluate(lastPos, out var vel) && double.IsRealNumber(vel.X))
                {
                    velMag += vel.Length();
                    if (instantField.Domain.IsWithinPhase(lastPos))
                        newPos += vel * (speed / avgSpeed) * FlowExplainer.DeltaTime * .01f;
                }
                for (int j = 0; j < span.Length - 1; j++)
                {
                    span[j] = span[j + 1];
                }
                span[posPer - 1] = newPos;
                c++;
            }
            PerData[i].TimeAlive += FlowExplainer.DeltaTime;
        }
        avgSpeed = velMag / c;
        base.Update();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        if (amount != PerData.Length)
            Init();

        for (int i = 0; i < amount; i++)
        {
            var span = centers.AsSpan(i * posPer, posPer);

            //grid.ScaleScaler(dat.TempratureField.Evaluate(span[posPer - 1].Up(dat.SimulationTime)));
            var color = new Color(opacity, opacity, opacity);
            var a = 0.0;
            var t = PerData[i].TimeAlive;

            if (t < 1 && t > .0f)
                a = t * 1;
            if (t >= 1 && t < end)
                a = 1;
            if (t >= end)
                a = double.Max(0, 1f - ((t - end) * .5f));

            color.A = (float)a;

            if (color.A > 0)
            {

                double distanceSquared = Vec2.DistanceSquared(span[0], span[span.Length - 1]);
                if (distanceSquared < .00005f)
                {
                    //    color.A *= (float)distanceSquared / .00005f;
                }
                StreamTube(view.Camera2D, span, color, thickness);

                /*else
                    Gizmos2D.Circle(view.Camera2D, span[^1],color, .003f/2);*/
            }
            a = 0;
        }

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private static Mesh streamtube;
    private static Material material = Material.NewDefaultUnlit;

    public static void StreamTube(ICamera camera, Span<Vec2> centers, Color color, double thickness)
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
        var total = 0.0;
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

            double c = (i / (double)centers.Length);
            var length = thickness * c * (thickness - c * c);
            length = Math.Sqrt(1 - (c * 2 - 1) * (c * 2 - 1) * c) * c * thickness;
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
    public string GetTitle()
    {
        return "Animated Glyphs (" + (AltVectorField?.DisplayName ?? GetRequiredWorldService<DataService>().VectorField.DisplayName) + ")";
    }
}