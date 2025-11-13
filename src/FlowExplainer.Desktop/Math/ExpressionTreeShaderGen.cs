using System.Linq.Expressions;
using System.Reflection;

namespace FlowExplainer;

public class ExpressionTreeShaderGen<T> where T : struct
{
    public static string ToGlsl(string baseString, Expression expression)
    {
        baseString = baseString.Replace("#pragma data", GetDataSpecification());
        baseString = baseString.Replace("#pragma interpolation", GetGeneratedFunctions());
        baseString = baseString.Replace("#pragma toColor", "return " + Convert(expression) + ";");
        return baseString;
    }

    public static string GetGeneratedFunctions()
    {
        string method = "Data linear(Data a, Data b, double c)\r\n{\r\n";
        method += "Data d;\r\n";
        foreach (var v in typeof(T).GetFields())
        {
            string field = v.Name;
            method += $"d.{field} = mix(a.{field},b.{field}, c); \r\n";
        }

        method += "return d; \r\n}";
        return method;
    }

    public static string GetDataSpecification()
    {
        string cur = "";
        foreach (var v in typeof(T).GetFields())
        {
            var fieldType = v.FieldType;
            cur += GetGlslEquivelantTypeName(fieldType) + " " + v.Name + ";" + Environment.NewLine;
        }

        return cur;
    }

    private static string GetGlslEquivelantTypeName(Type fieldType)
    {
        FieldInfo v;
        if (fieldType == typeof(double)) return "double";
        else if (fieldType == typeof(Vec2)) return "vec2";
        else if (fieldType == typeof(Vec3)) return "vec3";
        else if (fieldType == typeof(Vec4)) return "vec4";
        else if (fieldType == typeof(Color)) return "vec4";
        throw new NotImplementedException();
    }

    private static string ConvertArguments(IEnumerable<Expression> cur)
    {
        string s = "";
        foreach (var v in cur)
        {
            s += Convert(v) + ",";
        }

        return s[..^1];
    }

    private static string Convert(Expression cur)
    {
        switch (cur.NodeType)
        {
            case ExpressionType.Add:
                var bin = (BinaryExpression)cur;
                return Convert(bin.Left) + "+" + Convert(bin.Right);
            case ExpressionType.Multiply:
                var mul = (BinaryExpression)cur;
                return Convert(mul.Left) + "*" + Convert(mul.Right);
            case ExpressionType.Subtract:
                var sub = (BinaryExpression)cur;
                return Convert(sub.Left) + "-" + Convert(sub.Right);
            case ExpressionType.Divide:
                var div = (BinaryExpression)cur;
                return Convert(div.Left) + "/" + Convert(div.Right);
            case ExpressionType.ArrayIndex:
                var binary = (BinaryExpression)cur;
                return Convert(binary.Left) + "[" + Convert(binary.Right) + "]";
            case ExpressionType.MemberAccess:
                var mem = (MemberExpression)cur;
                if (mem.Expression is ParameterExpression p && p.Type.Name == nameof(InterpolatedRenderGrid<T>.GlobalGPUData))
                {
                    return mem.Member.Name;
                }

                var memName = mem.Member.Name;
                if (memName.Length == 1)
                    memName = memName.ToLower();

                return Convert(mem.Expression) + "." + memName;
            case ExpressionType.New:
                var neww = (NewExpression)cur;
                return GetGlslEquivelantTypeName(neww.Constructor.DeclaringType) + "(" + ConvertArguments(neww.Arguments) + ")";
            case ExpressionType.Constant:
                var constant = (ConstantExpression)cur;
                return constant.ToString();
            case ExpressionType.Call:
                var ex = (MethodCallExpression)cur;
                if (ex.Object is ParameterExpression parameterExpression && parameterExpression.Type.Name == nameof(InterpolatedRenderGrid<T>.GlobalGPUData))
                {
                    return ex.Method.Name + "(" + ConvertArguments(ex.Arguments) + ")";
                }

                return Convert(ex.Object) + "." + ex.Method.Name + "(" + ConvertArguments(ex.Arguments) + ")";
            case ExpressionType.Parameter:
                var para = (ParameterExpression)cur;
                if (para.Name.Length == 1)
                    return para.Name.ToLower();
                return para.Name;
        }

        throw new NotImplementedException();
    }
}