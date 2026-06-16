using System.Reflection;
using ImGuiNET;

namespace FlowExplainer;

public class OptionsManager
{
    public WorldService WorldService;

    public class InputInfo
    {
        public string Name;
        public Type Type;
        public object Min;
        public object Max;
        public Action<object, object> SetValue { get; init; }
        public Func<object, object> GetValue { get; init; }
    }

    public List<InputInfo> PersistentInputInfos = new();

    public OptionsManager(WorldService worldService)
    {
        WorldService = worldService;
    }

    public void Init()
    {
        var type = WorldService.GetType();
        var members = type.GetRuntimeFields();
        foreach (var memberInfo in members)
        {
            var attribute = memberInfo.GetCustomAttributes(typeof(InputAttribute), false).Cast<InputAttribute>().SingleOrDefault();
            if (attribute != null)
            {
                RegisterPersistentOption(memberInfo);
            }
        }
    }

    private void RegisterPersistentOption(FieldInfo memberInfo)
    {
        var attribute = memberInfo.GetCustomAttributes(typeof(InputAttribute), false).Cast<InputAttribute>().SingleOrDefault();
        var fieldAccesMethods = FastFieldAccessor.Get(memberInfo);
        PersistentInputInfos.Add(new InputInfo
        {
            Name = memberInfo.Name,
            Type = memberInfo.FieldType,
            Min = attribute.Min,
            Max = attribute.Max,
            GetValue = fieldAccesMethods.GetValue,
            SetValue = fieldAccesMethods.SetValue,
        });
    }

    public void DrawSettings()
    {
        foreach (var inputInfo in PersistentInputInfos)
        {
            DrawOption(inputInfo);
        }
    }
    public void DrawOption(InputInfo inputInfo)
    {
        var value = inputInfo.GetValue(WorldService);

        if (inputInfo.Type.IsConstructedGenericType && inputInfo.Type.GetGenericTypeDefinition() == typeof(Artifact<>))
        {
            var inputInfoType = inputInfo.Type.GenericTypeArguments[0];
            var inputInfoSetValue = inputInfo.SetValue;
            ArtifactSelector(inputInfoType, inputInfoSetValue);
        }
        else if (inputInfo.Type == typeof(int))
        {
            var v = (int)value;
            ImGui.SliderInt(inputInfo.Name, ref v, (int)inputInfo.Min, (int)inputInfo.Max);
            inputInfo.SetValue(WorldService, v);
        }
        else if (inputInfo.Type == typeof(float))
        {
            var v = (float)value;
            ImGui.SliderFloat(inputInfo.Name, ref v, (float)inputInfo.Min, (float)inputInfo.Max);
            inputInfo.SetValue(WorldService, v);
        }
        else if (inputInfo.Type == typeof(double))
        {
            var v = (float)(double)value;
            ImGui.SliderFloat(inputInfo.Name, ref v, (float)(double)inputInfo.Min, (float)(double)inputInfo.Max);
            inputInfo.SetValue(WorldService, (double)v);
        }
        else if (inputInfo.Type == typeof(bool))
        {
            var v = (bool)value;
            ImGui.Checkbox(inputInfo.Name, ref v);
            inputInfo.SetValue(WorldService, v);
        }
        else
        {
            throw new Exception();
        }
    }

    public void ArtifactSelector<T>(ref Artifact<T>? field)
    {
        if (ImGui.Button("Select artifact"))
        {
            ImGui.OpenPopupOnItemClick("artifact-selector", ImGuiPopupFlags.MouseButtonLeft);
        }
        var windowSize = new Vec2(900, 700);
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2 - windowSize / 2);
        ImGui.SetNextWindowSize(windowSize);
        if (ImGui.BeginPopup("artifact-selector"))
        {
            foreach (var service in WorldService.World.Services)
            {
                var artifacts = service.Artifacts.GetAll(typeof(T));
                foreach (var artifact in artifacts)
                {
                    if (ImGui.Button(artifact.DisplayName))
                    {
                        field = (Artifact<T>)artifact;
                        ImGui.CloseCurrentPopup();
                    }
                    //ImGui.Text(artifact.Description);
                }
            }
            ImGui.EndPopup();
        }        
    }

    public void ArtifactSelector(Type inputInfoType, Action<object, object> setter)
    {
        if (ImGui.Button("Select artifact"))
        {
            ImGui.OpenPopupOnItemClick("artifact-selector", ImGuiPopupFlags.MouseButtonLeft);
        }
        ArtifactPopup(inputInfoType, setter);
    }

    public void ArtifactPopup(Type type, Action<object, object> setter)
    {
        var windowSize = new Vec2(900, 700);
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2 - windowSize / 2);
        ImGui.SetNextWindowSize(windowSize);
        if (ImGui.BeginPopup("artifact-selector"))
        {
            foreach (var service in WorldService.World.Services)
            {
                var artifacts = service.Artifacts.GetAll(type);
                foreach (var artifact in artifacts)
                {
                    if (ImGui.Button(artifact.DisplayName))
                    {
                        setter(WorldService, artifact);
                        ImGui.CloseCurrentPopup();
                    }
                    //ImGui.Text(artifact.Description);
                }
            }
            ImGui.EndPopup();
        }
    }
    
}