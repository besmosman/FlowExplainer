using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace FlowExplainer;

public static class ImGuiToolWindows
{
    private static Dictionary<Type, bool> DrawsImGuiElements = new();
    private static ImageTexture settingsIcon = new ImageTexture("Assets/Images/settings-icon.png");


    public static bool OverridesImGuiDrawCall(Type type)
    {
        if (!DrawsImGuiElements.TryGetValue(type, out var v))
        {
            var m = type.GetRuntimeMethods().Where(m => m.Name == nameof(WorldService.DrawImGuiSettings));
            v = m.Single().DeclaringType == type;
            DrawsImGuiElements.Add(type, v);
        }

        return v;
    }



    public static void Draw(ImGUIService imguiService)
    {

        var visualizationService = imguiService.GetRequiredGlobalService<WorldManagerService>();

        var toolWorld = imguiService.GetGlobalService<ViewsService>().Views[0].World;
        if (ImGui.Begin("Services", ref imguiService.RenderData.ShowToolWindow))
        {
            foreach (var s in toolWorld.Services)
            {
                if (!s.IsInitialzied)
                {
                    s.Initialize();
                    s.IsInitialzied = true;
                }

                bool sIsEnabled = s.IsEnabled;

                if (CheckableCollapsingHeader(s, ref sIsEnabled, ImGuiTreeNodeFlags.DefaultOpen))
                {

                    // ImGui.TextColored(ImGuiController.highlightColor, name);
                    /*ImGui.SameLine();
                    ImGui.Checkbox("##Checkbox", ref sIsEnabled);
                    ImGui.Text("Header");
                    s.IsEnabled = sIsEnabled;
                    ImGui.Separator();
                    ImGui.Separator();*/ /**/

                    if (!s.IsEnabled)
                        ImGui.BeginDisabled();
                    ImGui.PushID(s.GetType().Name);
                    //if (OverridesImGuiDrawCall(s.GetType()))
                    {
                        s.DrawImGuiSettings();
                        ImGui.Spacing();
                        ImGui.Spacing();
                    }
                    ImGui.PopID();
                    // ImGui.colla();
                    if (!s.IsEnabled)
                        ImGui.EndDisabled();

                }

                if (s.IsEnabled != sIsEnabled)
                    {

                        if (s.IsEnabled)
                            s.OnDisable();
                        else
                            s.OnEnable();
                        
                        s.IsEnabled = sIsEnabled;
                    }
                
            }
            if (ImGui.Button("Add service"))
            {
                ImGui.OpenPopup("new-service");
            }
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(.1f, .1f, .1f, 1));
            ImGui.SetNextWindowPos(ImGui.GetWindowPos());
            ImGui.SetNextWindowSize(ImGui.GetWindowSize());
            ImGui.SetNextWindowBgAlpha(1);

            if (ImGui.BeginPopup("new-service"))
            {
                if (ImGui.BeginTabBar("options"))
                {
                    foreach (var key in ServicesInfo.ServicesByCategory.Keys)
                    {
                        if (ImGui.BeginTabItem(key))
                        {
                            foreach (var service in ServicesInfo.ServicesByCategory[key])
                            {
                                if (ImGui.Button(service.Name ?? "?", new Vector2(ImGui.GetWindowWidth() - 18, 48)))
                                {
                                    var worldService = (WorldService)Activator.CreateInstance(service.GetType())!;
                                    toolWorld.AddVisualisationService(worldService);
                                    if (!worldService.IsEnabled)
                                    {
                                        worldService.Enable();
                                    }
                                    if(!worldService.IsInitialzied) 
                                        worldService.Initialize();
                                    
                                    ImGui.CloseCurrentPopup();
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 20.0f);
                                    ImGui.TextWrapped(service.Description);
                                    ImGui.PopTextWrapPos();
                                    ImGui.EndTooltip();
                                }
                                /*ImGui.Text(service.Name ?? "?");
                                ImGui.SameLine();
                                ImGui.TextWrapped(service.Description +service.Description +service.Description ?? "???");*/
                                ImGui.Separator();
                            }
                            ImGui.EndTabItem();
                        }
                    }
                    ImGui.EndTabBar();
                }

                ImGui.EndPopup();
            }
            ImGui.PopStyleColor();
            ImGui.End();
        }
    }

    static bool CheckableCollapsingHeader(WorldService service, ref bool v, ImGuiTreeNodeFlags flags = 0)
    {
        unsafe
        {
            int nn = 5;
            string name = service.Name ?? "???";
            ImGui.PushID(name);

            bool isVisible = true;
            /*if (service.ui_needs_open)
            {
                ImGui.SetNextItemOpen(v);
                service.ui_needs_open = false;
            }*/


            bool heading_visible = true;
            bool is_open = ImGui.CollapsingHeader("##CollapsingHeader", ref heading_visible, flags | ImGuiTreeNodeFlags.AllowOverlap);

            if (!heading_visible)
            {
                service.World.RemoveWorldService(service);
            }
            
            if (ImGui.BeginDragDropSource())
            {
                ImGui.Text(name);
                ImGui.SetDragDropPayload("tot", (IntPtr)(&nn), sizeof(int));
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                //ImGui.Text(name);
                ImGui.SameLine();
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    Logger.LogDebug(name);
                }
                ImGui.EndDragDropTarget();
            }



            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1), name);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.GetTextLineHeightWithSpacing() * 2 - 20);
            var c = ImGui.GetStyleColorVec4(ImGuiCol.Header);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Grey(.2f).ToNumerics());
            if (ImGui.Checkbox("##Checkbox", ref v))
                service.ui_needs_open = true;

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.GetTextLineHeightWithSpacing() * 3 - 25);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0));

            if (ImGui.ImageButton("##butn", settingsIcon.TextureHandle, new Vector2(ImGui.GetTextLineHeight()), new Vector2(0f), new Vector2(1)))
            {
                ImGui.OpenPopup("dsata");
            }

            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(.1f, .1f, .1f, 1f));
            if (ImGui.BeginPopup("dsata"))
            {
                ImGui.PushItemWidth(300);
                service.DrawImGuiDataSettings();
                ImGui.PopItemWidth();
                ImGui.EndPopup();

            }
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();


            ImGui.PopStyleColor();
            ImGui.PopID();

            return is_open;

        }
    }

}