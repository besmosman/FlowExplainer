/*using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FlowExplainer.Core.Generator
{
//Oneday... all vecs will be generated.
    [Generator]
    public class VecGenerators : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register any callbacks here
            // We specify here the code requirements from the compiler
        }


        public string GetClass(string type, int count)
        {
            StringBuilder code = new StringBuilder();
            var vecTypeName = $"vec{count}";
            code.AppendLine($"public class {vecTypeName}");
            code.AppendLine($": IVec<{vecTypeName}, {type}>");
            code.AppendLine("{");
            code.AppendLine($"public {type} Max({type}) => {}");
            code.AppendLine("}");
            return code.ToString();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            
            // Create the source code to inject
            string sourceCode =
                @"using System; namespace FlowExplainer.Core 
{" +
                GetClass("double", 2) +
                @"}";
            // Add the source code to the compilation
            context.AddSource("HelloSayenGenerated", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    [Generator]
    public class HelloSayenGenerators : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register any callbacks here
            // We specify here the code requirements from the compiler
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Create the source code to inject
            string sourceCode = @" using System; namespace HelloSayenGenerated { public static class HelloSayen { public static void GreetSayen() => Console.WriteLine(""Hi from the Super Sayen!""); } }";
            // Add the source code to the compilation
            context.AddSource("HelloSayenGenerated", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }
}*/