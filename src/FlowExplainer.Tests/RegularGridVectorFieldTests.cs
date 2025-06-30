namespace FlowExplainer.Tests;

public class RegularGridVectorFieldTests
{
    [Fact]
    public void RegularGridVectorField2DTests()
    {
        
        RegularGridVectorField<Vec2, Vec2i, float> grid = new([0, 1, 2, 3], [2, 2]);
        
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
    public void RegularGridVectorField3DTests()
    {
        RegularGridVectorField<Vec3, Vec3i, float> grid = new([
            0, 1, 2, 3,
            4, 5, 6, 7
        ], [2, 2, 2]);

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