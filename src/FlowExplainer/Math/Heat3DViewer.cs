using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Heat3DViewer : WorldService
{
    private List<(Vec3, float h)> particles = new();
    private RegularGridVectorField<Vec3, Vec3i, float> heat3d;
    private StorageBuffer<float> StorageBuffer;
    public override ToolCategory Category => ToolCategory.Heat;

    private Material mat = new Material(Shader.DefaultWorldSpaceVertex, new Shader("Assets/Shaders/volume.frag", ShaderType.FragmentShader));
    public float zScale =2;
    public float depthScaling = 50;
    public override void Initialize()
    {
        //   heat3d = HeatSimulationToField.Convert(BinarySerializer.Load<HeatSimulation>("heat.sim"));
        //heat3d = SpeetjensSpectralImporter.Load("C:\\Users\\osman\\Downloads\\ScalarTransportBasicVersion\\ScalarTransportBasicVersion\\DataSet1");
        //heat3d = SpeetjensSpectralImporter.Load(Config.GetValue<string>("spectral-data-path"));
       // heat3d = GetRequiredWorldService<DataService>().TempratureField;
        StorageBuffer = new StorageBuffer<float>(heat3d.GridSize.Volume());
        StorageBuffer.Data = heat3d.Grid.Data;
    }
    private object lastVelField;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;
        if (lastVelField != GetRequiredWorldService<DataService>().VelocityField)
        {
            Initialize();
            lastVelField = GetRequiredWorldService<DataService>().VelocityField;
        }
        // Update();

        var grad = GetRequiredWorldService<DataService>().ColorGradient;

        float c = (MathF.Sin((float)FlowExplainer.Time.TotalSeconds * .8f) + 1) / 2f;

        var th = .02f;

        //GL.Enable(EnableCap.DepthTest);
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(1, 0, 0), th, new Color(1, 0, 0));
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(0, 1, 0), th, new Color(0, 1, 0));
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(0, 0, 1), th, new Color(0, 0, 1));
        
        /*GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(false);*/
        
        /*GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);*/
        mat.Use();
        StorageBuffer.Use();
        StorageBuffer.Upload();
        mat.SetUniform("tint", Color.White);
        mat.SetUniform("cameraPosUni", view.Camera.Position);
        var heat3dMaxCellPos = heat3d.RectDomain.MaxPos;
        var heat3dMinCellPos = heat3d.RectDomain.MinPos;
        heat3dMinCellPos.Z = 0;
        heat3dMaxCellPos.Z = zScale;
        view.CameraOffset = -(heat3dMaxCellPos + heat3dMinCellPos)/2;
        mat.SetUniform("volumeMin", heat3dMinCellPos);
        mat.SetUniform("heatFilterMin",heatfilterTemp - heatfilterSize/2);
        mat.SetUniform("heatFilterMax",heatfilterTemp + heatfilterSize/2);
        mat.SetUniform("volumeMax", heat3dMaxCellPos);
        mat.SetUniform("depthScaling", depthScaling);
        mat.SetUniform("gridSize", heat3d.GridSize.ToVec3());
        mat.SetUniform("view", view.Camera.GetViewMatrix());
        mat.SetUniform("projection", view.Camera.GetProjectionMatrix());
        var size = heat3dMaxCellPos - heat3dMinCellPos;
        mat.SetUniform("model", Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(size / 2));
        mat.SetUniform("colorgradient", GetRequiredWorldService<DataService>().ColorGradient.Texture.Value);
        Gizmos.UnitCube.Draw();
    }

    private float heatfilterSize = 2;
    private float heatfilterTemp = 1;
    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Filter Radius", ref heatfilterSize, 0, 2);
        ImGuiHelpers.SliderFloat("Filter Temperature", ref heatfilterTemp, 0, 2);
        ImGuiHelpers.SliderFloat("z Scale", ref zScale, 1, 10);
        ImGuiHelpers.SliderFloat("Depth Scaling", ref depthScaling, 1, 10_0);
        base.DrawImGuiEdit();
    }
    /*private void Update()
    {
        particles.Clear();
        //thresh *= .001f;
        for (int s = 1; s < loaded.Value.States.Length - 1; s++)
        {
            var states = loaded.Value.States;
            var state = states[s];
            for (int i = 0; i < state.ParticleHeat.Length; i++)
            {
                var flux = (states[s].ParticleHeat[i] - states[s - 1].ParticleHeat[i]) / (states[s].Time - states[s - 1].Time);
                var fluxNext = (states[s + 1].ParticleHeat[i] - states[s].ParticleHeat[i]) / (states[s + 1].Time - states[s].Time);
                var thresh = .8f;

                if (flux >= thresh
                   )
                {
                    particles.Add((new Vec3(states[s].ParticleX[i], states[s].ParticleY[i], states[s].Time), flux));
                }
            }
        }
    }*/
}