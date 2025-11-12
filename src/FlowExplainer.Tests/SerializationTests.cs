namespace FlowExplainer.Tests;

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