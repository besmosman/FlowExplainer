using System.Threading;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class Shader : IDisposable
    {
        private readonly FileInfo fileSource;

        public readonly ShaderType ShaderType;
        public readonly int ShaderHandle;

        public bool HasChanged;

        public static readonly Shader DefaultWorldSpaceVertex = new("Assets/Shaders/worldspace.vert", ShaderType.VertexShader);
        public static readonly Shader DefaultWorldSpaceLinesVertex = new("Assets/Shaders/worldspacelines.vert", ShaderType.VertexShader);
        public static readonly Shader DefaultUnlitFragment = new("Assets/Shaders/unlit.frag", ShaderType.FragmentShader);
        
        public static readonly Shader DefaultUnlitBoundedFragment = new("Assets/Shaders/unlitBounded.frag", ShaderType.FragmentShader);
        public static readonly Shader DefaultUnlitLinesFragment = new("Assets/Shaders/unlitlines.frag", ShaderType.FragmentShader);
        public static readonly Shader DefaultLitFragment = new("Assets/Shaders/lit.frag", ShaderType.FragmentShader);

        private ShaderImporter ShaderImporter = new ShaderImporter();

        public Shader(string path, ShaderType shaderType, bool hotreload = true)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Invalid shaderName source", Path.GetFullPath(path));


            fileSource = new FileInfo(path);
            ShaderType = shaderType;
            ShaderHandle = GL.CreateShader(ShaderType);

            Recompile();


            //hotreload is een meme met de nieuwe shader
             if (hotreload)
                 AssetWatcher.OnChange += OnSourceChange;
        }

        private void OnSourceChange(FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == Path.GetFileName(fileSource.FullName))
            {
                Thread.Sleep(100);
                //we sleep because the file might still be in the middle of being changed
                //TODO this is far from ideal
                try
                {
                    Recompile(e.FullPath);
                }
                catch (Exception v)
                {
                    Logger.LogWarn(v.Message);
                }
            }
        }

        public void Recompile(string? altPath = null)
        {

            try
            {
                Logger.LogMessage($"Compiling {ShaderType} \"{fileSource.Name}\"...");
                string path = fileSource.FullName;

                if (altPath != null)
                    path = altPath;

                var contents = ShaderImporter.Build(path);

                if (!ShaderCompiler.TryCompileShader(ShaderHandle, contents, out var err))
                    throw new Exception($"{ShaderType} \"{fileSource.Name}\" failed to compile: {err}");
                Logger.LogMessage($"Successfuly compiled \"{fileSource.Name}\"!");

                HasChanged = true;
            }
            catch (IOException e)
            {
                Logger.LogWarn("Failed to read shader file: " + e);
            }
        }

        public void Dispose()
        {
            AssetWatcher.OnChange -= OnSourceChange;
            GL.DeleteShader(ShaderHandle);
        }
    }
}