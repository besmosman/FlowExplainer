using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class AxisVisualizer : WorldService
{
    public bool DrawAxis = true;
    public bool DrawWalls = false;
    public int StepsX = 5;
    public int StepsY = 5;
    public bool DrawGradient = true;
    public bool DrawTitle = true;

    public string? Title;

    private Mesh gradientMesh;

    public override string? Name => "Axis";
    public override string? Description => "Render axis";
    public override string? CategoryN => "General";

    public override void Initialize()
    {
        gradientMesh = new Mesh(new Geometry(
        [
            new Vertex(new Vec3(0f, 0.0, 0), new Vec2(1, 0), Vec4.One),
            new Vertex(new Vec3(1f, 0.0, 0), new Vec2(1, 1), Vec4.One),
            new Vertex(new Vec3(1f, 1f, 0), new Vec2(0, 1), Vec4.One),
            new Vertex(new Vec3(0f, 1f, 0), new Vec2(0, 0), Vec4.One),
        ], [0, 1, 2, 0, 2, 3]));

    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
            return;
        var dat = GetRequiredWorldService<DataService>();
        var domain = dat.VectorField.Domain.RectBoundary;

        var color = Style.Current.TextColor;
        var thickness = 4f;
        var margin = 0.0;

        var lh = view.Width / 26f;
        if (DrawAxis)
        {
            var lb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Min.Y));
            var rb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Max.X, domain.Min.Y));
            var lt = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Max.Y));
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(-thickness / 2, margin), rb + new Vec2(0, margin), color, thickness);
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(0, margin), rb + new Vec2(0, margin), color, thickness);
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(-margin, 0), lt + new Vec2(-margin, 0), color, thickness);


            var y = lt.Y - lh * 2;
            if (DrawTitle)
                for (int index = World.Services.Count - 1; index >= 0; index--)
                {
                    var service = World.Services[index];
                    if (service is IAxisTitle titler && service.IsEnabled)
                    {
                        Gizmos2D.Text(view.ScreenCamera, new Vec2((lb.X + rb.X) / 2, y), lh, color, titler.GetTitle(), centered: true);
                        y -= lh;
                    }
                }
            //if (titler != null)

            /*if (!GetGlobalService<PresentationService>().IsPresenting)
            {

                string title = "";
                var gridVisualizer = GetWorldService<GridVisualizer>();
                if (gridVisualizer.IsEnabled)
                {
                    title += $"{gridVisualizer.GetTitle()} ({dat.currentSelectedScaler.Replace("Temperature2", "").Trim()})";
                }

                if (GetWorldService<ArrowVisualizer>().IsEnabled || GetWorldService<FlowDirectionVisualization>().IsEnabled)
                {
                    title += $" + {dat.currentSelectedVectorField.Replace("Temperature", "").Trim()} field";
                }

                Gizmos2D.Text(view.ScreenCamera, new Vec2((lb.X + rb.X) / 2, lt.Y - lh * 2), lh, color, title, centered: true);
            }*/
            for (int i = 0; i <= StepsX; i++)
            {
                double c = i / (double)StepsX;
                var value = Utils.Lerp(domain.Min.X, domain.Max.X, c);
                var pos = Utils.Lerp(lb, rb, c);
                Gizmos2D.Line(view.ScreenCamera, pos + new Vec2(0, 15), pos + new Vec2(0, -thickness / 2f), color, thickness);
                Gizmos2D.Text(view.ScreenCamera, pos + new Vec2(0, 10), lh, color, value.ToString("N1"), centered: true);
            }

            for (int i = 0; i <= StepsY; i++)
            {
                double c = i / (double)StepsY;
                var value = Utils.Lerp(domain.Min.Y, domain.Max.Y, c);
                var pos = Utils.Lerp(lb, lt, c);
                Gizmos2D.Line(view.ScreenCamera, pos + new Vec2(-15, 0), pos + new Vec2(thickness / 2f, 0), color, thickness);
                Gizmos2D.Text(view.ScreenCamera, pos + new Vec2(-10 - lh * 1, -lh / 2f), lh, color, value.ToString("N1"), centered: true);
            }
        }


        if (DrawGradient)
            for (int index = World.Services.Count - 1; index >= 0; index--)
            {
                var service = World.Services[index];
                if (service is IGradientScaler scaler && service.IsEnabled)
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
                    (double min, double max) = scaler.GetScale();

                    Gizmos2D.Text(view.ScreenCamera, new Vec2(posX + width / 2f, posY - lh - 5), lh, color, max.ToString("F2", CultureInfo.InvariantCulture), centered: true);
                    Gizmos2D.Text(view.ScreenCamera, new Vec2(posX + width / 2f, posY + height + 5), lh, color, min.ToString("F2", CultureInfo.InvariantCulture), centered: true);
                }
            }

        if (DrawWalls)
        {
            var thick = .012f;
            var off = -thick / 2;
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Max.Y + off + thick / 2), new Vec2(domain.Max.X, domain.Max.Y + off + thick / 2), dat.ColorGradient.Get(.00f), thick);
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Min.Y - off - thick / 2), new Vec2(domain.Max.X, domain.Min.Y - off - thick / 2), dat.ColorGradient.Get(1f), thick);
        }
    }

    public override void DrawImGuiSettings()
    {
        ImGui.Checkbox("Draw Axis", ref DrawAxis);
        ImGui.Checkbox("Draw Walls", ref DrawWalls);
        ImGui.Checkbox("Draw Gradient", ref DrawGradient);
        base.DrawImGuiSettings();
    }
}