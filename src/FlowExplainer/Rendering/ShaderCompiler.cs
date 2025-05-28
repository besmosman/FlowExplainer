using OpenTK.Graphics.OpenGL;
using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer
{
    public readonly struct ShaderCompiler
    {
        public static bool TryCompileShader(int handle, string code, [NotNullWhen(false)] out string? error)
        {
            CompileShader(handle, code);

            GL.GetShaderInfoLog(handle, out string info);
            GL.GetShader(handle, ShaderParameter.CompileStatus, out int compilationResult);

            bool compilationFailed = compilationResult == (int)All.False;

            if (compilationFailed)
            {
                error = $"Shader {handle} error: {info}";
                Logger.LogWarn(error);
                return false;
            }

            error = null;
            return true;
        }

        private static void CompileShader(int index, string code)
        {
            GL.ShaderSource(index, code);
            GL.CompileShader(index);
        }
    }
}