namespace FlowExplainer
{
    public class ViewsService : GlobalService
    {
        public List<View> Views = new();

        public override void Draw()
        {
            if (!Views.Any(n => n.IsFullScreen))
                foreach (var view in Views)
                {
                    view.Controller.UpdateAndDraw(view);
                }

            for (int i = Views.Count - 1; i >= 0; i--)
            {
                if (!Views[i].IsOpen)
                    Views.RemoveAt(i);
            }
        }

        public View NewView()
        {
            var view = new View(1, 1, GetRequiredGlobalService<WorldManagerService>().Worlds[0]);
            Views.Add(view);
            view.CameraOffset = new Vec3(.5f, .25f, 0f);
            view.CameraOffset = new Vec3(0, 0, 0);
            view.CameraZoom = 5;
            Views[Views.Count - 1].Camera2D.Scale = 14f;

            view.Camera2D.Scale = 1000;
            view.Camera2D.Position = -new Vec2(.5f, .25f);
            return view;
        }


        public override void Initialize()
        {
            NewView();
        }
    }
}