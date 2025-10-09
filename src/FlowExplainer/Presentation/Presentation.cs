namespace FlowExplainer;

public abstract class Presentation
{
    public PresiContext Presi { get; set; }
    public abstract Slide[] GetSlides();
    public abstract void Setup(FlowExplainer flowExplainer);
    public virtual void Prepare(FlowExplainer flowExplainer)
    {
        foreach (var slide in GetSlides())
        {
            slide.Prepare(flowExplainer);
        }
    }
}