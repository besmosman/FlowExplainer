namespace FlowExplainer;

interface ILine<T>
{
    IEnumerable<T> Points { get; }
}