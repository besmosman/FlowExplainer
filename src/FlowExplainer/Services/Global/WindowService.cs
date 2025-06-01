using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;

namespace FlowExplainer
{
    public class WindowService : GlobalService, IDisposable
    {
        public NativeWindow Window => SWindow;
        public static NativeWindow SWindow = null!;
        //public Vec3 ClearColor = new(1f, 1f, 1f);
        //public Vec3 ClearColor = new(15/255f, 15/255f, 15/255f);
        public Vec3 ClearColor = new(0/255f, 0/255f, 0/255f);

        public override void Initialize()
        {
            var preferencesService = GetRequiredGlobalService<PreferencesService>();
           // preferencesService.OnPreferencesChange += (p) => SetVsync(p.VSync);
            var pref = preferencesService.Preferences;

            SWindow = new(new NativeWindowSettings
            {
                Title = nameof(FlowExplainer),
                StartFocused = true,
                StartVisible = true,
                ClientSize = new(1600,1000 - 40),
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4,1 ),
                NumberOfSamples = 0,
            });

            ModifyBorderColorsWindows11Only.CustomWindow(Window, System.Drawing.Color.FromArgb(16,16,16), System.Drawing.Color.White, System.Drawing.Color.FromArgb(36,36,36) );
            
            bool vSync = pref.VSync;
            //Window.VSync = SetVsync(vSync);
            Window.CenterWindow();
            Window.VSync = VSyncMode.On;
            Window.Closing += OnWindowClose;
            Window.Resize += OnWindowResize;

            var logo = Image.Load<Rgba32>("Assets/Images/logo.png");
            logo.DangerousTryGetSinglePixelMemory(out var mem);
            var logoBytes = MemoryMarshal.AsBytes(mem.Span).ToArray();

            Window.Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new OpenTK.Windowing.Common.Input.Image(logo.Width, logo.Height, logoBytes));
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Multisample);
            GL.Disable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Less);
            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
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
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, 1);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void OnWindowResize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
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