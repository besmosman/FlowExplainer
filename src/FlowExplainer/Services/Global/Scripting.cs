using System.Numerics;

namespace FlowExplainer;

public static class GridComputations
{
    public static RegularGrid<Veci, TData> Multiply<Veci, TData, TN>(TN v, RegularGrid<Veci, TData> grid)
        where Veci : IVec<Veci, int>
        where TData : IMultiplyOperators<TData, TN, TData>
    {
        var r = new RegularGrid<Veci, TData>(grid.GridSize);
        for (int i = 0; i < grid.Data.Length; i++)
            r.Data[i] = grid.Data[i] * v;
        return r;
    }
}

public static class Scripting
{
    public static void Startup(World world)
    {
        var gridVisualizer = world.GetWorldService<GridVisualizer>();
        var dataService = world.GetWorldService<DataService>();
        gridVisualizer.Enable();
        dataService.ColorGradient = Gradients.GetGradient("BlueGrayRed");
        gridVisualizer.SetGridDiagnostic(new TemperatureGridDiagnostic());

        string fieldsFolder = "speetjens-computed-fields";
        //ComputeSpeetjensFields(dataService, fieldsFolder);

        var diffFlux = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
        var tempTot = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
        dataService.VelocityField = diffFlux;
        dataService.TempratureField = tempTot;

        //dataService.VelocityField = velocityField;
        ///dataService.TempratureField = tempTot;

        //dataService.VelocityField = field ;
        //dataService.TempratureField =  tempConvection;
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

        var field = new ArbitraryField<Vec3, Vec2>(tempTot.Domain, (p) => new Vec2(diffFluxX.Evaluate(p), diffFluxY.Evaluate(p)));

        var gridSize = new Vec3i(64, 32, 103);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        DiscretizeAndSave(Path.Combine(folder, "diffFlux.field"), gridSize, field);
        DiscretizeAndSave(Path.Combine(folder, "tempTot.field"), gridSize, tempTot);
        DiscretizeAndSave(Path.Combine(folder, "heatFlux.field"), gridSize, heatFlux);
        DiscretizeAndSave(Path.Combine(folder, "tempConvection.field"), gridSize, tempConvection);
        DiscretizeAndSave(Path.Combine(folder, "tempNoFlow.field"), gridSize, tempNoFlow);


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