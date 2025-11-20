using System.Collections.Concurrent;

namespace FlowExplainer;

public class NathanKutz : WorldService
{
    Vec2i N = new Vec2i(10, 10);
    RegularGrid<Vec2i, float> LastVoricity = null!;
    private RegularGrid<Vec2i, float> Voricity = null!;
    float dt = .01f;

    public override void Initialize()
    {
        LastVoricity = new RegularGrid<Vec2i, float>(N);
        Voricity = new RegularGrid<Vec2i, float>(N);
        for (int i = 0; i < N.X; i++)
        for (int j = 0; j < N.Y; j++)
        {
            Voricity[new Vec2i(i, j)] = float.Sin(i / (float)N.X + j / (float)N.Y);
        }

        float mean = Voricity.Data.Average();
        for (int i = 0; i < Voricity.Data.Length; i++)
            Voricity.Data[i] -= mean;

        Array.Copy(Voricity.Data, LastVoricity.Data, LastVoricity.Data.Length);
    }


    public void Test()
    {
        var h = N.X;
        //laplace van streamfunction = vorticity
        RegularGrid<Vec2i, float> StreamFunction = new RegularGrid<Vec2i, float>(N);
        for (int k = 0; k < 32; k++)
        {
            for (int i = 0; i < N.X; i++)
            for (int j = 0; j < N.Y; j++)
            {
                //  if (Random.Shared.NextSingle() > .5f)
                {
                    ref float center = ref wrapped(StreamFunction, i, j);
                    ref float left = ref wrapped(StreamFunction, i - 1, j);
                    ref float right = ref wrapped(StreamFunction, i + 1, j);
                    ref float up = ref wrapped(StreamFunction, i, j + 1);
                    ref float down = ref wrapped(StreamFunction, i, j - 1);
                    center = float.Lerp(center, (left + right + up + down - h * h * Voricity.AtCoords(new Vec2i(i, j))) / 4f, 1f);
                }
            }

            float error = 0f;
            for (int i = 0; i < N.X; i++)
            for (int j = 0; j < N.Y; j++)
            {
                ref float center = ref wrapped(StreamFunction, i, j);
                ref float left = ref wrapped(StreamFunction, i - 1, j);
                ref float right = ref wrapped(StreamFunction, i + 1, j);
                ref float up = ref wrapped(StreamFunction, i, j + 1);
                ref float down = ref wrapped(StreamFunction, i, j - 1);
                error += -4 * center + left + right + down + up - h * h * Voricity.AtCoords(new Vec2i(i, j));
            }

            int c = 5;
        }

        RegularGrid<Vec2i, float> StreamParts = new RegularGrid<Vec2i, float>(N);

        for (int i = 0; i < N.X; i++)
        for (int j = 0; j < N.Y; j++)
        {
            float streamdx = (wrapped(StreamFunction, i + 1, j) - wrapped(StreamFunction, i - 1, j)) / (2 * h);
            float streamdy = (wrapped(StreamFunction, i, j + 1) - wrapped(StreamFunction, i, j - 1)) / (2 * h);
            float vortdx = (wrapped(Voricity, i + 1, j) - wrapped(Voricity, i - 1, j)) / (2 * h);
            float vortdy = (wrapped(Voricity, i, j + 1) - wrapped(Voricity, i, j - 1)) / (2 * h);
            wrapped(StreamParts, i, j) = streamdx * vortdy - streamdy * vortdx;
        }

        for (int i = 0; i < N.X; i++)
        for (int j = 0; j < N.Y; j++)
        {
            var parts = StreamParts[new Vec2i(i, j)];
            var v = 1;
            var laplaceVorticity = wrapped(Voricity, i + 1, j) + wrapped(Voricity, i - 1, j) + wrapped(Voricity, i, j - 1) + wrapped(Voricity, i, j + 1) - 4 * wrapped(Voricity, i, j);
            wrapped(Voricity, i, j) = LastVoricity[new Vec2i(i, j)] + (v * laplaceVorticity - parts);
        }

        ref float wrapped(RegularGrid<Vec2i, float> grid, int i, int j)
        {
            if (i == -1)
                return ref grid.AtCoords(new Vec2i(grid.GridSize.X - 1, j));
            if (i == grid.GridSize.X)
                return ref grid.AtCoords(new Vec2i(0, j));
            if (j == -1)
                return ref grid.AtCoords(new Vec2i(i, grid.GridSize.Y - 1));
            if (j == grid.GridSize.Y)
                return ref grid.AtCoords(new Vec2i(i, 0));

            return ref grid.AtCoords(new Vec2i(i, j));
        }
    }


    public override void Draw(RenderTexture rendertarget, View view)
    {
        /*for (int i = 0; i < DensityGrid.Data.Length; i++)
        {
            var cell = DensityGrid.Data[i];
            var coords = DensityGrid.GetIndexCoords(i);
            var pos = ((coords.ToVecF() + new Vec2(.5f, .5f)) / GridSize.ToVecF()) * Domain.Size;
            float estimateHeat = EstimateHeat(pos);
            Gizmos2D.Instanced.RegisterRectCenterd(pos, 1f / GridSize.ToVecF() * Domain.Size, new Color(cell.Density * 4, 0, 1, .1f));
            foreach (var p in cell.Parcels)
            {
                Gizmos2D.Instanced.RegisterCircle(p.Position, .001f, Color.White);
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);*/
    }
}

public class PointSimulation : WorldService
{
    public const float ParcelValue = 1f / 90;

    class Cell
    {
        public float Density;
        public float Temp;
        public List<HeatParcel> Parcels = new();
        public ConcurrentBag<HeatParcel> NextParcels = new();
    }

    private RegularGrid<Vec2i, Cell> DensityGrid;

    class HeatParcel
    {
        public Vec2 Position;
        public int Amount;
    }

    public Vec2i GridSize = new Vec2i(32, 16) * 3;
    public Rect<Vec2> Domain = new Rect<Vec2>(Vec2.Zero, new Vec2(1, .5f));
    public IBounding<Vec2> Bounds;

    public double advectionFactor = 1;
    public double diffusionFactor = 0;

    public override void Initialize()
    {
        DensityGrid = new RegularGrid<Vec2i, Cell>(GridSize);
        for (int i = 0; i < DensityGrid.Data.Length; i++)
        {
            DensityGrid.Data[i] = new Cell();
        }

        Bounds = new GenBounding<Vec2>([BoundaryType.Periodic, BoundaryType.Fixed], Domain);
        for (int i = 0; i < 10000; i++)
        {
            var parcel = new HeatParcel
            {
                Position = Utils.Random(Domain),
                Amount = 1,
            };
            DensityGrid[ToVoxelPos(parcel.Position)].Parcels.Add(parcel);
        }
    }

    public Vec2i ToVoxelPos(Vec2 pos)
    {
        //0.5 => 16 erorr
        return ((pos - Domain.Min) / Domain.Size * (GridSize.ToVecF() - Vec2.One * .001f)).FloorInt();
    }

    public Vec2 ToWorldPos(Vec2i voxel)
    {
        return Domain.Min + voxel.ToVec2() * Domain.Size / (GridSize.ToVecF() - Vec2.Zero);
    }

    public double EstimateHeat(Vec2 pos)
    {
        // Convert position to voxel coordinates (integer part)
        var lt = ToVoxelPos(pos);
        var d = Domain.Size / (GridSize.ToVecF());

        var rt = ToVoxelPos(Bounds.Bound(pos + new Vec2(d.X, 0)));
        var lb = ToVoxelPos(Bounds.Bound(pos + new Vec2(0, d.Y)));
        var rb = ToVoxelPos(Bounds.Bound(pos + new Vec2(d.X, d.Y)));
        // Clamp neighbors to stay inside grid
        // Fractional position inside voxel
        var voxelPos = pos / Domain.Size; // convert world pos to voxel-space float

        var localPos = ((pos - Domain.Min) / Domain.Size * (GridSize.ToVecF() - Vec2.One)) - lt.ToVecF();
        // Bilinear interpolation
        // localPos = new Vec2(.5f, .5f);
        var top = DensityGrid.AtCoords(lt).Parcels.Count * ParcelValue * (1f - localPos.X)
                    + DensityGrid.AtCoords(rt).Parcels.Count * ParcelValue * localPos.X;

        var bottom = DensityGrid.AtCoords(lb).Parcels.Count * ParcelValue * (1f - localPos.X)
                       + DensityGrid.AtCoords(rb).Parcels.Count * ParcelValue * localPos.X;

        var interpolated = top * (1f - localPos.Y) + bottom * localPos.Y;

        return interpolated;
    }

    public Vec2 EstimateGradient(Vec2 pos)
    {
        var delta = Vec2.One / GridSize.ToVecF();

        var center = ToVoxelPos(pos);
        var dens = DensityGrid.AtCoords(center).Density;
        var totWeight = 0f;
        var direction = Vec2.Zero;
        float maxdistance = 6;
        for (int i = -1; i <= 1; i++)
        for (int j = -1; j <= 1; j++)
        {
            if (i == 0 || j == 0)
                continue;

            var coord = new Vec2i(i, j) + center;
            if (coord.X >= GridSize.X)
                coord.X -= GridSize.X;
            if (coord.X < 0)
                coord.X += GridSize.X;

            coord.Y = int.Clamp(coord.Y, 0, GridSize.Y - 1);
            var density = DensityGrid.AtCoords(coord).Density;
            var diff = density - dens;
            var distance = Vec2.Distance(pos, ToWorldPos(center) + (Domain.Size / GridSize.ToVecF()));
            direction += Vec2.Normalize(new Vec2(i, j)) * (maxdistance - distance) * diff;
        }

        return -Vec2.Normalize(direction);

        var tdx = (EstimateHeat(Bounds.Bound(pos - new Vec2(delta.X, 0))) - EstimateHeat(Bounds.Bound(pos + new Vec2(delta.X, 0)))) / delta.X;
        var tdy = (EstimateHeat(Bounds.Bound(pos - new Vec2(0, delta.Y))) - EstimateHeat(Bounds.Bound(pos + new Vec2(0, delta.Y)))) / delta.Y;
        return new Vec2(tdx, tdy);
    }

    public void Step(double dt)
    {
        var w = GetRequiredWorldService<DataService>();
        var velField = w.VectorField;

        for (int i = 0; i < GridSize.X; i++)
        {
            var cell = DensityGrid.AtCoords(new Vec2i(i, 0));
            for (int j = 0; j < (1f - cell.Density) / ParcelValue; j++)
            {
                var bounds = new Rect<Vec2>(ToWorldPos(new Vec2i(i, 0)), ToWorldPos(new Vec2i(i + 1, 1)));
                var parcel = new HeatParcel
                {
                    Position = Utils.Random(bounds),
                    Amount = 1,
                };
                cell.Parcels.Add(parcel);
            }

            DensityGrid.AtCoords(new Vec2i(i, GridSize.Y - 1)).Parcels.Clear();
        }


        foreach (var cell in DensityGrid.Data)
        {
            cell.Density = cell.Parcels.Count * ParcelValue;
        }

        for (int s = 0; s < 5; s++)
        {
            Parallel.For(0, DensityGrid.Data.Length, (i) =>
            {
                var cell = DensityGrid.Data[i];
                var coord = DensityGrid.GetIndexCoords(i);
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    var off = Utils.Clamp<Vec2i, int>(coord + new Vec2i(x, y), Vec2i.Zero, GridSize - Vec2i.One);
                    cell.Temp += (DensityGrid.AtCoords(off).Density - cell.Density) * .1f;
                }
            });

            foreach (var cell in DensityGrid.Data)
            {
                cell.Density += cell.Temp;
                cell.Temp = 0;
            }
        }


        Parallel.ForEach(DensityGrid.Data, (cell) =>
        {
            foreach (var p in cell.Parcels)
            {
                var vec3 = p.Position.Up(w.SimulationTime);
                var u = velField.Evaluate(vec3);
                var gradient = EstimateGradient(p.Position);
                p.Position += u * dt * advectionFactor;
                p.Position += gradient * dt * diffusionFactor;
                p.Position = Bounds.Bound(p.Position);
                DensityGrid.AtCoords(ToVoxelPos(p.Position)).NextParcels.Add(p);
            }
        });

        for (int i = 0; i < DensityGrid.Data.Length; i++)
        {
            var cell = DensityGrid.Data[i];
            cell.Parcels.Clear();
            cell.Parcels.AddRange(cell.NextParcels);
            cell.NextParcels.Clear();
        }
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        Step(GetRequiredWorldService<DataService>().MultipliedDeltaTime);

        for (int i = 0; i < DensityGrid.Data.Length; i++)
        {
            var cell = DensityGrid.Data[i];
            var coords = DensityGrid.GetIndexCoords(i);
            var pos = ((coords.ToVecF() + new Vec2(.5f, .5f)) / GridSize.ToVecF()) * Domain.Size;
            var estimateHeat = EstimateHeat(pos);
            Gizmos2D.Instanced.RegisterRectCenterd(pos, 1f / GridSize.ToVecF() * Domain.Size, new Color(cell.Density * 4, 0, 1, .1f));
            foreach (var p in cell.Parcels)
            {
                Gizmos2D.Instanced.RegisterCircle(p.Position, .001f, Color.White);
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }
    
    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.SliderFloat("Advection Factor", ref advectionFactor, 0, 1);
        ImGuiHelpers.SliderFloat("Diffusion Factor", ref diffusionFactor, 0, .1f);
        base.DrawImGuiSettings();
    }
}