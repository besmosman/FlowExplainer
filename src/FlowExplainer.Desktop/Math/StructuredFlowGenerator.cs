using ImGuiNET;

namespace FlowExplainer;

public class StructuredFlowGenerator : WorldService
{

    public static RegularGridVectorField<Vec3, Vec3i, Vec3> Generate3D()
    {
        var gridSize = new Vec3i(32, 32, 32);
        var domainRect = new Rect<Vec3>(Vec3.Zero, new Vec3(1, 1, 1));
        var domain = new RectDomain<Vec3>(domainRect, new GenBounding<Vec3>([BoundaryType.Periodic, BoundaryType.Periodic, BoundaryType.Periodic], domainRect));
        RegularGridVectorField<Vec3, Vec3i, Vec3> vectorField = new RegularGridVectorField<Vec3, Vec3i, Vec3>(gridSize, domain);
        RegularGridVectorField<Vec3, Vec3i, Vec3> potential = new RegularGridVectorField<Vec3, Vec3i, Vec3>(gridSize, domain);

        vectorField.DisplayName = "Random Structured Flow 3D";
        var delta = domainRect.Size / gridSize.ToVec3();


        var n = new FastNoise();
        {
            for (int z = 0; z < gridSize.Z; z++)
            for (int x = 0; x < gridSize.X; x++)
            for (int y = 0; y < gridSize.Y; y++)
            {
                var m = 80.0;
                var n0 = n.GetNoise(x * m, y * m, z * m);
                var n1 = n.GetNoise(x * m + 1000, y * m, z * m);
                var n2 = n.GetNoise(x * m + 2000, y * m, z * m);
                potential.AtCoords(new Vec3i(x, y, z)) = new Vec3(n0, n1, n2) / 100;
                potential.AtCoords(new Vec3i(x, y, z)) = new Vec3(Random.Shared.NextDouble(), Random.Shared.NextDouble(), Random.Shared.NextDouble()) / 100;
            }
            
            //gpt
            // Compute velocity as curl of potential: v = curl(A)
            for (int z = 0; z < gridSize.Z; z++)
            for (int x = 0; x < gridSize.X; x++)
            for (int y = 0; y < gridSize.Y; y++)
            {
                int xm = (x - 1 + gridSize.X) % gridSize.X;
                int xp = (x + 1) % gridSize.X;
                int ym = (y - 1 + gridSize.Y) % gridSize.Y;
                int yp = (y + 1) % gridSize.Y;
                int zm = (z - 1 + gridSize.Z) % gridSize.Z;
                int zp = (z + 1) % gridSize.Z;

                var Axm = potential.AtCoords(new Vec3i(xm, y, z));
                var Axp = potential.AtCoords(new Vec3i(xp, y, z));
                var Aym = potential.AtCoords(new Vec3i(x, ym, z));
                var Ayp = potential.AtCoords(new Vec3i(x, yp, z));
                var Azm = potential.AtCoords(new Vec3i(x, y, zm));
                var Azp = potential.AtCoords(new Vec3i(x, y, zp));

                // Central differences
                var dAy_dz = (Ayp.Z - Aym.Z) / (2.0 * delta.Y);
                var dAz_dy = (Azp.Y - Azm.Y) / (2.0 * delta.Z);
                var dAz_dx = (Azp.X - Azm.X) / (2.0 * delta.Z); // careful: dz
                var dAx_dz = (Axp.Z - Axm.Z) / (2.0 * delta.X);
                var dAx_dy = (Axp.Y - Axm.Y) / (2.0 * delta.X);
                var dAy_dx = (Ayp.X - Aym.X) / (2.0 * delta.Y);

                // curl(A)
                double u = dAz_dy - dAy_dz; // x-component
                double v = dAx_dz - dAz_dx; // y-component
                double w = dAy_dx - dAx_dy; // z-component
                var m = 6.0;

                var n0 = n.GetNoise(x * m, y * m, z * m);
                var n1 = n.GetNoise(x * m + 1000, y * m, z * m);
                var n2 = n.GetNoise(x * m + 2000, y * m, z * m);
                
                vectorField.AtCoords(new Vec3i(x, y, z)) = new Vec3(n0, n1, n2);
            }
        }
        return vectorField;
    }

    public RegularGridVectorField<Vec3, Vec3i, Vec2> Generate()
    {
        var gridSize = new Vec3i(64, 32, 10);
        var domainRect = new Rect<Vec3>(Vec3.Zero, new Vec3(1, .5f, 100));
        var domain = new RectDomain<Vec3>(domainRect, new GenBounding<Vec3>([BoundaryType.Fixed, BoundaryType.Fixed, BoundaryType.Periodic], domainRect));
        RegularGridVectorField<Vec3, Vec3i, Vec2> vectorField = new RegularGridVectorField<Vec3, Vec3i, Vec2>(gridSize, domain);
        RegularGridVectorField<Vec3, Vec3i, Vec1> streamfunction = new RegularGridVectorField<Vec3, Vec3i, Vec1>(gridSize, domain);

        vectorField.DisplayName = "Random Structured Flow";
        var delta = domainRect.Size.XY / gridSize.XY.ToVec2();


        var n = new FastNoise();
        for (int t = 0; t < gridSize.Z; t++)
        {
            for (int x = 0; x < gridSize.X; x++)
            for (int y = 0; y < gridSize.Y; y++)
            {
                var m = 6.0;
                var n0 = n.GetNoise(x * m, y * m, t * m * 10 + 1);
                var n1 = n.GetNoise(x * m + 1000, y * m, t * m + 1);
                streamfunction.AtCoords(new Vec3i(x, y, t)) = n0 / 100f;
            }
            for (int x = 0; x < gridSize.X; x++)
            for (int y = 0; y < gridSize.Y; y++)
            {

                ref var left = ref streamfunction.AtCoords(new Vec3i(x == 0 ? gridSize.X - 1 : x - 1, y, t));
                ref var right = ref streamfunction.AtCoords(new Vec3i(x == gridSize.X - 1 ? 0 : x + 1, y, t));
                ref var up = ref streamfunction.AtCoords(new Vec3i(x, y == 0 ? gridSize.Y - 1 : y - 1, t));
                ref var down = ref streamfunction.AtCoords(new Vec3i(x, y == gridSize.Y - 1 ? 0 : y + 1, t));

                /*var v = streamfunction.ToWorldPos(new Vec3(x+.5, y+.5, t));
                var d = .01;
                var dx = (streamfunction.Evaluate(v + new Vec3(-d, 0, 0)) - streamfunction.Evaluate(v + new Vec3(d, 0, 0))) / (2*d);
                var dy = (streamfunction.Evaluate(v + new Vec3(0, -d, 0)) - streamfunction.Evaluate(v + new Vec3(0, d, 0))) / (2*d);*/
                var dx = (right - left) * .5 / delta.X;
                var dy = (up - down) * .5 / delta.Y;
                vectorField.AtCoords(new Vec3i(x, y, t)) = new Vec2(-dy, dx);
            }
        }
        return vectorField;
    }

    public RegularGridVectorField<Vec3, Vec3i, Vec2> GenerateOld()
    {
        var gridSize = new Vec3i(32, 16, 2);
        var domainRect = new Rect<Vec3>(Vec3.Zero, new Vec3(1, .5f, 100));
        var domain = new RectDomain<Vec3>(domainRect, new GenBounding<Vec3>([BoundaryType.Fixed, BoundaryType.Fixed, BoundaryType.Periodic], domainRect));
        RegularGridVectorField<Vec3, Vec3i, Vec2> vectorField = new RegularGridVectorField<Vec3, Vec3i, Vec2>(gridSize, domain);
        vectorField.DisplayName = "Random Structured Flow";
        var delta = domainRect.Size.XY / gridSize.XY.ToVec2();
        var n = new FastNoise();
        for (int t = 0; t < gridSize.Z; t++)
        {
            for (int x = 1; x < gridSize.X - 1; x++)
            for (int y = 1; y < gridSize.Y - 1; y++)
            {

                var m = 16.0;
                var n0 = n.GetNoise(x * m, y * m, t * m + 1);
                var n1 = n.GetNoise(x * m + 1000, y * m, t * m + 1);

                //vectorField.AtCoords(new Vec3i(x, y, t)) = new Vec2(n0, n1).Normalized() * .5f;

            }

        }


        for (int t = 0; t < gridSize.Z; t++)
        {


            /*
            for (int y = 0; y < gridSize.Y; y++)
            {
                vectorField.AtCoords(new Vec3i(xCore, y, t)).X = 0;
            }
            */
            var xCore = gridSize.X / 2.0 + double.Sin(t / 10f) * gridSize.X / 4;

            for (int y = 0; y < gridSize.Y; y++)
            {
                vectorField.AtCoords(new Vec3i((int)xCore, y, t)).Y = 1;
                for (int i = -2; i <= 2; i++)
                {
                    //vectorField.AtCoords(new Vec3i(xCore + i, y, t)).Y = 1;

                    /*if (y > gridSize.Y * 4.0 / 5.0)
                    {
                        vectorField.AtCoords(new Vec3i(xCore + i, y, t)).X = 1 * float.Sign(i);
                        vectorField.AtCoords(new Vec3i(xCore + i, y, t)).Y = 0;
                    }*/
                }

            }
            for (int it = 0; it < 00; it++)
            {
                CorrectStep(vectorField, t, (int)xCore);
            }
        }



        /*
        for (int x = 0; x < gridSize.X; x++)
        for (int y = 0; y < gridSize.Y; y++)
        {
            vectorField.AtCoords(new Vec3i(x, y, gridSize.Z - 1)) = vectorField.AtCoords(new Vec3i(x, y, 0));
        }
        */


        return vectorField;
    }


    private static void CorrectStep(RegularGridVectorField<Vec3, Vec3i, Vec2> vectorField, int t, int coreX)
    {
        var gridSize = vectorField.GridSize;
        var delta = vectorField.Domain.RectBoundary.Size.XY / gridSize.XY.ToVec2();

        var tot = 0.0;
        //SetBorders(vectorField, t, coreX, gridSize);

        for (int x = 0; x < gridSize.X; x++)
        for (int y = 0; y < gridSize.Y; y++)
        {
            if (Random.Shared.NextDouble() > .9)
                continue;

            ref var cell = ref vectorField.AtCoords(new Vec3i(x, y, t));
            cell += new Vec2(Random.Shared.NextDouble() - .5f, Random.Shared.NextDouble() - .5f) / 10000f;
            //cell = cell.Normalized();
            ref var uL = ref vectorField.AtCoords(new Vec3i(x == 0 ? gridSize.X - 1 : x - 1, y, t)).X;
            ref var uR = ref vectorField.AtCoords(new Vec3i(x == gridSize.X - 1 ? 0 : x + 1, y, t)).X;
            ref var vD = ref vectorField.AtCoords(new Vec3i(x, y == 0 ? gridSize.Y - 1 : y - 1, t)).Y;
            ref var vU = ref vectorField.AtCoords(new Vec3i(x, y == gridSize.Y - 1 ? 0 : y + 1, t)).Y;

            var div = (uR - uL + vU - vD) * 0.5;
            var toDistrubute = div / 4;
            uR -= toDistrubute;
            uL += toDistrubute;
            vU -= toDistrubute;
            vD += toDistrubute;
            tot += double.Abs(div);
        }
        Logger.LogDebug("Div = " + tot);
    }
    private static void SetBorders(RegularGridVectorField<Vec3, Vec3i, Vec2> vectorField, int t, int coreX, Vec3i gridSize)
    {

        /*
        for (int y = 0; y < gridSize.Y; y++)
        {
            vectorField.AtCoords(new Vec3i(0, y, t)).X = 0;
            vectorField.AtCoords(new Vec3i(gridSize.X - 1, y, t)).X = 0;
        }

        for (int x = 0; x < gridSize.X; x++)
        {
            vectorField.AtCoords(new Vec3i(x, gridSize.Y - 1, t)).Y = 0;
            vectorField.AtCoords(new Vec3i(x, 0, t)).Y = 0;
        }
        */

        for (int y = 0; y < gridSize.Y; y++)
        {
            for (int i = -2; i <= 2; i++)
            {
                vectorField.AtCoords(new Vec3i(coreX + i, y, t)).X = 0;
                vectorField.AtCoords(new Vec3i(coreX + i, y, t)).Y = 1;

                if (y > gridSize.Y * 4.0 / 5.0)
                {
                    vectorField.AtCoords(new Vec3i(coreX + i, y, t)).X = 1 * float.Sign(i);
                    vectorField.AtCoords(new Vec3i(coreX + i, y, t)).Y = 0;
                }
            }

        }
    }

    public override void Initialize()
    {
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Step"))
        {
            var vectorField = (RegularGridVectorField<Vec3, Vec3i, Vec2>)GetRequiredWorldService<DataService>().VectorField;
            var gridSize = vectorField.GridSize;
            var t = 0;
            CorrectStep(vectorField, 0, gridSize.X / 2);

        }
        base.DrawImGuiSettings();
    }

    public override void Draw(View view)
    {

    }
}