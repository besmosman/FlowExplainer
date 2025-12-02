using ImGuiNET;
using OpenTK.Windowing.Common;

namespace FlowExplainer;

public class NewImGUIRenderService : GlobalService
{
    public override void Initialize()
    {
        ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        ImGuiStylePtr style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }
        ImguiImplOpenTK4.Init(GetGlobalService<WindowService>()!.Window);
        ImguiImplOpenGL3.Init();
        ImGuiController.RefreshImGuiStyleDark();


    }
    public override void Draw()
    {
        ImguiImplOpenGL3.NewFrame();
        ImguiImplOpenTK4.NewFrame();
        ImGui.NewFrame();
        ImGui.DockSpaceOverViewport();
    }

    public override void AfterDraw()
    {
        ImGui.Render();
        ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

        if (ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
            WindowService.SWindow.Context.MakeCurrent();
        }
        base.AfterDraw();
    }
}

public class ImGUIRenderService : GlobalService
{
    private ImGuiController controller;

    public override void Initialize()
    {
        var window = GetRequiredGlobalService<WindowService>().Window;
        controller = new ImGuiController(window.ClientSize.X, window.ClientSize.Y);
        window.Resize += WindowOnResize;
        window.MouseWheel += WindowOnMouseWheel;
        window.TextInput += WindowOnTextInput;
    }

    private void WindowOnTextInput(TextInputEventArgs obj)
    {
        controller.PressChar((char)obj.Unicode);
            
    }

    private void WindowOnMouseWheel(MouseWheelEventArgs obj)
    {
        var window = GetRequiredGlobalService<WindowService>().Window;

        controller.MouseScroll(window.MouseState.ScrollDelta);
    }

    private void WindowOnResize(ResizeEventArgs obj)
    {
        var window = GetRequiredGlobalService<WindowService>().Window;
        Config.UpdateValue("window-width", obj.Width);
        Config.UpdateValue("window-height", obj.Height);
        controller.WindowResized(window.ClientSize.X, window.ClientSize.Y);
    }
        
    public override void Draw()
    {
        var window = GetRequiredGlobalService<WindowService>().Window;
        ImGui.EndFrame();
        controller.Render();
        var io = ImGui.GetIO();
        //ImGui.NewFrame();
        controller.Update(window, FlowExplainer.DeltaTime);

        /*io.DisplayFramebufferScale.X = 1;
            io.DisplayFramebufferScale.Y = 1;
            io.MousePos = new Vec2(window.MousePosition.X, window.MousePosition.Y);
            io.MouseClicked[0] = window.IsMouseButtonPressed(MouseButton.Button1);
            io.MouseReleased[0] = window.IsMouseButtonReleased(MouseButton.Button1);
            io.MouseClicked[1] = window.IsMouseButtonPressed(MouseButton.Button2);
            io.MouseDown[0] = window.IsMouseButtonDown(MouseButton.Button1);
            io.MouseDown[1] = window.IsMouseButtonDown(MouseButton.Button2);
            io.ConfigFlags = ImGuiConfigFlags.DockingEnable;*/

    }
}