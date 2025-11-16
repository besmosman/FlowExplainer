using ImGuiNET;

namespace FlowExplainer;

public class UFLIC : IGridDiagnostic
{
    private RegularGrid<Vec3i, Vec2> Accumelation = new(new Vec3i(1, 1, 1));


    private IVectorField<Vec2, double> NoiseField = new NoiseField();
    private RegularGrid<Vec2i, double> InputTexture = new(Vec2i.One);
    private RegularGrid<Vec2i, double> InputTextureCopy = new(Vec2i.One);
    int globalStep = 0;
    public double dt = .004f;
    public double expected_lifetime = .06f;
    FastNoise noise = new FastNoise();

    public bool auto = false;
    public double startTime = 0;
    public int view_offset;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var t = gridVisualizer.GetRequiredWorldService<DataService>().SimulationTime;

        int max_steps = (int)double.Ceiling(expected_lifetime / dt) + 1;
        if (Accumelation.GridSize.XY != gridVisualizer.RegularGrid.GridSize || max_steps != Accumelation.GridSize.Last
                                                                            || (auto && t < globalStep * dt + startTime))
        {
            Accumelation.Resize(new Vec3i(gridVisualizer.RegularGrid.GridSize, max_steps));
            InputTexture.Resize(gridVisualizer.RegularGrid.GridSize);
            InputTextureCopy.Resize(gridVisualizer.RegularGrid.GridSize);
            globalStep = 0;
            var domainBoundary = gridVisualizer.RegularGrid.Domain.RectBoundary;
            ParallelGrid.For(InputTexture.GridSize, token, (i, j) => { InputTexture.AtCoords(new Vec2i(i, j)) = NoiseField.Evaluate(domainBoundary.Relative(new Vec2(i, j) / gridVisualizer.RegularGrid.GridSize.ToVec2())); });
            startTime = t;
            if (auto)
                warm(gridVisualizer, max_steps);
        }


        if (auto)
        {
            Step(gridVisualizer, token);
            while ((globalStep + 1) * dt + startTime < t)
            {
            }
        }

        var outputGrid = gridVisualizer.RegularGrid;
        ParallelGrid.For(outputGrid.GridSize, token, (i, j) =>
        {
            var cell_z = (globalStep + view_offset) % max_steps;
            ref var atpos = ref Accumelation.AtCoords(new Vec3i(i, j, cell_z));
            var value = atpos.X / atpos.Y;
            outputGrid.Grid.AtCoords(new Vec2i(i, j)).Value = value;
        });
    }

    private void Step(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var ft = gridVisualizer.GetRequiredWorldService<DataService>().SimulationTime;

        int max_steps = (int)double.Ceiling(expected_lifetime / dt) + 1;
        double start_t = gridVisualizer.GetRequiredWorldService<DataService>().SimulationTime;
        var vectorField = gridVisualizer.GetRequiredWorldService<DataService>().VectorField;
        var domainBoundary = vectorField.Domain.RectBoundary.Reduce<Vec2>();
        var outputGrid = gridVisualizer.RegularGrid;

        for (int i = 0; i < outputGrid.GridSize.X; i++)
        for (int j = 0; j < outputGrid.GridSize.Y; j++)
        {
            var pos = domainBoundary.Relative(new Vec2(i + .5f, j + .5f) / outputGrid.GridSize.ToVec2());
            var scatterValue = InputTexture.AtCoords(new Vec2i(i, j));

            double t = start_t + globalStep * dt;

            var phase = pos.Up(t);
            var lastBucket = new Vec3i(outputGrid.ToVoxelCoord(phase.XY).FloorInt(), globalStep % max_steps);
            for (int k = 0; k < max_steps; k++)
            {
                t += dt;
                var last = phase;
                phase = IIntegrator<Vec3, Vec2>.Rk4.Integrate(vectorField, phase, dt)
                    .Up(t);
                if (phase.X > domainBoundary.Max.X)
                {
                    phase.X -= domainBoundary.Size.X;
                    lastBucket = new Vec3i(outputGrid.ToVoxelCoord(phase.XY).FloorInt(), globalStep % max_steps);
                }

                if (phase.X < domainBoundary.Min.X)
                {
                    phase.X += domainBoundary.Size.X;
                    lastBucket = new Vec3i(outputGrid.ToVoxelCoord(phase.XY).FloorInt(), globalStep % max_steps);
                }

                if (domainBoundary.Contains(phase.XY))
                {
                    var cell_xy = outputGrid.ToVoxelCoord(phase.XY).FloorInt();
                    var cell_z = (k + globalStep) % max_steps;
                    var end = new Vec3i(cell_xy, cell_z);

                    foreach (var xy in DDA.Line(lastBucket.XY, end.XY))
                    {
                        var bucket = new Vec3i(xy, (k + globalStep) % max_steps);
                        var w = (phase.XY - last.XY).Length();
                        var age = phase.Last - start_t;
                        var age_factor = 1f - (age / expected_lifetime);
                        var W = w * age_factor;

                        var I_normalized = scatterValue * W;
                        ref var atpos = ref Accumelation.AtCoords(bucket);
                        atpos += new Vec2(I_normalized, W);
                        lastBucket = bucket;
                    }
                }
            }
        }

        globalStep++;
        ParallelGrid.For(InputTexture.GridSize, token, (i, j) =>
        {
            ref var atpos = ref Accumelation.AtCoords(new Vec3i(i, j, globalStep % max_steps));
            var value = atpos.X / atpos.Y;
            InputTexture.AtCoords(new Vec2i(i, j)) = value;
            if (atpos.Y == 0.0)
                InputTexture.AtCoords(new Vec2i(i, j)) = 0;
            if (globalStep != 0)
                Accumelation.AtCoords(new Vec3i(i, j, (globalStep - 1) % max_steps)) = Vec2.Zero;
        });


        for (int i = 0; i < 1; i++)
        {
            Array.Copy(InputTexture.Data, InputTextureCopy.Data, InputTexture.Data.Length);
            ParallelGrid.For(outputGrid.GridSize, token, (i, j) =>
            {
                double nextValue = 0.0;
                double totWeight = 0.0;
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    double weight = 1;
                    var xi = i + x;
                    var yj = j + y;
                    if (x == 0 && y == 0)
                        weight = -8;

                    if (InputTextureCopy.Contains(new Vec2i(xi, yj)))
                    {
                        nextValue += weight * InputTextureCopy.AtCoords(new Vec2i(xi, yj));
                        totWeight += weight;
                    }
                }

                ref var atPos = ref InputTexture.AtCoords(new Vec2i(i, j));
                // Random.Shared.NextSingle() - .5f) * .05f
                atPos = double.Clamp(atPos - (nextValue / 20f) + ((NoiseField.Evaluate(new Vec2(i, j) / outputGrid.GridSize.ToVec2())) - .5f) * 4, 0, 1);
                //   atPos = Random.Shared.NextSingle();
            });
        }
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        int max_steps = (int)double.Ceiling(expected_lifetime / dt) + 1;

        ImGuiHelpers.SliderFloat("dt", ref dt, 0, .1f);
        ImGuiHelpers.SliderFloat("expected_lifetime", ref expected_lifetime, 0, .4f);

        if (ImGui.Button("Step"))
        {
            Step(gridVisualizer, CancellationToken.None);
        }

        if (ImGui.Button("Warm"))
        {
            warm(gridVisualizer, max_steps);
        }

        ImGuiHelpers.SliderInt("view_offset", ref view_offset, 0, max_steps - 1);
    }

    private void warm(GridVisualizer gridVisualizer, int max_steps)
    {
        for (int i = 0; i < max_steps; i++)
        {
            Step(gridVisualizer, default);
        }
    }
}