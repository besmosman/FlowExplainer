using ImGuiNET;
using OpenTK.Windowing.Common;

namespace FlowExplainer;

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