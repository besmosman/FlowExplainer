using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public static class ModifyBorderColorsWindows11Only
{
    private static string ToBgr(System.Drawing.Color c) => $"{c.B:X2}{c.G:X2}{c.R:X2}";

    [DllImport("DwmApi")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

    const int DWWMA_CAPTION_COLOR = 35;
    const int DWWMA_BORDER_COLOR = 34;
    const int DWMWA_TEXT_COLOR = 36;
    public unsafe static void CustomWindow(NativeWindow gameWindow, System.Drawing.Color captionColor, System.Drawing.Color fontColor, System.Drawing.Color borderColor)
    {
        IntPtr hWnd =GLFW.GetWin32Window(gameWindow.WindowPtr);
        //Change caption color
        int[] caption = new int[] { int.Parse(ToBgr(captionColor), System.Globalization.NumberStyles.HexNumber) };
        DwmSetWindowAttribute(hWnd, DWWMA_CAPTION_COLOR, caption, 4);
        //Change font color
        int[] font = new int[] { int.Parse(ToBgr(fontColor), System.Globalization.NumberStyles.HexNumber) };
        DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, font, 4);
        //Change border color
        int[] border = new int[] { int.Parse(ToBgr(borderColor), System.Globalization.NumberStyles.HexNumber) };
        DwmSetWindowAttribute(hWnd, DWWMA_BORDER_COLOR, border, 4);

    }
}