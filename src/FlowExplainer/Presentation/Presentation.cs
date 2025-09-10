namespace FlowExplainer;

public abstract class Presentation
{
    public abstract Slide[] GetSlides();
    public abstract void Setup(FlowExplainer flowExplainer);
}