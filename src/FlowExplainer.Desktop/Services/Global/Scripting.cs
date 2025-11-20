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
        world.AddVisualisationService(new AxisVisualizer());

        SetGyreDataset(world);
        /*
        world.FlowExplainer.GetGlobalService<PresentationService>().LoadPresentation(new StochasticPresentation());
        world.FlowExplainer.GetGlobalService<PresentationService>().StartPresenting();
        SetGyreDataset(world);
        */




        // MakeDatasetPeriodic(world);
        //world.GetWorldService<DataService>().currentSelectedVectorField = "Total Flux";
        //world.GetWorldService<CriticalPointIdentifier>().Enable();
        //world.GetWorldService<StochasticPoincare>().Enable();

        //MakeDatasetPeriodic(world);

        /*var vel = world.GetWorldService<DataService>().VectorFields["Total Flux"];
        DiscretizedField<Vec3, Vec3i, Vec2> velDisc = new DiscretizedField<Vec3, Vec3i, Vec2>(new Vec3i(64, 32, 800), vel);
        StringBuilder s = new StringBuilder();
        s.Append("time,divergence,changeovertime\r\n");
        for (int z = 1; z < velDisc.GridField.GridSize.Z-1; z++)
        {
            double divergence =0.0;
            double changeOverTime =0.0;
            var d = velDisc.Domain.RectBoundary.Size.Down() / (velDisc.GridField.GridSize.X * velDisc.GridField.GridSize.Y);
            for (int x = 5; x < velDisc.GridField.GridSize.X - 5; x++)
            for (int y = 5; y < velDisc.GridField.GridSize.Y - 5; y++)
            {
                var left = velDisc.GridField.Grid[new Vec3i(x - 1, y, z)];
                var right = velDisc.GridField.Grid[new Vec3i(x + 1, y, z)];
                var up = velDisc.GridField.Grid[new Vec3i(x, y + 1, z)];
                var down = velDisc.GridField.Grid[new Vec3i(x, y - 1, z)];

                divergence += (right - left).X / (2 * d.X) + (up - down).Y / (2 * d.Y);
                changeOverTime += Vec2.Distance(velDisc.GridField.Grid[new Vec3i(x, y, z + 1)], velDisc.GridField.Grid[new Vec3i(x, y, z)]);
            }
            s.AppendLine($"{z.ToString(CultureInfo.InvariantCulture)},{divergence.ToString(CultureInfo.InvariantCulture)},{changeOverTime.ToString(CultureInfo.InvariantCulture)}");
            //changeOverTime /= velDisc.GridField.GridSize.X * velDisc.GridField.GridSize.Y;
        }

       File.WriteAllText("test.csv",s.ToString());*/
        int c = 5;
        //orld.GetWorldService<DataService>().currentSelectedVectorField = "Total Flux";
        //orld.GetWorldService<Poincare3DVisualizer>().Enable();
        //orld.GetWorldService<Poincare3DVisualizer>().SetupTrajects([new Vec2(.4f,.4f)]);
        /*
        var gridVisualizer = world.GetWorldService<GridVisualizer>();
        if (false)
        {
            gridVisualizer.TargetCellCount = 1000;
            gridVisualizer.Enable();
            gridVisualizer.RegularGrid.Interpolate = false;
            gridVisualizer.SetGridDiagnostic(new PoincareSmearGridDiagnostic());
            return;
            /*world.GetWorldService<Poincare3DVisualizer>().Enable();
            world.FlowExplainer.GetGlobalService<ViewsService>().Views[0].Is3DCamera = true;#1#
            /*gridVisualizer.TargetCellCount = 2000;
            gridVisualizer.Enable();
            gridVisualizer.RegularGrid.Interpolate = false;
            gridVisualizer.SetGridDiagnostic(new PoincareSmearGridDiagnostic()
            {
                //UseUnsteady = true,
            });#1#
        }
        else
        {
            var presentationService = world.FlowExplainer.GetGlobalService<PresentationService>()!;
            var updatePresentation = new ProgressPresentation();
            updatePresentation.Prepare(world.FlowExplainer);
            presentationService.LoadPresentation(updatePresentation);
            presentationService.StartPresenting();
            return;
        }
        */


        /*var data = world.GetWorldService<DataService>();
        var gridVisualizer = world.GetWorldService<GridVisualizer>();
        gridVisualizer.Enable();

        //ComputeSpeetjensFields(data, "speetjens-computed-fields");
        //SetGyreDataset(world);
        data.SimulationTime =0.0;
        data.TimeMultiplier = .1f;
        data.currentSelectedVectorField = "Velocity"; //"Total Flux";*/
        /*gridVisualizer.TargetCellCount = 20000;
        gridVisualizer.Enable();
        gridVisualizer.RegularGrid.Interpolate = false;
        gridVisualizer.SetGridDiagnostic(new UFLIC()
        {
            //UseUnsteady = true,
        });*/
        /*return;



        world.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
        var v = gridVisualizer;
        v.Enable();
        v.TargetCellCount = 200000;

        var dat = world.GetWorldService<DataService>();
        dat.currentSelectedVectorField = "Diffusion Flux";


        double[] ts = [0.01f, 0.3f];

        int timeSteps = 100;
        foreach (double t in ts)
        {
            var title = t.ToString(CultureInfo.InvariantCulture);
            var heatStructureGridDiagnostic = new HeatStructureGridDiagnostic()
            {
                T = t,
                M = 4,
                // K = 25,
            };
            v.SetGridDiagnostic(heatStructureGridDiagnostic);

            v.Save($"diffusion-sinks-T={title}.field", .0f, 1f, timeSteps);
            heatStructureGridDiagnostic.Reverse = true;
            v.Save($"diffusion-sources-T={title}.field", .0f, 1f, timeSteps);


            dat.currentSelectedVectorField = "Convection Flux";
            heatStructureGridDiagnostic.Reverse = false;
            v.Save($"convection-sinks-T={title}.field", .0f, 1f, timeSteps);
            heatStructureGridDiagnostic.Reverse = true;
            v.Save($"convection-sources-T={title}.field", .0f, 1f, timeSteps);
        }

        world.GetWorldService<DataService>().ColorGradient = Gradients.Parula;*/
        /*var regularGridVectorField = RegularGridVectorField<Vec3, Vec3i, double>.Load("sources.field");
        dat.ScalerFields.Add("sources", regularGridVectorField);
        dat.currentSelectedScaler = "sources";
        v.SetGridDiagnostic(new TemperatureGridDiagnostic());
        v.Continous = true;*/
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