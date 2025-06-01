
namespace FlowExplainer
{
    public struct Bounds
    {
        public Vec3 Min;
        public Vec3 Max;
        public Vec3 Center => (Max + Min) / 2;

        public Vec3 Size => (Max - Min).Abs();

        public Bounds(Vec3 min, Vec3 max)
        {
            Min = min;
            Max = max;
        }

        public Vec3 RelativeCoords(Vec3 p)
        {
            return new Vec3(
                (p.X - Min.X) / (Max.X - Min.X),
                (p.Y - Min.Y) / (Max.Y - Min.Y),
                (p.Z - Min.Z) / (Max.Z - Min.Z)
            );
        }
    }
}