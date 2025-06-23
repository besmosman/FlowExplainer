namespace FlowExplainer;

public abstract class Slide
{
    public PresiContext Presi = null!;

    public bool OverrideNextSlideAction = false;

    public virtual void Draw()
    {
    }

    public virtual void Next()
    {
    }

    public virtual void Load()
    {
    }

    public virtual void OnLeave()
    {
    }

    public virtual void OnEnter()
    {
    }
}