using ImGuiNET;

namespace FlowExplainer;

public class ImGUIViewRenderer
{
    public static void Render(View view, FlowExplainer flowExplainer)
    {
        var rendertexture = view.PostProcessingTarget;
        ImGui.SetNextWindowSize(rendertexture.Size.ToNumerics(), ImGuiCond.Appearing);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vec2(0, 0));
        if (ImGui.Begin(view.Name, ref view.IsOpen))
        {
            //Vec2 contentMin = ImGui.regio();
            //Vec2 contentMax = ImGui.GetWindowContentRegionMax();
            var min = ImGui.GetCursorScreenPos();
            Vec2 size = (Vec2)ImGui.GetContentRegionAvail() - new Vec2(6, 6);
            view.TargetSize = size;

            //drawing after target size has been set using ImGui info.
            view.World.Draw(view);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vec2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vec2(0, 0));
            ImGui.Image(rendertexture.TextureHandle, size, new Vec2(0, 1), new Vec2(1, 0));
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();
            ImGui.PopStyleVar();

            view.IsSelected = ImGui.IsItemHovered();

            view.RelativeMousePosition = (Vec2)(ImGui.GetMousePos() - min);
            
            
            if (ImGui.IsWindowDocked())
                ImGui.SetNextWindowPos(min + new Vec2(15, 15));
            else
                ImGui.SetNextWindowPos(ImGui.GetWindowPos() + new Vec2(0, -35));
        
            if (ImGui.Begin(view.Name + " overlay", ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                ImGui.Text(view.Name);
                ImGui.OpenPopupOnItemClick(view.Name + " settings", ImGuiPopupFlags.MouseButtonLeft);
                if (ImGui.BeginPopup(view.Name + " settings"))
                {
                    ImGui.Checkbox("3D View", ref view.Is3DCamera);
                    ImGui.EndPopup();
                }
            }
            ImGui.End();

           

        }
        ImGui.End();

      

        /*//issue with overlay when not docked...
        if (ImGui.IsWindowDocked())
            ImGui.SetNextWindowPos(min + new Vec2(15, 45));
        else
            ImGui.SetNextWindowPos(ImGui.GetWindowPos() + new Vec2(0, -35));


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
                    ImGui.EndMenu();#2##1#
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
                    foreach (var vis in flowExplainer.GetGlobalService<WorldManagerService>()!.Worlds)
                    {
                        if (ImGui.MenuItem(vis.Name, "", view.World == vis))
                            view.World = vis;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndPopup();
            }
            ImGui.End();
        }*/
    }
}