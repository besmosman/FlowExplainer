namespace FlowExplainer
{
    /// <summary>
    /// Because mesh constructor does GL calls which we can't use with <see cref="Parallel"/>
    /// </summary>
    public class UnloadedMesh
    {
        public Geometry Geometry;
        public bool DynamicVertices;
        public bool DynamicIndicies;
        public IVertexAttributes[] AdditionalAttributes;

        public UnloadedMesh(Geometry geometry, bool dynamicVerticies, bool dynamicIndicies, params IVertexAttributes[]? additionalAttributes)
        {
            Geometry = geometry;
            DynamicVertices = dynamicVerticies;
            AdditionalAttributes = additionalAttributes;
        }

        public Mesh Load()
        {
            return new Mesh(Geometry, DynamicVertices, DynamicIndicies, AdditionalAttributes);
        }
    }
}