using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer
{
    /// <summary>
    /// Don't implement directly. Use <see cref="VisualisationService"/> or <see cref="GlobalService"/>.
    /// </summary>
    public abstract class Service
    {
        public FlowExplainer FlowExplainer { get; internal set; } = null!;

        public T? GetGlobalService<T>() where T : GlobalService => FlowExplainer.GetGlobalService<T>();
        public T GetRequiredGlobalService<T>() where T : GlobalService => FlowExplainer.GetGlobalService<T>() ?? throw new Exception($"{typeof(T)} service not found.");
        public bool TryGetGlobalService<T>([NotNullWhen(true)] out T? service) where T : GlobalService
        {
            service = GetGlobalService<T>();
            return service != null;
        }

        /// <summary>
        /// Called once when the service is added to <see cref="FlowExplainer.Services"/>.
        /// </summary>
        public abstract void Initialize();
        public virtual void Deinitialize() { }
    }
}