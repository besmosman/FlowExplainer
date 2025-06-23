using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace FlowExplainer;

public class AxisVisualizer : WorldService
{
    public bool DrawAxis;
    public int StepsX = 5;
    public int StepsY = 10;
    public bool DrawGradient;

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
        if (!view.Is2DCamera)
            return;

        var dat = GetRequiredWorldService<DataService>();
        var domain = dat.VelocityField.Domain;

        var color = Color.White;
        var thickness = 4f;
        var margin = 0f;

        var lh = 36;
        if (DrawAxis)
        {
            var lb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Min.Y));
            var rb = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Max.X, domain.Min.Y));
            var lt = CoordinatesConverter2D.WorldToView(view, new Vec2(domain.Min.X, domain.Max.Y));
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(-thickness / 2, margin), rb + new Vec2(0, margin), color, thickness);
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(0, margin), rb + new Vec2(0, margin), color, thickness);
            Gizmos2D.Line(view.ScreenCamera, lb + new Vec2(-margin, 0), lt + new Vec2(-margin, 0), color, thickness);


            for (int i = 0; i <= 5; i++)
            {
                float c = i / 5f;
                var value = Utils.Lerp(domain.Min.X, domain.Max.X, c);
                var pos = Utils.Lerp(lb, rb, c);
                //Gizmos2D.Line(view.ScreenCamera, pos + new Vec2(0, 10), pos + new Vec2(0, -10), color, thickness);
                Gizmos2D.Text(view.ScreenCamera, pos + new Vec2(0, 10), lh, color, value.ToString("N1"), centered: true);
            }

            for (int i = 0; i <= 5; i++)
            {
                float c = i / 5f;
                var value = Utils.Lerp(domain.Min.Y, domain.Max.Y, c);
                var pos = Utils.Lerp(lb, lt, c);
                // Gizmos2D.Line(view.ScreenCamera, pos + new Vec2(-10, 0), pos + new Vec2(10,0), color, thickness);
                Gizmos2D.Text(view.ScreenCamera, pos + new Vec2(-10 - lh * 1, -lh / 2f), lh, color, value.ToString("N1"), centered: true);
            }
        }

        

        if (DrawGradient)
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
            var posX = view.Width - width - 20f;
            var posY = view.Height / 2f - height / 2f;
            texturedMat.SetUniform("model", Matrix4x4.CreateScale(width, height, .4f) * Matrix4x4.CreateTranslation(posX, posY, 0));
            gradientMesh.Draw();
            Gizmos2D.Text(view.ScreenCamera, new Vec2(posX + width / 2f, posY - lh - 5), lh, color, "1", centered: true);
            Gizmos2D.Text(view.ScreenCamera, new Vec2(posX + width / 2f, posY + height + 5), lh, color, "0", centered: true);
        }
    }

    public override void DrawImGuiEdit()
    {
        ImGui.Checkbox("Draw Axis", ref DrawAxis);
        ImGui.Checkbox("Draw Gradient", ref DrawGradient);
        base.DrawImGuiEdit();
    }
}

public class VelocityMagnitudeGridVisualizer : IGridVisual
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VelData
    {
        public float AverageVelocityMagnitude;
    }

    public float T = 1;

    public Type DataType => typeof(VelData);

    public void UpdateGridData(GridVisualizer vis)
    {
        var renderGrid = vis.GetRenderGrid<VelData>();

        var dat = vis.GetRequiredWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        renderGrid.SetColorFunction(
            (gl) => gl.ColorGradient(gl.Dat.AverageVelocityMagnitude));

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = new Vec2(i, j) / renderGrid.GridSize.ToVec2() * domain.Size;
            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VelocityField);
            renderGrid.AtCoords(new Vec2i(i, j)).AverageVelocityMagnitude = center.AverageAlong((prev, cur) => ((prev.XY - cur.XY) / (cur.Z - prev.Z)).Length());
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        ImGui.SliderFloat("T", ref T, -dat.VelocityField.Period * 4, dat.VelocityField.Period * 4);
    }
}