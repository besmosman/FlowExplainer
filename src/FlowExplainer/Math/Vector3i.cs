using System.Runtime.CompilerServices;

namespace FlowExplainer
{
    public struct Vec3i : IEquatable<Vec3i>, IVec<Vec3i, int>
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

        public Vec3 ToVec3()
        {
            return new Vec3(X, Y, Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator *(Vec3i left, int right)
        {
            return new Vec3i(left.X * right, left.Y * right, left.Z * right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator /(Vec3i left, int right)
        {
            return new Vec3i(left.X / right, left.Y / right, left.Z / right);
        }

        public Vec3i Max(Vec3i b)
        {
            return new Vec3i(int.Max(X, b.X), int.Max(Y, b.Y), int.Max(Z, b.Z));
        }

        public int Dimensions => 3;
        public int Last => Z;

        public int this[int n]
        {
            get
            {
                switch (n)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new Exception();
                }
            }
            set
            {
                switch (n)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    case 2:
                        Z = value;
                        return;
                    default:
                        throw new Exception();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator *(int left, Vec3i right)
        {
            return right * left;
        }
    }
}