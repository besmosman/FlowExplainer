using System.Runtime.InteropServices;

namespace FlowExplainer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : IEquatable<Vertex>
    {
        public const int Stride =
            sizeof(double) * 3 + // Position
            sizeof(double) * 2 + // TexCoords
            sizeof(double) * 3 + // Normal
            sizeof(double) * 4; // Colour

        public Vec3 Position;
        public Vec2 TexCoords;
        public Vec3 Normal;
        public Vec4 Colour;

        public Vertex(Vec3 pos)
        {
            Position = pos;
            Colour = Vec4.One;
        }
        
        
        public Vertex(Vec2 pos, Vec4 colour, Vec2 uv)
        {
            Position = new Vec3(pos.X,pos.Y,0);
            Colour = colour;
            TexCoords = uv;
        }


        public Vertex(Vec3 pos, Vec2 uv)
        {
            Position = pos;
            TexCoords = uv;
        }

        public Vertex(Vec3 pos, Vec4 col)
        {
            Position = pos;
            Colour = col;
        }

        public Vertex(Vec3 pos, Vec2 uv, Vec4 col)
        {
            Position = pos;
            TexCoords = uv;
            Colour = col;
        }

        public override bool Equals(object? obj) => obj is Vertex vertex && Equals(vertex);

        public bool Equals(Vertex other)
        {
            return Position.Equals(other.Position) &&
                   TexCoords.Equals(other.TexCoords) &&
                   Colour.Equals(other.Colour) &&
                   Normal.Equals(other.Normal);
        }

        public override int GetHashCode() => HashCode.Combine(Position, TexCoords, Colour, Normal);

        public static bool operator ==(Vertex left, Vertex right) => left.Equals(right);

        public static bool operator !=(Vertex left, Vertex right) => !(left == right);
    }
}