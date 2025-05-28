namespace FlowExplainer
{
    public class ShaderImporter
    {
        private Dictionary<string, string> shaders = new();
        private const string ImportComment = "//@";

        public string GetContents(string shaderfullpath)
        {
            //hot reload needs this.
            return File.ReadAllText(shaderfullpath);


            if (!shaders.ContainsKey(shaderfullpath))
                shaders.Add(Path.GetFullPath(shaderfullpath), File.ReadAllText(shaderfullpath));

            return shaders[shaderfullpath];
        }

        public string Build(string shaderPath)
        {
            var shader = GetContents(Path.GetFullPath(shaderPath));
            while (shader.Contains(ImportComment))
            {
                var start = shader.IndexOf(ImportComment);
                var end = shader.IndexOf("\r", start);

                //ik haat line endings. dit moet want als ik repo clone zijn line endings ineens anders
                if (end == -1)
                    end = shader.IndexOf("\n", start);

                var relPath = shader[(start + ImportComment.Length)..end];
                var absPath = Path.GetFullPath(shaderPath + "\\..\\" + relPath);
                shader = shader[..start] + GetContents(absPath) + shader[end..];
            }
            return shader;
        }
    }
}