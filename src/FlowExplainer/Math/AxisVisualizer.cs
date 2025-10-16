using System.Globalization;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Axis3D : WorldService
{
    public override void Initialize()
    {

    }
    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
        {
            var dat = GetRequiredWorldService<DataService>();
            var domain = dat.VectorField.Domain.RectBoundary;

            var th = 0.02f;
            GL.Enable(EnableCap.DepthTest);
            Gizmos.DrawLine(view, domain.Min, new Vec3(domain.Max.X, domain.Min.Y, domain.Min.Z), th, new Color(1, 0, 0, 1));
            Gizmos.DrawLine(view, domain.Min, new Vec3(domain.Min.X, domain.Max.Y, domain.Min.Z), th, new Color(0, 1, 0, 1));
            Gizmos.DrawLine(view, domain.Min, new Vec3(domain.Min.X, domain.Min.Y, domain.Max.Z), th, new Color(0, 0, 1, 1));
            GL.Disable(EnableCap.DepthTest);

        }

    }
}

public class AxisVisualizer : WorldService
{
    public bool DrawAxis = true;
    public bool DrawWalls = false;
    public int StepsX = 5;
    public int StepsY = 5;
    public bool DrawGradient = true;

    public string? Title;
    public IAxisTitle? titler;
    public IGradientScaler? scaler;

    Mesh gradientMesh = new Mesh(new Geometry(
    [
        new Vertex(new Vec3(0f, 0f, 0), new Vec2(1, 0), Vec4.One),
        new Vertex(new Vec3(1f, 0f, 0), new Vec2(1, 1), Vec4.One),
        new Vertex(new Vec3(1f, 1f, 0), new Vec2(0, 1), Vec4.One),
        new Vertex(new Vec3(0f, 1f, 0), new Vec2(0, 0), Vec4.One),
    ], [0, 1, 2, 0, 2, 3]));

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if(!view.Is2DCamera)
            return;
        var dat = GetRequiredWorldService<DataService>();
        var domain = dat.VectorField.Domain.RectBoundary;

        var color = Style.Current.TextColor;
        var thickness = 4f;
        var margin = 0f;

        var lh = view.Width / 26f;
        if (DrawAxis)
        {
            var lb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Min.Y));
            var rb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Max.X, domain.Min.Y));
            var lt = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Max.Y));
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(-thickness / 2, margin), rb + new Vec2(0, margin), color, thickness);
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(0, margin), rb + new Vec2(0, margin), color, thickness);
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(-margin, 0), lt + new Vec2(-margin, 0), color, thickness);

            //if (titler != null)
            //    Gizmos2D.Text(view.ScreenCamera, new Vec2((lb.X + rb.X) / 2, lt.Y - lh * 2), lh, color, titler.GetTitle(), centered: true);

            if (!GetGlobalService<PresentationService>().IsPresenting)
            {

                string title = "";
                var gridVisualizer = GetWorldService<GridVisualizer>();
                if (gridVisualizer.IsEnabled)
                {
                    title += $"{gridVisualizer.GetTitle()} ({dat.currentSelectedScaler.Replace("Temperature2", "").Trim()})";
                }

                if (GetWorldService<FlowFieldVisualizer>().IsEnabled || GetWorldService<FlowDirectionVisualization>().IsEnabled)
                {
                    title += $" + {dat.currentSelectedVectorField.Replace("Temperature", "").Trim()} field";
                }

                Gizmos2D.Text(view.ScreenCamera, new Vec2((lb.X + rb.X) / 2, lt.Y - lh * 2), lh, color, title, centered: true);
            }
            for (int i = 0; i <= StepsX; i++)
            {
                float c = i / (float)StepsX;
                var value = Utils.Lerp(domain.Min.X, domain.Max.X, c);
                var pos = Utils.Lerp(lb, rb, c);
                Gizmos2D.Line(view.ScreenCamera, pos + new Vec2(0, 15), pos + new Vec2(0, -thickness / 2f), color, thickness);
                Gizmos2D.Text(view.ScreenCamera, pos + new Vec2(0, 10), lh, color, value.ToString("N1"), centered: true);
            }

            for (int i = 0; i <= StepsY; i++)
            {
                float c = i / (float)StepsY;
                var value = Utils.Lerp(domain.Min.Y, domain.Max.Y, c);
                var pos = Utils.Lerp(lb, lt, c);
                Gizmos2D.Line(view.ScreenCamera, pos + new Vec2(-15, 0), pos + new Vec2(thickness / 2f, 0), color, thickness);
                Gizmos2D.Text(view.ScreenCamera, pos + new Vec2(-10 - lh * 1, -lh / 2f), lh, color, value.ToString("N1"), centered: true);
            }
        }


        if (DrawGradient && scaler != null)
        {
            var textr = dat.ColorGradient.Texture.Value;
            //Gizmos2D.ImageCentered(view.ScreenCamera, textr, new Vec2(view.Width-50f, 50), 10000f, 1);
            var texturedMat = Gizmos2D.texturedMat;
            texturedMat.Use();
            texturedMat.SetUniform("tint", new Vec4(1, 1, 1, 1));
            texturedMat.SetUniform("view", view.ScreenCamera.GetViewMatrix());
            texturedMat.SetUniform("projection", view.ScreenCamera.GetProjectionMatrix());
            texturedMat.SetUniform("mainTex", textr);
            var width = 40;
            var height = 200;
            var posX = view.Width - width - 50f;
            var posY = view.Height / 2f - height / 2f;
            texturedMat.SetUniform("model", Matrix4x4.CreateScale(width, height, .4f) * Matrix4x4.CreateTranslation(posX, posY, 0));
            gradientMesh.Draw();
            (float min, float max) = scaler.GetScale();

            Gizmos2D.Text(view.ScreenCamera, new Vec2(posX + width / 2f, posY - lh - 5), lh, color, max.ToString("F2", CultureInfo.InvariantCulture), centered: true);
            Gizmos2D.Text(view.ScreenCamera, new Vec2(posX + width / 2f, posY + height + 5), lh, color, min.ToString("F2", CultureInfo.InvariantCulture), centered: true);
        }

        if (DrawWalls)
        {
            var thick = .012f;
            var off = -thick / 2;
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Max.Y + off + thick / 2), new Vec2(domain.Max.X, domain.Max.Y + off + thick / 2), dat.ColorGradient.Get(.00f), thick);
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Min.Y - off - thick / 2), new Vec2(domain.Max.X, domain.Min.Y - off - thick / 2), dat.ColorGradient.Get(1f), thick);
        }
    }

    public override void DrawImGuiEdit()
    {
        ImGui.Checkbox("Draw Axis", ref DrawAxis);
        ImGui.Checkbox("Draw Walls", ref DrawWalls);
        ImGui.Checkbox("Draw Gradient", ref DrawGradient);
        base.DrawImGuiEdit();
    }
}