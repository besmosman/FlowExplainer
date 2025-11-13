namespace FlowExplainer;

public static class FiniteDifferences
{

    public static void Test(IVectorField<Vec3, Vec2> velocityField, RegularGrid<Vec2i, double> temprature, double t, double dt)
    {
        var u = velocityField.Evaluate;

        RegularGrid<Vec2i, Vec2> vel = RegularGrid<Vec2i, Vec2>.Rent(temprature.GridSize);
        RegularGrid<Vec2i, double> laplacianT = RegularGrid<Vec2i, double>.Rent(temprature.GridSize);
        var delta = (1f / vel.GridSize.ToVec2()) * new Vec2(1, .5f);


        for (int x = 0; x < laplacianT.GridSize.X; x++)
        for (int y = 0; y < laplacianT.GridSize.Y; y++)
        {
            var pos = new Vec2(x, y) / laplacianT.GridSize.ToVec2() * new Vec2(1, .5f);
            vel.AtCoords(new Vec2i(x, y)) = u(pos.Up(t));
        }

        var tempGradient =RegularGrid<Vec2i, Vec2>.Rent(temprature.GridSize);

        for (int x = 1; x < laplacianT.GridSize.X - 1; x++)
        for (int y = 1; y < laplacianT.GridSize.Y - 1; y++)
        {
            //var pos = new Vec2(x, y) / laplacianT.GridSize.ToVec2() * new Vec2(1, .5f);
            var center = temprature.AtCoords(new Vec2i(x, y));
            var right = temprature.AtCoords(new Vec2i(x + 1, y));
            var left = temprature.AtCoords(new Vec2i(x - 1, y));
            var up = temprature.AtCoords(new Vec2i(x, y + 1));
            var down = temprature.AtCoords(new Vec2i(x, y - 1));
            var lapX = (right - 2 * center + left) / (delta.X * delta.X);
            var lapY = (up - 2 * center + down) / (delta.Y * delta.Y);

            laplacianT.AtCoords(new Vec2i(x, y)) = lapX + lapY;
            tempGradient.AtCoords(new Vec2i(x, y)) = new Vec2((right - left) / (2 * delta.X), (up - down) / (2 * delta.Y));
        }

        var Pe = 300;
        var rhs = RegularGrid<Vec2i, double>.Rent(temprature.GridSize);
        var lhs = laplacianT;
        for (int x = 0; x < tempGradient.GridSize.X; x++)
        for (int y = 0; y < tempGradient.GridSize.Y; y++)
        {
            rhs.AtCoords(new Vec2i(x, y)) = Pe * Vec2.Dot(vel.AtCoords(new Vec2i(x, y)), tempGradient.AtCoords(new Vec2i(x, y)));
        }

        for (int x = 0; x < tempGradient.GridSize.X; x++)
        for (int y = 0; y < tempGradient.GridSize.Y; y++)
        {
            temprature.AtCoords(new Vec2i(x, y)) += dt * (lhs.AtCoords(new Vec2i(x, y)) - rhs.AtCoords(new Vec2i(x, y)));
        }
        
        RegularGrid<Vec2i, Vec2>.Return(vel);
        RegularGrid<Vec2i, Vec2>.Return(tempGradient);
        RegularGrid<Vec2i, double>.Return(laplacianT);
        RegularGrid<Vec2i, double>.Return(rhs);
        
    }
}