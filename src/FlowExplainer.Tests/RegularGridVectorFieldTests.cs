namespace FlowExplainer.Tests;

public class RegularGridVectorFieldTests
{
    private void PerfTest()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 1, 0);
        for (int i = 0; i < 100000000; i++)
        {
            a += b - a * 2;
        }

        var r = a;
        a = Vec3.Zero;
        for (int i = 0; i < 100000000; i++)
        {
            for (int d = 0; d < a.ElementCount; d++)
            {
                a[d] += b[d] - a[d] * 2;
            }
        }

        Assert.Equal(a, r);
    }

    [Fact]
    public void RegularGridVectorFieldCoordsTest()
    {
        RegularGridVectorField<Vec2, Vec2i, float> grid = new([0, 1, 2, 3], new Vec2i(2, 2), Vec2.One, new Vec2(2, 5));
        Assert.Equal(Vec2.Zero, grid.ToVoxelCoord(new Vec2(1, 1)), Vec2.ApproximateComparer);
        Assert.Equal(Vec2.One, grid.ToVoxelCoord(new Vec2(2, 5)), Vec2.ApproximateComparer);
        Assert.Equal(Vec2.One / 2f, grid.ToVoxelCoord(new Vec2(1.5f, 3)), Vec2.ApproximateComparer);
    }


    [Fact]
    public void RegularGridVectorField2DTest()
    {
        RegularGridVectorField<Vec2, Vec2i, float> grid = new([0, 1, 2, 3], new Vec2i(2, 2), Vec2.Zero, Vec2.One);
        Assert.Equal(0, grid.Data.GetCoordsIndex(new Vec2i(0, 0)));
        Assert.Equal(1, grid.Data.GetCoordsIndex(new Vec2i(1, 0)));
        Assert.Equal(2, grid.Data.GetCoordsIndex(new Vec2i(0, 1)));
        Assert.Equal(3, grid.Data.GetCoordsIndex(new Vec2i(1, 1)));
        
        Assert.Equal(0, grid.Evaluate(new Vec2(0, 0)));
        Assert.Equal(1, grid.Evaluate(new Vec2(1, 0)));
        Assert.Equal(2, grid.Evaluate(new Vec2(0, 1)));
        Assert.Equal(3, grid.Evaluate(new Vec2(1, 1)));
        Assert.Equal((0 + 1 + 2 + 3) * .25f, grid.Evaluate(new Vec2(.5f, .5f)));

        Assert.Equal(0 * (0.75) * (0.75) +
                     1 * (0.25) * (0.75) +
                     2 * (0.75) * (0.25) +
                     3 * (0.25) * (0.25), grid.Evaluate(new Vec2(.25f, .25f)), 5);


        Assert.Equal(0 * (0.9) * (0.4) +
                     1 * (0.1) * (0.4) +
                     2 * (0.9) * (0.6) +
                     3 * (0.1) * (0.6), grid.Evaluate(new Vec2(.1f, .6f)), 5);
    }

    [Fact]
    public void RegularGridVectorField3DTest()
    {
        RegularGridVectorField<Vec3, Vec3i, float> grid = new([
            0, 1, 2, 3,
            4, 5, 6, 7
        ], new Vec3i(2, 2, 2), Vec3.Zero, Vec3.One);

        Assert.Equal(0, grid.Evaluate(new Vec3(0, 0, 0)));
        Assert.Equal(1, grid.Evaluate(new Vec3(1, 0, 0)));
        Assert.Equal(2, grid.Evaluate(new Vec3(0, 1, 0)));
        Assert.Equal(3, grid.Evaluate(new Vec3(1, 1, 0)));
        Assert.Equal(4, grid.Evaluate(new Vec3(0, 0, 1)));
        Assert.Equal(5, grid.Evaluate(new Vec3(1, 0, 1)));
        Assert.Equal(6, grid.Evaluate(new Vec3(0, 1, 1)));
        Assert.Equal(7, grid.Evaluate(new Vec3(1, 1, 1)));

        Assert.Equal((0 + 1 + 2 + 3 + 4 + 5 + 6 + 7) / 8f, grid.Evaluate(new Vec3(0.5f, 0.5f, 0.5f)), 5);
        Assert.Equal(.92f, grid.Evaluate(new Vec3(0.1f, 0.01f, 0.2f)), 5);
    }
}