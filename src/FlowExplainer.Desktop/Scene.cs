namespace FlowExplainer;

public abstract class Scene
{
    public virtual string Name => this.GetType().Name.Replace("Scene", "");
    public abstract void Load(FlowExplainer flowExplainer);
}