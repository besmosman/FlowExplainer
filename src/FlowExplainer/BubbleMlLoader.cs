using NetCDFInterop;
using PureHDF;

namespace FlowExplainer;

public class GribLoader
{
    public RegularGridVectorField<Vec3, Vec3i, Vec2> VelocityField;
    public RegularGridVectorField<Vec3, Vec3i, float> HeatField;

    public void Load()
    {
        var dat = new NGrib.Grib1Reader("C:\\Users\\20183493\\Downloads\\data.grib");
        var records = dat.ReadRecords().ToArray();



        var groups = records.GroupBy(g => g.ProductDefinitionSection.BaseTime).ToArray();
        int nx = groups[0].First().GridDefinitionSection.Nx;
        int ny = groups[0].First().GridDefinitionSection.Ny;
        int t = 0;

        var minTime = records.Min(g => g.ProductDefinitionSection.BaseTime);
        var maxTime = records.Max(g => g.ProductDefinitionSection.BaseTime);
        var timeFrame = (float)(maxTime - minTime).TotalDays;
        VelocityField = new(new Vec3i(nx, ny, groups.Length), Vec3.Zero, new Vec3(nx / 100f, ny / 100f, timeFrame));
        HeatField = new(new Vec3i(nx, ny, groups.Length), Vec3.Zero, new Vec3(nx / 100f, ny / 100f, timeFrame));
        foreach (var group in groups.Take(500))
        {
            var velXRecord = group.First(r => r.ProductDefinitionSection.ParameterNumber == 165);
            var velYRecord = group.First(r => r.ProductDefinitionSection.ParameterNumber == 166);
            var tempRecord = group.First(r => r.ProductDefinitionSection.ParameterNumber == 167);

            // Read data arrays
            var velX = dat.ReadRecordValues(velXRecord).ToArray();
            var velY = dat.ReadRecordValues(velYRecord).ToArray();
            var tempK = dat.ReadRecordValues(tempRecord).ToArray();


            for (int i = 0; i < velX.Length; i++)
            {
                var x = i % nx;
                var y = ny - i / nx;
                VelocityField.Data.AtCoords(new Vec3i(x, y, t)) = new Vec2(velX[i].Value, velY[y].Value);
                HeatField.Data.AtCoords(new Vec3i(x, y, t)) = tempK[i].Value;
            }
            t++;
        }


        var minTemp = HeatField.Data.Data.Min();
        var maxTemp = HeatField.Data.Data.Max();

        for (int i = 0; i < HeatField.Data.Data.Length; i++)
            HeatField.Data.Data[i] = (HeatField.Data.Data[i] - minTemp) / (maxTemp - minTemp);

        int c = 5;
    }
}

public class BubbleMlLoader
{
    public RegularGridVectorField<Vec3, Vec3i, Vec2> VelocityField;
    public RegularGridVectorField<Vec3, Vec3i, float> TemperatureField;
    public RegularGridVectorField<Vec3, Vec3i, float> PressureField;

    public void Load(string name = "Twall-103.hdf5")
    {
        string? path = Config.GetValue<string>("bubble-ml-data-path")!;
        //string filePath = Path.Combine(path, "SingleBubble.hdf5");
        string filePath = Path.Combine(path, name);
        var p = H5File.OpenRead(filePath);

        // Read velocity data - layout is T x Y x X according to spec
        var s = p.Dataset("/velx").ReadAsync<double[]>().Result;
        var velX = p.Dataset("/velx").Read<double[,,]>();
        var velY = p.Dataset("/vely").Read<double[,,]>();
        var temp = p.Dataset("/temperature").Read<double[,,]>();
        var pressure = p.Dataset("/pressure").Read<double[,,]>();

        var xCoord = p.Dataset("/x").Read<double[]>();
        var yCoord = p.Dataset("/y").Read<double[]>();

        //Dimensions: T x Y x X
        int timeSteps = velX.GetLength(0);
        int ySize = velX.GetLength(1);
        int xSize = velX.GetLength(2);

        var gridSize = new Vec3i(xSize, ySize, timeSteps);

        VelocityField = new(gridSize, Vec3.Zero, Vec3.One);
        TemperatureField = new(gridSize, Vec3.Zero, Vec3.One);
        PressureField = new(gridSize, Vec3.Zero, Vec3.One);

        var minX = (float)xCoord.Min();
        var maxX = (float)xCoord.Max();
        var minY = (float)yCoord.Min();
        var maxY = (float)yCoord.Max();

        for (int t = 0; t < timeSteps; t++)
        for (int y = 0; y < ySize; y++)
        for (int x = 0; x < xSize; x++)
        {
            var vX = (float)velX[t, y, x];
            var vY = (float)velY[t, y, x];
            var tem = (float)temp[t, y, x];
            var pres = (float)pressure[t, y, x];
            VelocityField.Data.AtCoords(new Vec3i(x, y, t)) = new Vec2(vX, vY);
            TemperatureField.Data.AtCoords(new Vec3i(x, y, t)) = tem;
            PressureField.Data.AtCoords(new Vec3i(x, y, t)) = pres;
        }

        VelocityField.MinCellPos = new Vec3(minX, minY, 0);
        VelocityField.MaxCellPos = new Vec3(maxX, maxY, timeSteps);
        TemperatureField.MinCellPos = new Vec3(minX, minY, 0);
        TemperatureField.MaxCellPos = new Vec3(maxX, maxY, timeSteps);
        PressureField.MinCellPos = new Vec3(minX, minY, 0);
        PressureField.MaxCellPos = new Vec3(maxX, maxY, timeSteps);
    }
}