using ImGuiNET;

namespace FlowExplainer;

public class DensityPathStructuresExamples : WorldService
{
    public override string? Name => "Examples";

    public struct Entry
    {
        public required string Dataset;
        public required string VectorField;
        public required double Time;
        public required int ParticleCount;
        public required double FictitiousDeltaTime;
        public required double t0;
        public required double t1;
        public required double tau;
        public required double Decay;
        public required double Power;
    }

    public Entry[] Entries;
    public override void Initialize()
    {
        var e1 = new Entry
        {
            Dataset = "Double Gyre EPS=0.1, Pe=100",
            VectorField = "Total Flux",
            ParticleCount = 20_000,
            FictitiousDeltaTime = .02f,
            Time = 2,
            t0 = 0,
            t1 = 2,
            tau = 0.1,
            Power = 0.5,
            Decay = 0.05f,
        };
        var e2 = e1 with
        {
            FictitiousDeltaTime = -e1.FictitiousDeltaTime,
            t1 = 5,
            t0 = 2,
        };
        var diff = new Entry
        {
            Dataset = "Double Gyre EPS=0, Pe=100",
            VectorField = "Diffusion Flux",
            ParticleCount = 16_000,
            FictitiousDeltaTime = -.02f,
            Time = 1,
            t0 = 0,
            t1 = 4,           
            Power = 0.3,
            tau =3,
            Decay = 0.05f,
        };
        Entries =
        [
            e1,
            e2,
            diff,
            diff with {Dataset =  "Double Gyre EPS=0.1, Pe=100"},
             /*
            new Entry
            {
                Dataset = "Double Gyre EPS=0.1, Pe=100",
                VectorField = "Total Flux",
                ParticleCount = 5_000,
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
                ParticleCount = 5_000,
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
                ParticleCount = 5_000,
                FictitiousDeltaTime = -.05f,
                Time = 3,
                SeedRange = 3,
                VisualizationRange = 2f,
                Decay = 0.1f,
            },*/
        ];
    }
    public override void Draw(View view)
    {

    }
    public override void DrawImGuiSettings()
    {
        ImGui.Begin("Settings");
        ImGui.Columns(2);
        for (int i = 0; i < Entries.Length; i++)
        {
            var entry = Entries[i];
            if (ImGui.Button("Data" + i))
            {
                Initialize();
                LoadExample(entry);
            }                ImGui.SameLine();
        }
        ImGui.NewLine();
        if (ImGui.BeginCombo("Gradient", DataService.ColorGradient.Name.Replace("matlab ", "")))
        {
            foreach (var grad in Gradients.All)
            {
                bool isSelected =  DataService.ColorGradient == grad;
                ImGui.Image(grad.Texture.Value.TextureHandle, new Vec2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()), new Vec2(0, 0), new Vec2(1, 1));
                ImGui.SameLine();
                if (ImGui.Selectable(grad.Name.Replace("matlab ", ""), ref isSelected))
                {
                    DataService.ColorGradient = grad;
                }
            }

            ImGui.EndCombo();
        }
        var struc = GetRequiredWorldService<DensityPathStructuresSpaceTime>();
        ImGuiHelpers.Slider("t_slice", ref DataService.SimulationTime, 0, 5f);
        ImGui.NextColumn();
        ImGuiHelpers.Slider("σ", ref struc.InfluenceRadius, 0, .01f);
        ImGuiHelpers.Slider("α", ref struc.AccumelationFactor, 0, 1f);
        ImGuiHelpers.Slider("d", ref struc.Decay, 0, .5f);
        ImGui.End();
        base.DrawImGuiSettings();
    }

    public void LoadExample(Entry entry)
    {
        DataService.SimulationTime = entry.Time;
        DataService.SetDataset(entry.Dataset);
        DataService.currentSelectedVectorField = entry.VectorField;
        var particles = GetRequiredWorldService<DensityParticlesData>();
        var structures = GetRequiredWorldService<DensityPathStructuresSpaceTime>();
        particles.dFicticious = entry.FictitiousDeltaTime;
        particles.Reversed = false;
        if (particles.dFicticious < 0)
        {
            particles.dFicticious = -particles.dFicticious;
            particles.Reversed = true;
        }
        particles.SeedInterval = new Rect<Vec1>(entry.t0, entry.t1);
        //particles.SeedTimeRange = entry.SeedRange;
        structures.Tau = entry.tau;
        structures.Decay = entry.Decay;
        structures.Power = entry.Power;
        particles.Initialize();
        structures.Initialize();
        particles.Particles.ResizeIfNeeded(entry.ParticleCount);
    }
}