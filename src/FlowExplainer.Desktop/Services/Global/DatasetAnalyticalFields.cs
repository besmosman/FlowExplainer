namespace FlowExplainer;

public static class DatasetAnalyticalFields
{
    public static Dictionary<Type, Dictionary<string, Type>> AnalyticalFieldsByType = new();

    static DatasetAnalyticalFields()
    {
        RegisterAnalyticalField(typeof(SpeetjensVelocityField));
        RegisterAnalyticalField(typeof(PoiseuilleFlowPipe));
    }

    public static void RegisterAnalyticalField(Type type)
    {
        var vectorfieldType = type.GetInterfaces()
            .FirstOrDefault(f => f.IsGenericType
                                 && f.GetGenericTypeDefinition() == typeof(IVectorField<,>));

        if (!AnalyticalFieldsByType.TryGetValue(vectorfieldType, out var types))
        {
            types = new();
            AnalyticalFieldsByType.Add(vectorfieldType, types);
        }

        types.Add(type.Name, type);
    }

    public static IVectorField<TIn, TOut> BuildFieldFromSave<TIn, TOut>(AnalyticalVectorFieldSave save, IDomain<TIn> refDomain)
        where TIn : IVec<TIn, double>
    {
        var f = AnalyticalFieldsByType[typeof(IVectorField<TIn, TOut>)][save.TypeName];
        IVectorField<TIn, TOut> inst = null!;
        var domainConstructor = f.GetConstructor([typeof(IDomain<TIn>)]);
        if (domainConstructor != null)
        {
            inst = (IVectorField<TIn, TOut>)Activator.CreateInstance(f, [refDomain])!;
        }
        else inst = (IVectorField<TIn, TOut>)Activator.CreateInstance(f)!;

        if (save.Arguments != null)
            foreach (var arg in save.Arguments)
            {
                var valueString = arg.Item2;
                var fieldInfo = f.GetField(arg.Item1);
                if (fieldInfo.FieldType == typeof(int))
                    fieldInfo.SetValue(inst, int.Parse(arg.Item2));
                else if (fieldInfo.FieldType == typeof(double))
                    fieldInfo.SetValue(inst, double.Parse(arg.Item2));
                else if (fieldInfo.FieldType == typeof(IDomain<TIn>))
                    fieldInfo.SetValue(inst, refDomain);
                else throw new NotImplementedException();
            }

        return inst;
    }
}