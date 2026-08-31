using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;

namespace FlowExplainer;

public static class ArtifactKeys
{
    public const string LTF_flux_total = "LTF/flux/total";
}

public class LagrangianTransportFormalismService : WorldService
{
    public void Recompute(IVectorField<Vec3,Vec2> u, 
        IVectorField<Vec3,double> T,
        IVectorField<Vec3,double> T_reference,
        IVectorField<Vec3,double> Pe)
    {
        
        /*
        IVectorField<Vec3, double> PeField = null;
        if (dataset.Properties.TryGetValue("Pe", out string? property))
        {
            var Pe = double.Parse(property);
            PeField = new ArbitraryField<Vec3, double>(IDomain<Vec3>.Infinite, _ => Pe);
        }
        else if(dataset.TryGetVectorField<Vec3,double>("Pe", out var f))
        {
            PeField = f;
        }
        else
        {
            throw new Exception("No Pe defined");
        }*/

        var U_mean = 1e-2;
        var T_conv = new ArbitraryField<Vec3, double>(T.Domain, x => T.Evaluate(x) - T_reference.Evaluate(x)) ;
        var h = T.Domain.RectBoundary.Size.X / 100000;
        var T_conv_grad = T_conv.FiniteDifferenceGradientIgnoreLastField<Vec3, Vec2>(h);
        var q_prime = new ArbitraryField<Vec3, Vec2>(T.Domain, x => -(1.0 / Pe.Evaluate(x)) * T_conv_grad.Evaluate(x));
        var q_c = new ArbitraryField<Vec3, Vec2>(T.Domain, x => (u.Evaluate(x)/U_mean) * T_conv.Evaluate(x));
        var q_total = new ArbitraryField<Vec3, Vec2>(T.Domain, x => q_prime.Evaluate(x) + q_c.Evaluate(x));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>
            (q_total, "LTF/flux/total", "Flux (net convective)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>
            (q_c, "LTF/flux/convection", "Flux (convection)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>(q_prime,
            "LTF/flux/conductive", "Flux (conductive)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, double>>(T_conv,
            "LTF/temperature/convective", "Temperature (relative)", "LTF"));
    }

    public override void Initialize()
    {
    }

    public override void Draw(View view)
    {
   
    }
}

public class SpeetjensSpectralDatasetProcesser
{
    public static void RebuildSpeetjensDatasets()
    {
        string folder = Config.GetValue<string>("spectral-data-path")!;
        foreach (var datasetFolder in Directory.GetDirectories(folder).Take(1)) //TEMP
        {
            string outputPath = Path.Combine("Datasets", $"Speetjens-{Path.GetFileName(datasetFolder)}");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            ComputeSpeetjensDataset(datasetFolder, outputPath);
        }
    }

    private static void ComputeSpeetjensDataset(string tspFolder, string outputFieldsFolder)
    {
        var tempTot = SpeetjensSpectralImporterSpectral.Load(tspFolder, false); //TspTOT
        var tempNoFlow = SpeetjensSpectralImporterSpectral.Load(tspFolder, true); //TDIFFspTOT


        var name = Path.GetFileName(Directory.GetFiles(tspFolder).First());
        var parts = name.Split('_');
        var Pe = double.Parse(parts.Single(p => p.StartsWith("Pe=")).Split('=')[1], CultureInfo.InvariantCulture);
        var Epsilon = double.Parse(parts.Single(p => p.StartsWith("EPS=")).Split('=')[1], CultureInfo.InvariantCulture);
        var P = int.Parse(parts[1].Split('x')[1]);

        var velocityField = new SpeetjensVelocityField();
        velocityField.Epsilon = Epsilon;

        //T' = T - T_DIFF:
        //TCONVspTOT = TspTOT-TDIFFspTOT;

        var tempConvection = new SpectralField(new RegularGrid<Vec3i, Complex>(tempTot.Usps.GridSize), tempTot.Rect); //TCONVspTOT
        for (int i = 0; i < tempConvection.Usps.Data.Length; i++)
            tempConvection.Usps.Data[i] = tempTot.Usps.Data[i] - tempNoFlow.Usps.Data[i];


        var bounds = new GenBounding<Vec3>([BoundaryType.Periodic, BoundaryType.Fixed, BoundaryType.Fixed], tempTot.Rect);
        var D1 = DerivY(P);

        var (dCdX, dCdY) = Grad(tempConvection, D1);
        for (int i = 0; i < dCdX.Data.Length; i++)
        {
            dCdX.Data[i] = -dCdX.Data[i] / Pe;
            dCdY.Data[i] = -dCdY.Data[i] / Pe;
        }

        var diffFluxX = new SpectralField(dCdX, tempTot.Rect);
        var diffFluxY = new SpectralField(dCdY, tempTot.Rect);

        var diffFlux = new ArbitraryField<Vec3, Vec2>(tempTot.Domain, (p) => new Vec2(diffFluxX.Evaluate(p), diffFluxY.Evaluate(p)));
        var convectiveHeatFlux = new ArbitraryField<Vec3, Vec2>(tempTot.Domain, (p) => tempConvection.Evaluate(p) * velocityField.Evaluate(p));
        var totalFlux = new ArbitraryField<Vec3, Vec2>(tempTot.Domain, (p) => diffFlux.Evaluate(p) + convectiveHeatFlux.Evaluate(p));

        var gridSize = new Vec3i(100, 50, diffFluxX.Usps.GridSize.Z) / 4;
        // var gridSize = new Vec3i(64, 32, 5);
        //var gridSize = new Vec3i(32, 16, diffFluxX.Usps.GridSize.Z / 8);  

        var physicalSource = new ArbitraryField<Vec3, double>(tempTot.Domain,
            x =>
            {
                var grad = tempNoFlow.FiniteDifferenceGradientIgnoreLast<Vec3, Vec2>(x, .0001f);
                if (double.IsNaN(grad.Y))
                    grad.Y = 0;
                return Vec2.Dot(-velocityField.Evaluate(x), grad);
            });


        if (!Directory.Exists(outputFieldsFolder))
            Directory.CreateDirectory(outputFieldsFolder);

        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "totalFlux"), "Total Flux", gridSize, totalFlux);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "diffFlux"), "Diffusion Flux", gridSize, diffFlux);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "convectiveHeatFlux"), "Convection Flux", gridSize, convectiveHeatFlux);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "tempConvection"), "Convective Temperature", gridSize, tempConvection);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "tempTot"), "Total Temperature", gridSize, tempTot);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "tempNoFlow"), "No Flow Temperature", gridSize, tempNoFlow);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "physicalSource"), "Physical Source", gridSize, physicalSource);

        Dictionary<string, string> props = new Dictionary<string, string>();
        props.Add("Pe", Pe.ToString(CultureInfo.InvariantCulture));
        props.Add("EPS", Epsilon.ToString(CultureInfo.InvariantCulture));
        props.Add("P", P.ToString(CultureInfo.InvariantCulture));
        props.Add("Name", $"Double Gyre EPS={Epsilon}, Pe={Pe}");

        var ser = JsonConvert.SerializeObject(props, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputFieldsFolder, "properties.json"), ser);


        BinarySerializer.Save(Path.Combine(outputFieldsFolder, "velocity.vec3_vec2_field_analytical"), new AnalyticalVectorFieldSave
        {
            DisplayName = "Velocity",
            TypeName = nameof(SpeetjensVelocityField),
            Arguments = [(nameof(SpeetjensVelocityField.Epsilon), Epsilon.ToString())],
        });

        void DiscretizeAndSave<TData>(string path, string name, Vec3i gridSize, IVectorField<Vec3, TData> field)
            where TData : IMultiplyOperators<TData, double, TData>, IAdditionOperators<TData, TData, TData>
        {
            var discritized = new DiscretizedField<Vec3, Vec3i, TData>(gridSize, field, bounds);
            discritized.DisplayName = name;

            var ext = ".vec3_???_field";
            if (typeof(TData) == typeof(double))
                ext = ".vec3_vec1_field";
            if (typeof(TData) == typeof(Vec2))
                ext = ".vec3_vec2_field";
            if (typeof(TData) == typeof(Vec3))
                ext = ".vec3_vec3_field";

            discritized.GridField.SaveToPath(path + ext);
        }
    }

    public static RegularGrid<Vec2i, double> DerivY(int P)
    {
        var D1 = new RegularGrid<Vec2i, double>(new Vec2i(P + 1, P + 1));

        for (int i = 0; i < P + 1; i++)
        for (int j = i + 1; j < P + 1; j++)
        {
            if ((i + j) % 2 == 1)
            {
                D1[new Vec2i(i, j)] = j;
            }
        }

        for (int j = 0; j < P + 1; j++)
        {
            D1[new Vec2i(0, j)] = D1[new Vec2i(0, j)] / 2.0f;
        }

        for (int i = 0; i < P + 1; i++)
        for (int j = 0; j < P + 1; j++)
        {
            D1[new Vec2i(i, j)] = 2.0f * D1[new Vec2i(i, j)];
        }

        return D1;
    }

    public static (RegularGrid<Vec3i, Complex> dCdX, RegularGrid<Vec3i, Complex> dCdY)
        Grad(SpectralField spectralField, RegularGrid<Vec2i, double> D1)
    {
        var I = Complex.ImaginaryOne;
        int M = 64;
        int N = 32;
        double D = 0.5f;

        var gridSize = spectralField.Usps.GridSize;
        var dCdX = new RegularGrid<Vec3i, Complex>(gridSize);
        var dCdY = new RegularGrid<Vec3i, Complex>(gridSize);

        var usps = spectralField.Usps;

        // X-derivative: Fourier differentiation
        for (int t = 0; t < gridSize.Z; t++)
        for (int j = 0; j < gridSize.Y; j++) // Fourier modes
        {
            int m = j; // Mode number
            Complex multiplier = I * (-m) * Math.PI; // i*(-m)*π

            for (int i = 0; i < gridSize.X; i++) // Chebyshev modes
            {
                dCdX[new Vec3i(i, j, t)] = multiplier * usps[new Vec3i(i, j, t)];
            }
        }

        // Y-derivative: Chebyshev matrix multiplication
        for (int t = 0; t < gridSize.Z; t++)
        for (int j = 0; j < gridSize.Y; j++) // For each Fourier mode
        {
            for (int i = 0; i < gridSize.X; i++) // For each output Chebyshev mode
            {
                Complex sum = Complex.Zero;

                // Matrix multiplication: (D1 * Csp)[:, j]
                for (int k = 0; k < gridSize.X; k++)
                {
                    sum += D1[new Vec2i(i, k)] * usps[new Vec3i(k, j, t)];
                }

                dCdY[new Vec3i(i, j, t)] = (2.0 / D) * sum;
            }
        }

        return (dCdX, dCdY);
    }
}