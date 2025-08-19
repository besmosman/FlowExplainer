namespace FlowExplainer;

public class PreferencesService : GlobalService
{
    public override void Initialize()
    {
    }


    private bool taskDispatched;
    private TimeSpan lastSaveTime = TimeSpan.Zero;

    public override void Draw()
    {
        if (Config.IsDirty && !taskDispatched && (FlowExplainer.Time - lastSaveTime).TotalSeconds > .5)
        {
            taskDispatched = true;
            Task.Run(() =>
            {
                Config.Save();
                taskDispatched = false;
                Config.IsDirty = false;
                lastSaveTime = FlowExplainer.Time;
            });
        }
    }
}