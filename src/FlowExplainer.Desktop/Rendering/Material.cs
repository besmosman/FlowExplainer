using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Numerics;
using Color = System.Drawing.Color;

namespace FlowExplainer
{
    public class Material : IDisposable
    {
        public readonly Shader[] Shaders;
        public readonly int ProgramHandle;

        private readonly Dictionary<string, int> uniformLocations = new();
        private readonly Dictionary<string, MaterialTexture> texturesByUniform = new();

        /*public static Material NewDefaultLit => new(
            Shader.DefaultWorldSpaceVertex,
            Shader.DefaultLitFragment
        );*/

        public static Material NewDefaultUnlit => new(
            Shader.DefaultWorldSpaceVertex,
            Shader.DefaultUnlitFragment
        );

        public Material(params Shader[] shaders)
        {
            Shaders = shaders;

            ProgramHandle = GL.CreateProgram();

            foreach (var shader in shaders)
                GL.AttachShader(ProgramHandle, shader.ShaderHandle);

            GL.LinkProgram(ProgramHandle);
            GL.GetProgram(ProgramHandle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == (int)All.False)
            {
                GL.GetProgram(ProgramHandle, GetProgramParameterName.InfoLogLength, out int maxLogLength);
                GL.GetProgramInfoLog(ProgramHandle, maxLogLength, out _, out var log);
                GL.DeleteProgram(ProgramHandle);
                throw new Exception(log);
            }
        }

        public void Use()
        {
            bool b = false;
            foreach (var shader in Shaders)
                if (shader.HasChanged)
                {
                    shader.HasChanged = false;
                    GL.DetachShader(ProgramHandle, shader.ShaderHandle);
                    GL.AttachShader(ProgramHandle, shader.ShaderHandle);

                    uniformLocations.Clear();
                    texturesByUniform.Clear();
                    b = true;
                }
            if (b)
                GL.LinkProgram(ProgramHandle);

            GL.UseProgram(ProgramHandle);
            foreach (var item in texturesByUniform.Values) //TODO not ideal performance
            {
                GL.ActiveTexture(item.TextureUnit);
                GL.BindTexture(TextureTarget.Texture2D, item.Texture.TextureHandle);
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramHandle);
        }

        public void SetUniform<T>(string name, T val)
        {
            int loc = GetUniformLocation(name);

            switch (val)
            {
                case bool v:
                    GL.ProgramUniform1(ProgramHandle, loc, v ? 1 : 0);
                    break;
                case float v:
                    GL.ProgramUniform1(ProgramHandle, loc, v);
                    break;
                case double v:
                    GL.ProgramUniform1(ProgramHandle, loc, (float)v);
                    break;
                case int v:
                    GL.ProgramUniform1(ProgramHandle, loc, v);
                    break;
                case uint v:
                    GL.ProgramUniform1(ProgramHandle, loc, v);
                    break;
                case byte v:
                    GL.ProgramUniform1(ProgramHandle, loc, v);
                    break;
                case Texture v:
                    {
                        // if the uniform was already set, replace the old association
                        MaterialTexture? materialTexture;
                        if (texturesByUniform.ContainsKey(name))
                        {
                            materialTexture = texturesByUniform[name];
                            materialTexture.Texture = v;
                        }
                        else
                        {
                            materialTexture = new(v, (TextureUnit)(texturesByUniform.Count + (int)TextureUnit.Texture0));
                            texturesByUniform.Add(name, materialTexture);
                        }
                        SetTextureUniformDirectly(loc, v.TextureHandle, materialTexture.TextureUnit, v.TextureTarget);
                    }
                    break;
                case Vec2 v:
                    GL.ProgramUniform2(ProgramHandle, loc, (float)v.X, (float)v.Y);
                    break;
                case Vec3 v:
                    GL.ProgramUniform3(ProgramHandle, loc, (float)v.X, (float)v.Y, (float)v.Z);
                    break;
                case Vec3i v:
                    GL.ProgramUniform3(ProgramHandle, loc, v.X, v.Y, v.Z);
                    break;
                case Vector2 v:
                    GL.ProgramUniform2(ProgramHandle, loc, v.X, v.Y);
                    break;
                case Vector3 v:
                    GL.ProgramUniform3(ProgramHandle, loc, v.X, v.Y, v.Z);
                    break;
                case Color v:
                    GL.ProgramUniform4(ProgramHandle, loc, v.R, v.G, v.B, v.A);
                    break;
                case Vec4 v:
                    GL.ProgramUniform4(ProgramHandle, loc, (float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
                    break;
                case Vector4 v:
                    GL.ProgramUniform4(ProgramHandle, loc, v.X, v.Y, v.Z, v.W);
                    break;
                case float[] v:
                    GL.ProgramUniform1(ProgramHandle, loc, v.Length, v);
                    break;
                case double[] v:
                    GL.ProgramUniform1(ProgramHandle, loc, v.Length, v);
                    break;
                case int[] v:
                    GL.ProgramUniform1(ProgramHandle, loc, v.Length, v);
                    break;
                case Matrix4x4 v:
                    unsafe
                    {
                        GL.ProgramUniformMatrix4(ProgramHandle, loc, 1, false, &v.M11);
                    }
                    break;
                default:
                    throw new Exception($"{typeof(T).Name} is not a supported uniform type");
            }
        }

        public int GetUniformLocation(string name)
        {
            if (!uniformLocations.TryGetValue(name, out int loc))
            {
                loc = GL.GetUniformLocation(ProgramHandle, name);
                uniformLocations.Add(name, loc);
            }

            return loc;
        }

        public void SetTextureUniformDirectly(int loc, int textureHandle, TextureUnit textureUnit, TextureTarget textureTarget)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(textureTarget, textureHandle);
            int textureUnitIndex = (int)textureUnit - (int)TextureUnit.Texture0;
            GL.ProgramUniform1(ProgramHandle, loc, 1, ref textureUnitIndex);
        }
    }
}