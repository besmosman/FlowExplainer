using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class ImGUIViewRenderer
{
    public static void Render(View view, FlowExplainer flowExplainer)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        var rendertexture = view.PostProcessingTarget;
        ImGui.SetNextWindowSize(rendertexture.Size.ToNumerics(), ImGuiCond.Appearing);
        if (ImGui.Begin(view.Name, ref view.IsOpen))
        {
            //Vector2 contentMin = ImGui.regio();
            //Vector2 contentMax = ImGui.GetWindowContentRegionMax();
            var min = ImGui.GetCursorPos();
            Vector2 size = ImGui.GetContentRegionAvail() - new Vector2(6, 6);
            view.TargetSize = size;

            //drawing after target size has been set using ImGui info.
            view.Visualisation.Draw(view);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
            ImGui.Image(rendertexture.TextureHandle, size, new Vector2(0, 1), new Vector2(1, 0));
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();

            view.IsSelected = ImGui.IsItemHovered();
            view.RelativeMousePosition = ImGui.GetMousePos() - min;

            //issue with overlay when not docked...
            if (ImGui.IsWindowDocked())
                ImGui.SetNextWindowPos(min + new Vector2(15, 45));
            else
                ImGui.SetNextWindowPos(ImGui.GetWindowPos() + new Vector2(0, -35));

            
            if (ImGui.Begin(view.Name + " overlay", ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                    ImGui.Text(view.Name);

                ImGui.OpenPopupOnItemClick(view.Name + " settings", ImGuiPopupFlags.MouseButtonLeft);

                if (ImGui.BeginPopup(view.Name + " settings"))
                {
                    if (ImGui.BeginMenu("Center camera"))
                    {
                        /*foreach (var data in view.Visualisation.GetVisualisationService<TCKDataService>().Data)
                        {
                            if (ImGui.MenuItem(Path.GetFileNameWithoutExtension(data.Path)))
                            {
                                view.CameraOffset = data.Bounds.Min + data.Bounds.Size / 2;
                            }
                        }
                        ImGui.EndMenu();#1#*/
                    }

                    ImGui.Checkbox("Camera lock", ref view.CameraLocked);

                    if (ImGui.BeginMenu("Camera sync"))
                    {
                        if (view.CameraSync != null)
                        {
                            ImGui.MenuItem($"Syncing with: {view.CameraSync.Name}");
                            if (ImGui.BeginMenu("Change sync"))
                            {
                                foreach (var v in flowExplainer.GetGlobalService<ViewsService>()!.Views)
                                {
                                    if (v != view && ImGui.MenuItem(v.Name))
                                    {
                                        view.CameraSync = v;
                                    }
                                }
                                ImGui.EndMenu();
                            }
                            if (ImGui.MenuItem("Disable sync"))
                                view.CameraSync = null;
                        }
                        else
                        {
                            var views = flowExplainer.GetGlobalService<ViewsService>()!.Views;
                            foreach (var v in views)
                            {
                                if (v != view && ImGui.MenuItem(v.Name))
                                    view.CameraSync = v;
                            }
                            if (views.Count == 1)
                                ImGui.MenuItem("No other view found");
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Visualisation"))
                    {
                        foreach (var vis in flowExplainer.GetGlobalService<VisualisationManagerService>()!.Visualisations)
                        {
                            if (ImGui.MenuItem(vis.Name, "", view.Visualisation == vis))
                                view.Visualisation = vis;
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndPopup();
                }
                ImGui.End();
            }
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }
}