using System.Numerics;
using System.Runtime.InteropServices;

namespace FlowExplainer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : IEquatable<Vertex>
    {
        public const int Stride =
            sizeof(float) * 3 + // Position
            sizeof(float) * 2 + // TexCoords
            sizeof(float) * 3 + // Normal
            sizeof(float) * 4; // Colour

        public Vector3 Position;
        public Vector2 TexCoords;
        public Vector3 Normal;
        public Vector4 Colour;

        public Vertex(Vector3 pos)
        {
            Position = pos;
            Colour = Vector4.One;
        }
        
        
        public Vertex(Vector2 pos, Vector4 colour, Vector2 uv)
        {
            Position = new Vector3(pos.X,pos.Y,0);
            Colour = colour;
            TexCoords = uv;
        }


        public Vertex(Vector3 pos, Vector2 uv)
        {
            Position = pos;
            TexCoords = uv;
        }

        public Vertex(Vector3 pos, Vector4 col)
        {
            Position = pos;
            Colour = col;
        }

        public Vertex(Vector3 pos, Vector2 uv, Vector4 col)
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