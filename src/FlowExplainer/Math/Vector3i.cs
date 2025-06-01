using System.Runtime.CompilerServices;

namespace FlowExplainer
{
    public struct Vec3i : IEquatable<Vec3i>
    {
        public int X;
        public int Y;
        public int Z;

        public Vec3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object? obj)
        {
            return obj is Vec3i other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public Vec3 ToNumerics()
        {
            return new Vec3(X, Y, Z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static bool operator ==(Vec3i left, Vec3i right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator !=(Vec3i left, Vec3i right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator +(Vec3i left, Vec3i right)
        {
            return new Vec3i(
                left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator -(Vec3i left, Vec3i right)
        {
            return new Vec3i(
                left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z
            );
        }

        public bool Equals(Vec3i other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
    }
}