using System.Collections.Concurrent;

namespace FlowExplainer;

public static class Rental<T>
{
    private static ConcurrentDictionary<int, ConcurrentStack<T[]>> stacks = new();

    public static T[] Rent(int length)
    {
        if (!stacks.TryGetValue(length, out var stack))
        {
            stack = new();
            stacks[length] = stack;
        }

        if (stack.TryPop(out var ts))
            return ts;

        return new T[length];
    }

    public static void Return(T[] array)
    {
        stacks[array.Length].Push(array);
    }
}