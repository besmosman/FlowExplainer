using System.Globalization;
using System.Numerics;

namespace FlowExplainer;

public static class Scripting
{
    public static void Startup(World world)
    {
        SetGyreDataset(world);
        var presentationService = world.FlowExplainer.GetGlobalService<PresentationService>()!;
        presentationService.LoadPresentation(new HeatStructuresPresentation());
        presentationService.StartPresenting();
        return;

        /*
        world.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
        var v = world.GetWorldService<GridVisualizer>();
        v.Enable();
        v.TargetCellCount = 100000;

        var dat = world.GetWorldService<DataService>();
        dat.currentSelectedVectorField = "Diffusion Flux";


        float[] ts = [0.01f, 0.3f];

        int timeSteps = 10;
        foreach (float t in ts)
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
        */

        world.GetWorldService<DataService>().ColorGradient = Gradients.Parula;
        /*var regularGridVectorField = RegularGridVectorField<Vec3, Vec3i, float>.Load("sources.field");
        dat.ScalerFields.Add("sources", regularGridVectorField);
        dat.currentSelectedScaler = "sources";
        v.SetGridDiagnostic(new TemperatureGridDiagnostic());
        v.Continous = true;*/
    }


    public static void SetBickly(World world)
    {
        var dat = world.GetWorldService<DataService>();
        dat.VectorFields.Add("Velocity", new BickleyJet2());
    }


    public static void SetGyreDataset(World world)
    {
        var dat = world.GetWorldService<DataService>();
        string fieldsFolder = "speetjens-computed-fields";
        var DiffFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
        var ConvFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "convectiveHeatFlux.field"));
        var TempConvection = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
        var TempTot = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempTot.field"));
        var TempTotNoFlow = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempNoFlow.field"));
        var totalFlux = new ArbitraryField<Vec3, Vec2>(DiffFluxField.Domain, p => DiffFluxField.Evaluate(p) + ConvFluxField.Evaluate(p));
        var velocityField = new SpeetjensVelocityField()
        {
            epsilon = .1f,
        };

        dat.VectorFields.Clear();
        dat.ScalerFields.Clear();
        dat.VectorFields.Add("Velocity", velocityField);
        dat.VectorFields.Add("Diffusion Flux", DiffFluxField);
        dat.VectorFields.Add("Convection Flux", ConvFluxField);
        dat.VectorFields.Add("Total Flux", totalFlux);
        dat.ScalerFields.Add("Total Temperature", TempTot);
        dat.ScalerFields.Add("Convective Temperature", TempConvection);
        dat.ScalerFields.Add("No Flow Temperature", TempTotNoFlow);

        // TempratureField = temprature;
    }

    private static void ComputeSpeetjensFields(DataService dataService, string folder)
    {
        string datasetPath = Config.GetValue<string>("spectral-data-path")!;
        var tempTot = SpeetjensSpectralImporterSpectral.Load(datasetPath, false); //TspTOT
        var tempNoFlow = SpeetjensSpectralImporterSpectral.Load(datasetPath, true); //TDIFFspTOT

        var P = 32;
        var Pe = 100;

        var velocityField = new SpeetjensVelocityField();
        velocityField.epsilon = .1f;

        //T' = T - T_DIFF:
        //TCONVspTOT = TspTOT-TDIFFspTOT;

        var tempConvection = new SpectralField(new RegularGrid<Vec3i, Complex>(tempTot.Usps.GridSize), tempTot.Rect); //TCONVspTOT
        for (int i = 0; i < tempConvection.Usps.Data.Length; i++)
            tempConvection.Usps.Data[i] = tempTot.Usps.Data[i] - tempNoFlow.Usps.Data[i];


        float t = .9f;
        var heatFlux = new ArbitraryField<Vec3, Vec2>(tempTot.Domain, pos =>
            velocityField.Evaluate(pos) * tempTot.Evaluate(pos));


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

        var gridSize = new Vec3i(64, 32, 103);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        DiscretizeAndSave(Path.Combine(folder, "diffFlux.field"), gridSize, diffFlux);
        DiscretizeAndSave(Path.Combine(folder, "tempTot.field"), gridSize, tempTot);
        DiscretizeAndSave(Path.Combine(folder, "heatFlux.field"), gridSize, heatFlux);
        DiscretizeAndSave(Path.Combine(folder, "tempConvection.field"), gridSize, tempConvection);
        DiscretizeAndSave(Path.Combine(folder, "tempNoFlow.field"), gridSize, tempNoFlow);
        DiscretizeAndSave(Path.Combine(folder, "convectiveHeatFlux.field"), gridSize, convectiveHeatFlux);


        void DiscretizeAndSave<Vec, Veci, TData>(string path, Veci gridSize, IVectorField<Vec, TData> field)
            where Vec : IVec<Vec>, IVecIntegerEquivelant<Veci>
            where Veci : IVec<Veci, int>, IVecFloatEquivelant<Vec>
            where TData : IMultiplyOperators<TData, float, TData>, IAdditionOperators<TData, TData, TData>
        {
            var discritized = new DiscritizedField<Vec, Veci, TData>(gridSize, field);
            discritized.GridField.Save(path);
        }
    }

    public static RegularGrid<Vec2i, float> DerivY(int P)
    {
        var D1 = new RegularGrid<Vec2i, float>(new Vec2i(P + 1, P + 1));

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
        Grad(SpectralField spectralField, RegularGrid<Vec2i, float> D1)
    {
        var I = Complex.ImaginaryOne;
        int M = 64;
        int N = 32;
        float D = 0.5f;

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