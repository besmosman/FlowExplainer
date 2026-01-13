using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;



public class TrajectoryVis : WorldService
{
    struct ParticleInfo
    {
        public double TimeAlive;
        public double LifeTime;
    }

    private int ParticleCount = 16000;
    private int HistoryLength = 8;
    private Vec2[] ParticleHistories;
    private ParticleInfo[] ParticleInfos;
    public int CircularBufferIndex;

    private StorageBuffer<Vec2> ParticleHistoryBuffer;
    private StorageBuffer<ParticleInfo> ParticleInfosBuffer;
    private GpuLinePartitioner partitioner;
    public override string? Name => "TrajectoryVis";
    private Material material;

    public override void Initialize()
    {
        material = new Material(new Shader("Assets/Shaders/trajvis.frag", ShaderType.FragmentShader), Shader.DefaultWorldSpaceVertex);
        ParticleHistories = new Vec2[ParticleCount * HistoryLength];
        ParticleInfos = new ParticleInfo[ParticleCount];
        ParticleHistoryBuffer = new StorageBuffer<Vec2>(ParticleHistories);
        ParticleInfosBuffer = new StorageBuffer<ParticleInfo>(ParticleInfos);
        var domainRectBoundary = DataService.VectorField.Domain.RectBoundary;
        for (int i = 0; i < ParticleCount; i++)
        {
            var pos = Utils.Random(domainRectBoundary).XY;
            for (int j = 0; j < HistoryLength; j++)
            {
                ParticleHistoryAt(i, j) = pos;
            }
        }

        partitioner = new GpuLinePartitioner();
        partitioner.GridSize = new Vec2i(32, 16) * 3;
        partitioner.Cells.buffer.BufferIndex = 1;
        partitioner.LinesOrganized.buffer.BufferIndex = 2;
        partitioner.WorldViewRect = DataService.VectorField.Domain.RectBoundary.Reduce<Vec2>();
    }

    public int n = 0;
    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (DataService.MultipliedDeltaTime > 0)
        {
            SimulateStep(-DataService.SimulationTime, -DataService.MultipliedDeltaTime);

            n++;
            for (int p = 0; p < ParticleCount; p++)
            {
                for (int j = 0; j < HistoryLength - 1; j++)
                {
                    // ParticleHistoryAt(p, 0); = 1
                    //ParticleHistoryAt(p, HistoryLength); = 0
                    var start = ParticleHistoryAt(p, j);
                    var end = ParticleHistoryAt(p, j + 1);
                    partitioner.RegisterLine(new GpuLinePartitioner.Line
                    {
                        StartX = (float)start.X,
                        StartY = (float)start.Y,
                        EndX = (float)end.X,
                        EndY = (float)end.Y,
                        ParticleId = p,
                        StartTimeAliveFactor = 1f - (float)j / (HistoryLength - 1),
                        EndTimeAliveFactor = 1f - (float)(j + 1) / (HistoryLength - 1),
                    });
                }
            }

        } 
        partitioner.Organize();

        //DrawRectsDebug(view);
        {
            var size = partitioner.WorldViewRect.Size;
            var start = partitioner.WorldViewRect.Min;
            material.Use();
            material.SetUniform("tint", new Color(1, 0, 1, 1));
            partitioner.LinesOrganized.Use();
            partitioner.LinesOrganized.Upload();
            partitioner.Cells.Upload();
            partitioner.Cells.Use();

            material.SetUniform("WorldViewMin", partitioner.WorldViewRect.Min);
            material.SetUniform("WorldViewMax", partitioner.WorldViewRect.Max);
            material.SetUniform("GridSize", partitioner.GridSize.ToVec2());
            material.SetUniform("view", view.Camera2D.GetViewMatrix());
            material.SetUniform("projection", view.Camera2D.GetProjectionMatrix());
            var model = Matrix4x4.CreateScale((float)size.X, (float)size.Y, .4f) * Matrix4x4.CreateTranslation((float)start.X, (float)start.Y, 0);
            material.SetUniform("model", model);
            Gizmos2D.imageQuadInvertedY.Draw();
        }
        
        Gizmos2D.texturedMat.Use();
       // DrawLines(view);
        // DrawParticles(view);
    }
    private void DrawRectsDebug(View view)
    {

        for (int i = 0; i < partitioner.GridSize.X; i++)
        for (int j = 0; j < partitioner.GridSize.Y; j++)
        {
            var worldStart = partitioner.WorldViewRect.FromRelative(new Vec2(i, j) / partitioner.GridSize);
            var worldEnd = partitioner.WorldViewRect.FromRelative(new Vec2(i + 1, j + 1) / partitioner.GridSize);
            var center = (worldStart + worldEnd) / 2;
            var worldToCell = partitioner.WorldToCell((float)center.X, (float)center.Y);
            var cell = partitioner.Cells.buffer.Data[partitioner.GetCellIndex(worldToCell.X, worldToCell.Y)];
            var color = new Vec4((i % 32) / 32.0, ((j * i * 3248424) % 64) / 64.0, 0, .4f);
            Gizmos2D.Rect(view.Camera2D, worldStart, worldEnd, new Vec4(cell.LinesCount / 10f, 0, 0, 1));
            for (int l = 0; l < cell.LinesCount; l++)
            {
                var line = partitioner.LinesOrganized.buffer.Data[l + cell.LinesStartIndex];
                //  Gizmos2D.Instanced.RegisterLine(new Vec2(line.StartX, line.StartY), new Vec2(line.EndX, line.EndY), new Color(color.X, color.Y, color.Z, 1), .0001f);
            }
        }
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }

    private ref Vec2 ParticleHistoryAt(int p, int h)
    {
        var hI = (CircularBufferIndex - h + HistoryLength) % HistoryLength;
        return ref ParticleHistories[p * HistoryLength + hI];;
    }

    private ref Vec2 ParticleCurrentPosition(int p)
    {
        return ref ParticleHistoryAt(p, 0);
    }

    private void SimulateStep(double t, double dt)
    {
        var integrator = IIntegrator<Vec3, Vec2>.Rk4;
        var vectorfield = DataService.VectorField;
        var rect = vectorfield.Domain.RectBoundary;
        Parallel.For(0, ParticleCount, i =>
        {
            ref var currentPos = ref ParticleHistoryAt(i, 0);
            ref var nextPos = ref ParticleHistoryAt(i, -1);
            var aliveFactor = ParticleInfos[i].TimeAlive / ParticleInfos[i].LifeTime;
            ParticleInfos[i].TimeAlive += double.Abs(dt);
            if (aliveFactor > 1)
            {
                Array.Fill(ParticleHistories, Utils.Random(rect).XY, i * HistoryLength, HistoryLength);
                ParticleInfos[i].LifeTime = Utils.Random(1, 4) * 3;
                ParticleInfos[i].TimeAlive = 0;
            }
            nextPos = integrator.Integrate(vectorfield, currentPos.Up(t), dt).XY;
        });
        CircularBufferIndex++;
    }

    private void DrawParticles(View view)
    {
        float radius = .003f;
        foreach (ref var p in ParticleHistories.AsSpan())
        {
            Gizmos2D.Instanced.RegisterCircle(p, radius, Color.White);
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }

    private void DrawLines(View view)
    {
        float radius = .0003f;
        for (int p = 0; p < ParticleCount; p++)
        {
            
            for (int j = 0; j < HistoryLength-1; j++)
            {
                var start = ParticleHistoryAt(p, j);
                var end = ParticleHistoryAt(p, j + 1);
                var alpha = 1f - (j+1) / (HistoryLength - 1f);
                Gizmos2D.Instanced.RegisterLine(start, end, new Color(0, 1, 0, alpha), radius);
               Gizmos2D.Instanced.RegisterCircle( start, .0003f,  new Color(0, 1, 0, 1));
            }

        }
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Init"))
        {
            Initialize();
        }
        base.DrawImGuiSettings();
    }
}