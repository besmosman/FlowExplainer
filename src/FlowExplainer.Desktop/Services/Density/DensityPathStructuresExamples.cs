using ImGuiNET;

namespace FlowExplainer;

public class DensityPathStructuresExamples : WorldService
{
    public struct Entry
    {
        public required string Dataset;
        public required string VectorField;
        public required double Time;
        public required int ParticleCount;
        public required double FictitiousDeltaTime;
        public required double SeedRange;
        public required double VisualizationRange;
        public required double Decay;
    }

    public Entry[] Entries;
    public override void Initialize()
    {
        Entries =
        [
            new Entry
            {
                Dataset = "Double Gyre EPS=0.1, Pe=100",
                VectorField = "Total Flux",
                ParticleCount = 16_000,
                FictitiousDeltaTime = .01f,
                Time = 3,
                SeedRange = .3,
                VisualizationRange = .10f,
                Decay = 0.12f,
            },
            new Entry
            {
                Dataset = "Double Gyre EPS=0.1, Pe=100",
                VectorField = "Total Flux",
                ParticleCount = 16_000,
                FictitiousDeltaTime = -.02f,
                Time = 3,
                SeedRange = .3,
                VisualizationRange = .10f,
                Decay = 0.05f,
            },
            new Entry
            {
                Dataset = "Double Gyre EPS=0, Pe=100",
                VectorField = "Diffusion Flux",
                ParticleCount = 10_000,
                FictitiousDeltaTime = -.05f,
                Time = 3,
                SeedRange = 3,
                VisualizationRange = 2f,
                Decay = 0.1f,
            },
            new Entry
            {
                Dataset = "Double Gyre EPS=0.1, Pe=100",
                VectorField = "Diffusion Flux",
                ParticleCount = 10_000,
                FictitiousDeltaTime = -.05f,
                Time = 3,
                SeedRange = 3,
                VisualizationRange = 2f,
                Decay = 0.1f,
            },
        ];
    }
    public override void Draw(View view)
    {
        Initialize();
    }
    public override void DrawImGuiSettings()
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            var entry = Entries[i];
            if (ImGui.Button("Example " + i))
            {
                DataService.SimulationTime = entry.Time;
                DataService.SetDataset(entry.Dataset);
                DataService.currentSelectedVectorField = entry.VectorField;
                var particles = GetRequiredWorldService<DensityParticlesData>();
                var structures = GetRequiredWorldService<DensityPathStructuresSpaceTime>();
                particles.dt = entry.FictitiousDeltaTime;
                particles.Particles.ResizeIfNeeded(entry.ParticleCount);
                particles.SeedTimeRange = entry.SeedRange;
                structures.Tau = entry.VisualizationRange;
                structures.Decay = entry.Decay;
                particles.Initialize();
                structures.Initialize();
            }
        }
        base.DrawImGuiSettings();
    }
}