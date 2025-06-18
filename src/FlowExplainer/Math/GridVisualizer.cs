using ImGuiNET;

namespace FlowExplainer;





public class GridVisualizer : WorldService
{
    struct GridData
    {
        public Vec2 Velocity;
        public float Heat;
        public float Vorticity;
    }

    private InterpolatedRenderGrid<GridData> renderGrid;

    public override void Initialize()
    {
        renderGrid = new InterpolatedRenderGrid<GridData>(new Vec2i(100, 50));
      
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        renderGrid.UpdateColorFunction(
            (gl, dat) => new Color(gl.floor(dat.Velocity.X*10)/10, dat.Velocity.Y, 0, 1));
        
        
        
        var dat = GetRequiredWorldService<DataService>();
        var domain = dat.VelocityField.Domain;
        
        for (int i = 0; i < renderGrid.GridSize.X; i++)
        {
            for (int j = 0; j < renderGrid.GridSize.Y; j++)
            {
                var vel = dat.VelocityField.Evaluate(new Vec3(new Vec2(i, j) / renderGrid.GridSize.ToVec2() * domain.Size, dat.SimulationTime)).Abs();

                renderGrid.AtCoords(new Vec2i(i, j)) = new GridData
                {
                    Velocity = vel,
                    Heat = .5f,
                };
            }
        }

        renderGrid.Draw(view.Camera2D, dat.VelocityField.Domain.Min, dat.VelocityField.Domain.Max);
    }

    public override void DrawImGuiEdit()
    {
        ImGui.Begin("Shader");
        ImGui.TextWrapped(renderGrid.material.Shaders[1].Content);
        ImGui.End();
        base.DrawImGuiEdit();
    }
}