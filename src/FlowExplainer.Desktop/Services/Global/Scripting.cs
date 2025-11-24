using System.Data;
using System.Globalization;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;

namespace FlowExplainer;


public static class Scripting
{
    public static void Startup(World world)
    {
        string datasetPath = Config.GetValue<string>("spectral-data-path")!;
       // RebuildSpeetjensDatasets(datasetPath);
       
        LoadPeriodicCopies(world);

        SetGyreDataset(world);
        world.DataService.LoadedDataset.VectorFields.Add("s", new NonlinearSaddleFlow());
        
        world.AddVisualisationService(new AxisVisualizer());
       // world.AddVisualisationService(new StochasticConnectionVisualization());
    }
    private static void LoadPeriodicCopies(World world)
    {

        var datasetsService = world.FlowExplainer.GetGlobalService<DatasetsService>();
        foreach (var d in datasetsService.Datasets.ToList())
        {
            var cop = d.Value.Clone();

            cop.Load = dataset =>
            {
                d.Value.Load(dataset);
                MakeDatasetPeriodic(dataset, 5, 1);
            };

            cop.Properties["Name"] = "(P) " + cop.Properties["Name"];
            datasetsService.Datasets.Add(cop.Name, cop);
        }
    }


    /*public static void SetBickly(World world)
    {
        var dat = world.GetWorldService<DataService>();
        dat.VectorFields.Add("Velocity", new BickleyJet2());
    }*/


    public static void RebuildSpeetjensDatasets(string folder)
    {
        foreach (var datasetFolder in Directory.GetDirectories(folder))
        {
            string outputPath = Path.Combine("Datasets", $"Speetjens-{Path.GetFileName(datasetFolder)}");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            ComputeSpeetjensDataset(datasetFolder, outputPath);
        }
    }

    public static void MakeDatasetPeriodic(Dataset dat, float t, float period)
    {
        foreach (var p in dat.VectorFields.ToList())
        {
            var domain = new RectDomain<Vec3>(p.Value.Domain.RectBoundary, p.Value.Domain.Bounding);
            domain.MakeFinalAxisPeriodicSlice(t, period);
            dat.VectorFields[p.Key] = new ArbitraryField<Vec3, Vec2>(domain, c =>
            {
                var pPeriodic = c;
                pPeriodic.Z = pPeriodic.Last % period + t;
                return p.Value.Evaluate(pPeriodic);
            })
            {
                DisplayName = p.Value.DisplayName,
            };
        }

        foreach (var p in dat.ScalerFields.ToList())
        {
            var domain = new RectDomain<Vec3>(p.Value.Domain.RectBoundary);
            domain.MakeFinalAxisPeriodicSlice(t, period);
            dat.ScalerFields[p.Key] = new ArbitraryField<Vec3, double>(domain, c =>
            {
                var pPeriodic = c;
                pPeriodic.Z = pPeriodic.Last % period + t;
                return p.Value.Evaluate(pPeriodic);
            })
            {
                DisplayName = p.Value.DisplayName,
            };
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
        velocityField.epsilon = Epsilon;

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

        var gridSize = new Vec3i(64, 32, diffFluxX.Usps.GridSize.Z);
        // var gridSize = new Vec3i(64, 32, 5);
        //var gridSize = new Vec3i(32, 16, diffFluxX.Usps.GridSize.Z / 8);  
        if (!Directory.Exists(outputFieldsFolder))
            Directory.CreateDirectory(outputFieldsFolder);

        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "diffFlux"), "Diffusion Flux", gridSize, diffFlux);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "convectiveHeatFlux"), "Convection Flux", gridSize, convectiveHeatFlux);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "tempConvection"), "Convective Temperature", gridSize, tempConvection);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "tempTot"), "Total Temperature", gridSize, tempTot);
        DiscretizeAndSave(Path.Combine(outputFieldsFolder, "tempNoFlow"), "No Flow Temperature", gridSize, tempNoFlow);

        Dictionary<string, string> props = new Dictionary<string, string>();
        props.Add("Pe", Pe.ToString(CultureInfo.InvariantCulture));
        props.Add("EPS", Epsilon.ToString(CultureInfo.InvariantCulture));
        props.Add("P", P.ToString(CultureInfo.InvariantCulture));
        props.Add("Name", $"Double Gyre EPS={Epsilon}, Pe={Pe}");

        var ser = JsonConvert.SerializeObject(props, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputFieldsFolder, "properties.json"), ser);

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

            discritized.GridField.Save(path + ext);
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

    public static void SetGyreDataset(World w1)
    {
        var name = w1.FlowExplainer.GetGlobalService<DatasetsService>()!.Datasets.First().Key;
        w1.GetWorldService<DataService>().SetDataset(name);
    }
}