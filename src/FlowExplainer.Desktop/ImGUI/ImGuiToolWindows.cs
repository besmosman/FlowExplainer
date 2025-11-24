using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace FlowExplainer;

public static class ImGuiToolWindows
{
    private static Dictionary<Type, bool> DrawsImGuiElements = new();
    private static ImageTexture settingsIcon = new ImageTexture("Assets/Images/settings-icon.png");
    private static ImageTexture moveIcon = new ImageTexture("Assets/Images/move-icon.png");
    private static ImageTexture closeIcon = new ImageTexture("Assets/Images/close-icon.png");


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
        lastDraggingIndex = draggingIndex;
        var toolWorld = imguiService.GetGlobalService<ViewsService>().Views[0].World;
        if (ImGui.Begin("Services", ref imguiService.RenderData.ShowToolWindow))
        {
            if (ImGui.Button("Add service"))
            {
                ImGui.OpenPopup("new-service");
            }

            var lastCurserY = ImGui.GetCursorScreenPos().Y;
            for (int i = 0; i < toolWorld.Services.Count; i++)
            {
                var s = toolWorld.Services[i];
                if (!s.IsInitialzied)
                {
                    s.Initialize();
                    s.IsInitialzied = true;
                }

                if (dragging != null && (lastCurserY + ImGui.GetCursorScreenPos().Y) / 2 < ImGui.GetMousePos().Y)
                {
                    draggingIndex = i;
                    draggingY = ImGui.GetCursorScreenPos().Y;
                }

                if (s != dragging)
                {

                    bool sIsEnabled = DrawService(s, ref lastCurserY);


                    if (s.IsEnabled != sIsEnabled)
                    {

                        if (s.IsEnabled)
                            s.OnDisable();
                        else
                            s.OnEnable();

                        s.IsEnabled = sIsEnabled;
                    }
                }
            }

            if (dragging != null && (lastCurserY + ImGui.GetCursorScreenPos().Y) / 2 < ImGui.GetMousePos().Y)
            {
                draggingIndex = toolWorld.Services.Count + 1;
                draggingY = ImGui.GetCursorScreenPos().Y;
            }

            if (dragging != null && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                var i = toolWorld.Services.IndexOf(dragging);
                toolWorld.Services.RemoveAt(i);
                if (i < draggingIndex)
                    draggingIndex--;
                
                if (draggingIndex == toolWorld.Services.Count + 1)
                {
                    toolWorld.Services.Add(dragging);
                    
                    
                }else
                    toolWorld.Services.Insert(draggingIndex, dragging);
                dragging = null;

            }

            if (dragging != null)
            {
                ImGui.GetForegroundDrawList().AddLine(new Vector2(ImGui.GetWindowPos().X, draggingY), new Vector2(ImGui.GetWindowPos().X + ImGui.GetWindowWidth(), draggingY), ImGui.GetColorU32(new Vector4(1, 1, 0, 1)), 4);

                Logger.LogDebug(draggingOffsetY.ToString());
                ImGui.SetNextWindowPos(new Vector2(ImGui.GetWindowPos().X, ImGui.GetMousePos().Y - draggingOffsetY));
                ImGui.SetNextWindowSize(new Vector2(ImGui.GetWindowWidth(), dragginHeight));
                ImGui.PushStyleVarY(ImGuiStyleVar.WindowPadding, 0);
                if (ImGui.Begin("for", ImGuiWindowFlags.NoDecoration))
                {
                    // ImGui.Text("wow");
                    DrawService(dragging, ref lastCurserY);
                    ImGui.End();
                }
                ImGui.PopStyleVar();

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
                                    if (!worldService.IsInitialzied)
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
    private static bool DrawService(WorldService s, ref float lastCurserY)
    {

        bool sIsEnabled = s.IsEnabled;


        lastCurserY = ImGui.GetCursorScreenPos().Y;

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

            if (dragging == s)
            {
                dragginHeight = ImGui.GetCursorScreenPos().Y - lastCurserY;
            }
        }
        return sIsEnabled;
    }

    private static WorldService dragging;
    private static int draggingIndex;
    private static int lastDraggingIndex;
    private static float draggingY;
    private static float draggingOffsetY;
    private static float dragginHeight;
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
            bool is_open = ImGui.CollapsingHeader("##CollapsingHeader", flags | ImGuiTreeNodeFlags.AllowOverlap);



            if (!heading_visible)
            {
                service.World.RemoveWorldService(service);
            }
            /*if (ImGui.BeginDragDropSource())
            {
                ImGui.Text(name);
                dragging = service;
                int i = 0;
                ImGui.SetDragDropPayload("drop", IntPtr.Zero, 0);
                ImGui.EndDragDropSource();
            }
            if (ImGui.BeginDragDropTarget())
            {
                //ImGui.Text(name);
                ImGui.SameLine();
                var d = ImGui.AcceptDragDropPayload("drop");
                if (d.NativePtr != default)
                {
                    var toMove = service.World.Services.IndexOf(dragging);
                    service.World.Services.RemoveAt(toMove);
                    var cur = service.World.Services.IndexOf(service);
                    service.World.Services.Insert(cur , dragging);
                    Logger.LogDebug(name);
                }
                ImGui.EndDragDropTarget();
            }*/
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1), name);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.GetTextLineHeightWithSpacing() * 3 - 27);
            var c = ImGui.GetStyleColorVec4(ImGuiCol.Header);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Grey(.2f).ToNumerics());
            if (ImGui.Checkbox("##Checkbox", ref v))
                service.ui_needs_open = true;

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.GetTextLineHeightWithSpacing() * 4 - 30);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0));

            if (ImGui.ImageButton("##setting-btn", settingsIcon.TextureHandle, new Vector2(ImGui.GetTextLineHeight()), new Vector2(0f), new Vector2(1)))
            {
                ImGui.OpenPopup("dsata");
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.GetTextLineHeightWithSpacing() * 2 - 30);
            if (ImGui.ImageButton("##close-btn", closeIcon.TextureHandle, new Vector2(ImGui.GetTextLineHeight()), new Vector2(0f), new Vector2(1)))
            {
                service.World.RemoveWorldService(service);
            }

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.GetTextLineHeightWithSpacing() * 1 - 25);

            var yy = ImGui.GetCursorScreenPos().Y;
            if (ImGui.ImageButton("##move-btn", moveIcon.TextureHandle, new Vector2(ImGui.GetTextLineHeight()), new Vector2(0f), new Vector2(1)))
            {
                draggingOffsetY = ImGui.GetMousePos().Y - yy;
            }
            if (ImGui.IsItemActivated())
            {
                dragging = service;
                draggingOffsetY = ImGui.GetMousePos().Y - yy;
            }
            /*if (ImGui.BeginDragDropSource())
            {
                ImGui.Text(name);
                dragging = service;
                int i = 0;
                ImGui.SetDragDropPayload("drop", IntPtr.Zero, 0);
                ImGui.EndDragDropSource();
            }*/


            /*
            if (ImGui.BeginDragDropSource())
            {
                ImGui.Text(name);
                dragging = service;
                int i = 0;
                ImGui.SetDragDropPayload("drop", IntPtr.Zero, 0);
                ImGui.EndDragDropSource();
            }
            */


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