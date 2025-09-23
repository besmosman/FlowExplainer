using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class ParticleLagrangianTest : WorldService
{

    public class Entry
    {
        public Vec2 StartPos;
        public float ValueScaled;
        public Trajectory<Vec3> Trajectory;
        public Trajectory<Vec2> TrajectoryValues;
        public Vec2 timeRange;
        public float ValueAt;
    }

    public Entry[] Entries;
    public float T = 950;
    public float DisplayT;
    public int amount = 1000;
    public float radius = .1f;
    public override ToolCategory Category => ToolCategory.Flow;
    public override void Initialize()
    {
        Reset();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        var grad = GetRequiredWorldService<DataService>().ColorGradient;
        var bounds = GetRequiredWorldService<DataService>().VectorField.Domain.Boundary;
        var f = DisplayT;
        //for (float f = .99f; f <= 1f; f += .05f)
        {
            float max = float.NegativeInfinity;
            float min = float.PositiveInfinity;


            foreach (var entry in Entries)
            {
                float t = Utils.Lerp(entry.Trajectory.Entries[0].Z, entry.Trajectory.Entries[^1].Z, f);
                entry.ValueAt = entry.TrajectoryValues.AtTime(t).X;
            }

            foreach (var entry in Entries)
            {
                max = float.Max(entry.ValueAt, max);
                min = float.Min(entry.ValueAt, min);
            }

            foreach (var entry in Entries)
                entry.ValueScaled = (entry.ValueAt - min) / (max - min);



            foreach (var entry in Entries)
            {
                float t = Utils.Lerp(entry.Trajectory.Entries[0].Z, entry.Trajectory.Entries[^1].Z, 0f);
                var renderPos = entry.Trajectory.AtTime(t);
                var col = grad.GetCached(entry.ValueScaled);
                col.A = 1f;
                Gizmos2D.Instanced.RegisterCircle(renderPos.XY, radius * bounds.Size.X/100f, col);
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

    }

    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderInt("Amount", ref amount, 1, 100000);
        ImGuiHelpers.SliderFloat("T", ref T, 0, 1000);
        ImGuiHelpers.SliderFloat("Radius", ref radius, 0, 1f);
        ImGuiHelpers.SliderFloat("DisplayT", ref DisplayT, 0, 1);
        if (ImGui.Button("Reset"))
        {
            Reset();
        }
        base.DrawImGuiEdit();
    }

    private void Reset()
    {
        Entries = new Entry[amount];
        var dat = GetWorldService<DataService>()!;
        var datVectorField = dat.VectorField;
        var datVectorFieldBack = new ArbitraryField<Vec3, Vec2>(dat.VectorField.Domain, x => dat.VectorField.Evaluate(x));
        var spatialbounds = datVectorField.Domain.Boundary.Reduce<Vec2>();

        spatialbounds.Min -= new Vec2(1.6f, 0);
        spatialbounds.Max += new Vec2(1.6f, 0);
        //for (int i = 0; i < Entries.Length; i++)


        float F(Vec3 x, Vec3 last, Vec3 cur)
        {
            if (cur.Z == x.Z)
                return 0;
            //float ftle = FTLEComputer.Compute(x.XY, x.Z, cur.Z, dat.VectorField, new Vec2(1f / 400f));
            //return ftle;
            return (cur - last).Down().Length();
           // return float.Sin((cur.X - cur.Y * 3) / 1);
        }

        Parallel.For(0, Entries.Length, i =>
        {
            var pos = Utils.Random(spatialbounds);
            var traj = IFlowOperator<Vec2, Vec3>.Default.Compute(dat.SimulationTime, dat.SimulationTime + T, pos, datVectorField);
            var trajBack = IFlowOperator<Vec2, Vec3>.Default.Compute(dat.SimulationTime, dat.SimulationTime + T, pos, datVectorFieldBack);
            var final = traj.Entries.Last();
            var rel = spatialbounds.Relative(final.Down());
            var rel2 = spatialbounds.Relative(trajBack.Entries.Last().Down());
            //var value = float.Sin((rel.X - rel.Y * 90) / 550);
            //var value = traj.Entries.Sum(f => float.Sin((f.X - f.Y * 90) / 250));
            //value += float.Sin((rel2.X - rel2.Y * 90) / 550);

            // value = FTLEComputer.Compute(pos, dat.SimulationTime, dat.SimulationTime + T, dat.VectorField, new Vec2(1f / 40f));
            //value = traj.AverageAlong((p, c) => (c - p).Down().Length());
            //value += trajBack.AverageAlong((p, c) => (c - p).Down().Length());

            Entries[i] = new Entry()
            {
                StartPos = pos,
                Trajectory = traj,
                TrajectoryValues = traj.Select((l, s) => new Vec2(F(pos.Up(dat.SimulationTime), l, s), s.Z)),
            };
        });
    }
}