using System.Linq.Expressions;
using System.Reflection;

namespace FlowExplainer;

public static class FastFieldAccessor
{
    public class FieldAccesMethods
    {
        public Action<object,object> SetValue { get; init; }
        public Func<object, object> GetValue { get; init; }
    }

    private static Dictionary<FieldInfo, FieldAccesMethods> Values = new();

    public static FieldAccesMethods Get(FieldInfo field)
    {
        var method = GetOrGenerate(field);
        return method;
    }

    private static FieldAccesMethods GetOrGenerate(FieldInfo field)
    {
        if (!Values.TryGetValue(field, out var method))
        {
            method = new FieldAccesMethods
            {
                GetValue = GenerateGetter(field),
                SetValue = GenerateSetter(field),
            };
            Values.Add(field, method);
        }

        return method;
    }

    private static Action<object, object> GenerateSetter(FieldInfo field)
    {
        var targetExp = Expression.Parameter(typeof(object), "target");
        var valueExp = Expression.Parameter(typeof(object), "value");

        var fieldExp = Expression.Field(Expression.Convert(targetExp, field.DeclaringType), field);
        var assignExp = Expression.Assign(fieldExp, Expression.Convert(valueExp, field.FieldType));

        return Expression.Lambda<Action<object, object>>(assignExp, targetExp, valueExp).Compile();
    }

    //source: https://blog.zhaytam.com/2020/11/17/expression-trees-property-getter/
    private static Func<object, object> GenerateGetter(FieldInfo property)
    {
        // Define our instance parameter, which will be the input of the Func
        var objParameterExpr = Expression.Parameter(typeof(object), "instance");
        // 1. Cast the instance to the correct type
        var instanceExpr = Expression.TypeAs(objParameterExpr, property.DeclaringType);
        // 2. Call the getter and retrieve the value of the property
        var propertyExpr = Expression.Field(instanceExpr, property);
        // 3. Convert the property's value to object
        var propertyObjExpr = Expression.Convert(propertyExpr, typeof(object));
        // Create a lambda expression of the latest call & compile it
        return Expression.Lambda<Func<object, object>>(propertyObjExpr, objParameterExpr).Compile();
    }
}