namespace FlowExplainer;

public class TestPresentation : Presentation
{
    public class TitleSlide : Slide
    {
        public override void Draw()
        {
            LayoutTitle();
            TitleTitle("Demo Presentation", "Some subtext here..");
            base.Draw();
        }
    }
    
    public override Slide[] GetSlides()
    {
        return
        [
            new TitleSlide(),
        ];
    }
    public override void Setup(FlowExplainer flowExplainer)
    {
        
    }
}