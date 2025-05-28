using System.Numerics;

namespace FlowExplainer
{
    public struct Bounds
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Center => (Max + Min) / 2;

        public Vector3 Size => (Max - Min).Abs();

        public Bounds(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public Vector3 RelativeCoords(Vector3 p)
        {
            return new Vector3(
                (p.X - Min.X) / (Max.X - Min.X),
                (p.Y - Min.Y) / (Max.Y - Min.Y),
                (p.Z - Min.Z) / (Max.Z - Min.Z)
            );
        }
    }
}