namespace FlowExplainer;

public static class ArtifactSerializer
{
    public static void Save(IArtifact o, string path)
    {
        if (o.ValueObj is DiscretizedField<Vec2, Vec2i, Vec2> field)
        {
            field.GridField.RectDomain = new RectDomain<Vec2>(field.Domain.RectBoundary, GenBounding<Vec2>.None());
            field.DisplayName = o.DisplayName;
            field.GridField.Save(path + ".vec2_vec2_field");
        }
        else if (o.ValueObj is DiscretizedField<Vec2, Vec2i, double> sfield)
        {
            sfield.GridField.RectDomain = new RectDomain<Vec2>(sfield.Domain.RectBoundary, GenBounding<Vec2>.None());
            sfield.DisplayName = o.DisplayName;
            sfield.GridField.Save(path + ".vec2_vec1_field");
        }
        else
            throw new Exception();
    }

    public static IArtifact Load(string path)
    {
        var ext = Path.GetExtension(path);
        if (ext == ".vec2_vec2_field")
        {
            var regularGridVectorField = RegularGridVectorField<Vec2, Vec2i, Vec2>.Load(path);
            return new Artifact<IVectorField<Vec2, Vec2>>(regularGridVectorField, regularGridVectorField.DisplayName, "");
        }
        if (ext == ".vec2_vec1_field")
        {
            var regularGridVectorField = RegularGridVectorField<Vec2, Vec2i, double>.Load(path);
            return new Artifact<IVectorField<Vec2, double>>(regularGridVectorField, regularGridVectorField.DisplayName, "");
        }
        else
            throw new Exception();
    }
}