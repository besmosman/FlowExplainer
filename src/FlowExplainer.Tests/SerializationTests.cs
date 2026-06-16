namespace FlowExplainer.Tests;

public class TrajectoriesTests
{
    /*[Fact]
    public void ScalingTest()
    {
        var vel = IVectorField<Vec3, Vec2>.Arbitrary(IDomain<Vec3>.Infinite, (x) => new Vec2(double.Sin(x.X), -double.Cos(x.Y)));
        var vel = IVectorField<Vec3, Vec2>.Arbitrary(IDomain<Vec3>.Infinite, (x) => new Vec2(double.Sin(x.X), -double.Cos(x.Y)));
        
        var flowOp = IFlowOperator<Vec2, Vec3>.Default;
        var x = new Vec2(1, 0);
        Assert.True(Vec2.Distance(flowOp.ComputeEnd(0, 1, x, vel),flowOp.ComputeEnd(0, 1, x, vel)));
    }*/
}

public class SerializationTests
{
    [Fact]
    public void GenBoundaryTest()
    {
        var bounds = new GenBounding<Vec3>([BoundaryType.Fixed, BoundaryType.Periodic, BoundaryType.ReflectiveNeumann], new Rect<Vec3>(Vec3.Zero, Vec3.One));
        var copy = BinarySerializer.CloneBySerialization(bounds);
        Assert.Equal(bounds.Rect.Min, copy.Rect.Min);
        Assert.Equal(bounds.Rect.Max, copy.Rect.Max);
        Assert.Equal(bounds.Boundaries, copy.Boundaries);
        Assert.Equal(new Vec3(0, .5f, .5f), bounds.Bound(new Vec3(-1, 2.5f, .5f)));
        Assert.Equal(new Vec3(0, .5f, .5f), copy.Bound(new Vec3(-1, 2.5f, .5f)));
    }
}