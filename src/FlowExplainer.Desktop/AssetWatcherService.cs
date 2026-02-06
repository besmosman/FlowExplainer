namespace FlowExplainer;

public class AssetWatcherService : GlobalService
{
    public override void Initialize()
    {
    }

    public override void Draw()
    {
        AssetWatcher.Execute();
    }
}