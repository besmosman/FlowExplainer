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
        public InputAttribute Attribute;
        public FastFieldAccessor.FieldAccesMethods AccessMethods;
    }

    public List<InputInfo> InputInfos = new();

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
                InputInfos.Add(new InputInfo
                {
                    Name = memberInfo.Name,
                    Type = memberInfo.FieldType,
                    Attribute = attribute,
                    AccessMethods = FastFieldAccessor.Get(memberInfo),
                });
            }
        }
    }

    public void DrawSettings()
    {
        foreach (var inputInfo in InputInfos)
        {
            var value = inputInfo.AccessMethods.GetValue(WorldService);

            if (inputInfo.Type.IsConstructedGenericType && inputInfo.Type.GetGenericTypeDefinition() == typeof(Artifact<>))
            {
                if (ImGui.Button("Select artifact"))
                {
                    ImGui.OpenPopupOnItemClick("artifact-selector", ImGuiPopupFlags.MouseButtonLeft);
                }

                ArtifactSelector(inputInfo);
            }
            else if (inputInfo.Type == typeof(int))
            {
                var v = (int)value;
                ImGui.SliderInt(inputInfo.Name, ref v, (int)inputInfo.Attribute.Min, (int)inputInfo.Attribute.Max);
                inputInfo.AccessMethods.SetValue(WorldService, v);
            }
            else if (inputInfo.Type == typeof(float))
            {
                var v = (float)value;
                ImGui.SliderFloat(inputInfo.Name, ref v, (float)inputInfo.Attribute.Min, (float)inputInfo.Attribute.Max);
                inputInfo.AccessMethods.SetValue(WorldService, v);
            }
            else if (inputInfo.Type == typeof(double))
            {
                var v = (float)(double)value;
                ImGui.SliderFloat(inputInfo.Name, ref v, (float)(double)inputInfo.Attribute.Min, (float)(double)inputInfo.Attribute.Max);
                inputInfo.AccessMethods.SetValue(WorldService, (double)v);
            }
            else if (inputInfo.Type == typeof(bool))
            {
                var v = (bool)value;
                ImGui.Checkbox(inputInfo.Name, ref v);
                inputInfo.AccessMethods.SetValue(WorldService, v);
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public void ArtifactSelector(InputInfo info)
    {
        var windowSize = new Vec2(900, 700);
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2 - windowSize / 2);
        ImGui.SetNextWindowSize(windowSize);
        if (ImGui.BeginPopup("artifact-selector"))
        {
            foreach (var service in WorldService.World.Services)
            {
                var artifacts = service.Artifacts.GetAll(info.Type.GenericTypeArguments[0]);
                foreach (var artifact in artifacts)
                {
                    if (ImGui.Button(artifact.DisplayName))
                    {
                        info.AccessMethods.SetValue(WorldService, artifact);
                        ImGui.CloseCurrentPopup();
                    }
                    //ImGui.Text(artifact.Description);
                }
            }

            ImGui.EndPopup();
        }
    }
}