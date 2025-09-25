using System.Drawing.Printing;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class ParticleLagrangianTest : WorldService
{

    public class Entry
    {
        public Vec2 StartPos;
        public float ValueScaled;
        public Vec2 renderpos;
        public Trajectory<Vec3> Trajectory;
        public Trajectory<Vec2> TrajectoryValues;
        public Vec2 timeRange;
        public float ValueAt;
    }

    public Entry[] Entries;
    public float T = 3;
    public float DisplayT;
    public int amount = 1000;
    public float radius = .1f;

    public PositionEnum positionEnum;
    public ValueEnum valueEnum;
    public bool useGradient;

    public enum PositionEnum
    {
        Start,
        End,
        t,
    }

    public enum ValueEnum
    {
        CellStart,
        CellEnd,
        Avg,
        t,
        AvgTillT,
    }

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

            float t = Utils.Lerp(Entries[0].Trajectory.Entries[0].Z, Entries[0].Trajectory.Entries[^1].Z, f);
            switch (valueEnum)
            {

                case ValueEnum.CellStart:
                    foreach (var entry in Entries)
                        entry.ValueAt = entry.TrajectoryValues.Entries[0].X;
                    break;
                case ValueEnum.CellEnd:
                    foreach (var entry in Entries)
                        entry.ValueAt = entry.TrajectoryValues.Entries[^1].X;
                    break;
                case ValueEnum.t:
                    foreach (var entry in Entries)
                        entry.ValueAt = entry.TrajectoryValues.AtTime(t).X;
                    break;
                case ValueEnum.AvgTillT:
                    foreach (var entry in Entries)
                    {
                        int c = 0;
                        float avg = 0f;
                        foreach (var v in entry.TrajectoryValues.Entries)
                        {
                            if (v.Last < t)
                            {
                                avg += v.X;
                                c++;
                            }
                        }
                        entry.ValueAt = avg / c;
                    }
                    break;
                case ValueEnum.Avg:
                    foreach (var entry in Entries)
                    {
                        int c = 0;
                        float avg = 0f;
                        foreach (var v in entry.TrajectoryValues.Entries)
                        {
                            avg += v.X;
                            c++;
                        }
                        entry.ValueAt = avg / c;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            /*foreach (var entry in Entries)
            {
                float t = Utils.Lerp(entry.Trajectory.Entries[0].Z, entry.Trajectory.Entries[^1].Z, f);
                float avg = 0f;
                foreach (var v in entry.TrajectoryValues.Entries)
                {
                    if (v.Last < t)
                    {
                        avg += v.X;
                    }
                }
                entry.ValueAt = entry.TrajectoryValues.AtTime(t).X;
                entry.ValueAt = avg;
                // entry.ValueAt = entry.TrajectoryValues.Entries.Select(s => s.X).Average();
            }*/

            foreach (var entry in Entries)
            {
                max = float.Max(entry.ValueAt, max);
                min = float.Min(entry.ValueAt, min);
            }

            foreach (var entry in Entries)
                entry.ValueScaled = (entry.ValueAt - min) / (max - min);

            switch (positionEnum)
            {
                case PositionEnum.Start:
                    foreach (var entry in Entries)
                        entry.renderpos = entry.Trajectory.Entries[0].XY;
                    break;
                case PositionEnum.End:
                    foreach (var entry in Entries)
                        entry.renderpos = entry.Trajectory.Entries[^1].XY;
                    break;
                case PositionEnum.t:
                    foreach (var entry in Entries)
                        entry.renderpos = entry.Trajectory.AtTime(t).XY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var entry in Entries)
            {
                var col = grad.GetCached(entry.ValueScaled);
                if (entry.renderpos.X > 0 && entry.renderpos.X < 1f)
                    Gizmos2D.Instanced.RegisterCircle(entry.renderpos, radius * bounds.Size.X / 100f, col);
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var title = "Trajectory Length";
        if (useGradient)
            title = "Gradient";
        title += $" (Position={Enum.GetName(positionEnum)}, Value={Enum.GetName(valueEnum)})";
        var domain = GetRequiredWorldService<DataService>().VectorField.Domain.Boundary;
        var lb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Min.Y));
        var rb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Max.X, domain.Min.Y));
        var lt = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Max.Y));
        var lh = view.Width / 26f;
        Gizmos2D.Text(view.ScreenCamera, new Vec2((lb.X + rb.X) / 2, lt.Y - lh * 2), lh, Color.White, title, centered: true);

    }

    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderInt("Amount", ref amount, 1, 100000);
        ImGuiHelpers.SliderFloat("T", ref T, 0, 10);
        ImGuiHelpers.SliderFloat("Radius", ref radius, 0, 1f);
        ImGuiHelpers.SliderFloat("t", ref DisplayT, 0, 1);
        ImGuiHelpers.Combo("Position", ref positionEnum);
        ImGuiHelpers.Combo("Value", ref valueEnum);
        ImGui.Checkbox("UseGradient", ref useGradient);
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
            // float ftle = FTLEComputer.Compute(x.XY, x.Z, cur.Z, dat.VectorField, new Vec2(1f / 400f));
            // return ftle;

            if (useGradient)
                return float.Sin(((cur.X) * 4) / 1);

            return (cur - last).Down().Length();

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
            for (int j = 0; j < Entries[i].TrajectoryValues.Entries.Length; j++)
            {
                //Entries[i].TrajectoryValues.Entries[j] = Entries[i].TrajectoryValues.Entries[5];
            }
        });
    }
}