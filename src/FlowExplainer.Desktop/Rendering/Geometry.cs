namespace FlowExplainer
{
    public class Geometry
    {
        public Vertex[] Vertices;
        public uint[] Indices;

        public Geometry(Vertex[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}