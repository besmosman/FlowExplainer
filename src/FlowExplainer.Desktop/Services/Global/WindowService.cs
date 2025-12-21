using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;

namespace FlowExplainer
{
    public class WindowService : GlobalService, IDisposable
    {
        public NativeWindow Window => SWindow;

        public static NativeWindow SWindow = null!;

        //public Vec3 ClearColor = new(1f, 1f, 1f);
        public Color ClearColor = new(15 / 255f, 15 / 255f, 15 / 255f);
        //public Vec3 ClearColor = new(0/255f, 0/255f, 0/255f);

        public override void Initialize()
        {
            var preferencesService = GetRequiredGlobalService<PreferencesService>();

            var width = Config.GetValue<int?>("window-width") ?? 1920;
            var height = Config.GetValue<int?>("window-height") ?? 1080;

            if (width <= 0 || height <= 0)
            {
                width = 1920;
                height = 1080;
            }

        SWindow = new(new NativeWindowSettings
            {
                Title = nameof(FlowExplainer),
                StartFocused = true,
                StartVisible = false,
                ClientSize = new(width, height),
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 1),
                NumberOfSamples = 0,
            });

            ModifyBorderColorsWindows11Only.CustomWindow(Window, System.Drawing.Color.FromArgb(16, 16, 16),
                System.Drawing.Color.White, System.Drawing.Color.FromArgb(36, 36, 36));

            Window.VSync = SetVsync(Config.GetValue<bool?>("vsync") ?? false);
            Window.VSync = VSyncMode.On;
            Window.CenterWindow();
            Window.IsVisible = true;
            Window.Closing += OnWindowClose;
            Window.Resize += OnWindowResize;

            var logo = Image.Load<Rgba32>("Assets/Images/logo.png");
            logo.DangerousTryGetSinglePixelMemory(out var mem);
            var logoBytes = MemoryMarshal.AsBytes(mem.Span).ToArray();

            Window.Icon =
                new OpenTK.Windowing.Common.Input.WindowIcon(
                    new OpenTK.Windowing.Common.Input.Image(logo.Width, logo.Height, logoBytes));
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Multisample);
            GL.Disable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Less);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            GL.LineWidth(1);
        }

        private VSyncMode SetVsync(bool vSync)
        {
            return Window.VSync = vSync ? VSyncMode.On : VSyncMode.Off;
        }

        public override void Draw()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Window.ClientSize.X, Window.ClientSize.Y);
            Window.Context.SwapBuffers();
            // NativeWindow.ProcessWindowEvents(Window.IsEventDriven);
            Window.ProcessEvents(0f);

            //GL.ClearColor(0.13f, 0.11f, 0.18f, 1);
            GL.ClearColor((float)ClearColor.R, (float)ClearColor.G, (float)ClearColor.B, (float)ClearColor.A);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void OnWindowResize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            Config.UpdateValue("window-width", obj.Width);
            Config.UpdateValue("window-height", obj.Height);
            GL.Viewport(0, 0, obj.Size.X, obj.Size.Y);
        }

        private void OnWindowClose(System.ComponentModel.CancelEventArgs obj)
        {
            Window.IsVisible = false;
            FlowExplainer.Exit();
        }

        public void Dispose()
        {
            Window.Close();
            Window.Dispose();
        }
    }
}