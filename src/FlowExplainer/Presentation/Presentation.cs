namespace FlowExplainer;

public abstract class Presentation
{
    public PresiContext Presi { get; set; }
    public abstract Slide[] GetSlides();
    public abstract void Setup(FlowExplainer flowExplainer);
}