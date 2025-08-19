using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace FlowExplainer;

public class NcLoader
{
    public RegularGridVectorField<Vec3, Vec3i, Vec2> VelocityField;
    public RegularGridVectorField<Vec3, Vec3i, float> HeatField;

    public void Load()
    {
        var ncpath = Config.GetValue<string>("nc-path");
         var d = DataSet.Open( Path.Combine(ncpath,"cmems_mod_glo_phy_anfc_0.083deg_PT1H-m_1755250746269.nc"));
        // var d = DataSet.Open("C:\\Users\\20183493\\Downloads\\cmems_mod_glo_phy_anfc_0.083deg_PT1H-m_1755591489763.nc");
        //var d = DataSet.Open("C:\\Users\\20183493\\Downloads\\cmems_mod_glo_phy_anfc_0.083deg_PT1H-m_1755506053802.nc");
        //var d = DataSet.Open("C:\\Users\\20183493\\Downloads\\cmems_mod_glo_phy_anfc_0.083deg_PT1H-m_1755528087715.nc");
        //var  d = DataSet.Open("C:\\Users\\20183493\\Downloads\\cmems_mod_glo_phy-cur_anfc_0.083deg_P1D-m_1755589173409.nc");
        // var  d = DataSet.Open("C:\\Users\\20183493\\Downloads\\cmems_mod_glo_phy_anfc_0.083deg_PT1H-m_1755591113298.nc");
        var g = d.Variables.All;
        var uo = d.GetData<float[,,,]>("uo");
        var vo = d.GetData<float[,,,]>("vo");
        float[,,,] temp = new float[
            uo.GetLength(0),
            uo.GetLength(1),
            uo.GetLength(2),
            uo.GetLength(3)
            ];

        if (d.Variables.Contains("thetao"))
        {
            temp = d.GetData<float[,,,]>("thetao");
        }
        int nT = uo.GetLength(0);
        int nY = uo.GetLength(2);
        int nX = uo.GetLength(3);
        var minTemp = float.MaxValue;
        var maxTemp = float.MinValue;

        var min = Vec3.Zero;
        var max = new Vec3(nX, nY, nT) / 100f;
        VelocityField = new(new Vec3i(nX, nY, nT), min, max);
        HeatField = new(new Vec3i(nX, nY, nT), min, max);
        for (int t = 0; t < nT; t++)
        {

            for (int x = 0; x < nX; x++)
            for (int y = 0; y < nY; y++)
            {
                VelocityField.Data.AtCoords(new Vec3i(x, y, t)) = new Vec2(uo[t, 0, y, x], vo[t, 0, y, x]);
                float temper = temp[t, 0, y, x];
                HeatField.Data.AtCoords(new Vec3i(x, y, t)) = temper;
                if (float.IsRealNumber(temper))
                {
                    minTemp = float.Min(minTemp, temper);
                    maxTemp = float.Max(maxTemp, temper);
                }
            }

        }
        for (int t = 0; t < nT; t++)
        for (int x = 0; x < nX; x++)
        for (int y = 0; y < nY; y++)
        {
            HeatField.Data.AtCoords(new Vec3i(x, y, t)) = (HeatField.Data.AtCoords(new Vec3i(x, y, t)) - minTemp) / (maxTemp - minTemp);
        }

        for (int i = 0; i < VelocityField.Data.Data.Length; i++)
        {
            ref var f = ref VelocityField.Data.Data[i];
            if (!float.IsRealNumber(f.Sum()))
                f = Vec2.Zero;
        }
    }
}