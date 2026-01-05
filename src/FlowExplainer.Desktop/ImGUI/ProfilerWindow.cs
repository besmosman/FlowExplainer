using ImGuiNET;

namespace FlowExplainer;

public static class ProfilerWindow
{
    public class ProfilerEntry
    {
        public bool IsOptional;
        public string Category;
        public TimeSpan StartTime;
        public TimeSpan Duration;
        public ProfilerEntry Parent;
        public List<ProfilerEntry> Children;
    }

    private static List<int> Entries = new();
    private static Dictionary<int, float> MovingAverage = new();
    private static Dictionary<int, CircleBuffer<float>> ProfilerBuffer = new();

    public class CircleBuffer<T>
    {
        public static int Count = 64;
        public T[] values = new T[Count];
        int index = 0;

        public void Push(T value)
        {
            values[index] = value;
            index++;
            if (index == Count)
                index = 0;
        }
    }

    public static void ProfilerTab()
    {
        Entries.Clear();

        var seen = new HashSet<string>();
        if (ImGui.Checkbox("Profiler Enabled", ref Profiler.IsRunning))
        {
            if (Profiler.IsRunning)
            {
                
            }   
        }

        Profiler.Begin("Profiler");
        Profiler.Begin("Profiler compute");
        for (int i = 0; i < Profiler.CurrentEntries; i++)
        {
            var e = Profiler.Entries[i];
            if (e.Type == Profiler.EntryType.Finished)
            {
                int hash = GetHash(e);
                if (MovingAverage.TryGetValue(hash, out float avg))
                    MovingAverage[hash] = Utils.Lerp(avg, (float)e.Duration.TotalMilliseconds, .1f);
                else
                    MovingAverage.Add(hash, (float)e.Duration.TotalMilliseconds);

                if (!ProfilerBuffer.ContainsKey(hash))
                    ProfilerBuffer.Add(hash, new());

                ProfilerBuffer[hash].Push((float)e.Duration.TotalMilliseconds);
            }
        }

        Profiler.End("Profiler compute");
        for (int i = 0; i < Profiler.CurrentEntries; i++)
        {
            var e = Profiler.Entries[i];
            if (e.Type != Profiler.EntryType.Finished || e.Depth != 0)
            {
                continue;
            }

            Entries.Add(i);
        }

        Profiler.Begin("Method calling");
        foreach (int i in Entries.OrderBy(e => Profiler.Entries[e].Category).DistinctBy(e => Profiler.Entries[e].Category))
        {
            Render(Profiler.Entries, i, null);
        }

        Profiler.End("Method calling");

#if DEBUGs
            foreach (var e in Profiler.ParentLessEntries.OrderBy(e => e.Category))
                if (!seen.Contains(e.Category))
                {
                    Render(e);
                    seen.Add(e.Category);
                }
#endif
        Profiler.End("Profiler");
    }

    private static int GetHash(Profiler.Entry e)
    {
        return HashCode.Combine(e.Category.GetHashCode(), e.Depth.GetHashCode(), e.EntriesCount, e.Type);
    }

    private static void Render(Profiler.Entry[] entries, int index, TimeSpan? parentDuration)
    {
        var e = entries[index];
        float avg = 0f;
        float max = 0f;
        foreach (float v in ProfilerBuffer[GetHash(e)].values)
        {
            avg += v;
            if (v > max)
                max = v;
        }

        avg /= CircleBuffer<float>.Count;

        float t = 0;
        if (parentDuration.HasValue)
            t = (float)e.Duration.TotalMilliseconds / (float)parentDuration.Value.TotalMilliseconds;

        //ImGui.PushStyleColor(ImGuiCol.Text, Utils.Lerp(Color.Grey(1f), Color.Red, t).ToNumerics());
        string avgText = Math.Round(avg, 2).ToString("N2");
        string maxText = Math.Round(max, 1).ToString("N1");

        var x = ImGui.GetCursorPosX();
        bool treeNode = ImGui.TreeNode(e.Category, $"{t * 100:00}%% {avgText}ms {maxText}ms ");
        ImGui.SameLine();
        ImGui.SetCursorPosX(x + 230);
        ImGui.Text(e.Category);
        if (treeNode)
        {
            var sum = e.Duration;
            for (int i = index + 1; i < index + e.EntriesCount; i++)
            {
                if (entries[i].Depth == e.Depth + 1)
                    sum -= entries[i].Duration;
            }

            //ImGui.PushStyleColor(ImGuiCol.Text, Color.White.ToNumerics());

            //if (sum.TotalMilliseconds < .1f)
            //   ImGui.PushStyleColor(ImGuiCol.Text, Color.Grey(.5f).ToNumerics());

            ImGui.TreeNode(e.Category + "sum", Math.Round(sum.TotalMilliseconds, 2).ToString("N2") + "ms " + "Not profiled");
            //ImGui.PopStyleColor();
            for (int i = index + 1; i < index + e.EntriesCount; i++)
            {
                if (entries[i].Depth == e.Depth + 1)
                    Render(entries, i, e.Duration);
            }

            ImGui.TreePop();
        }
        //ImGui.PopStyleColor();

        //ImGui.PushStyleColor(ImGuiCol.Text, Color.White.ToNumerics());
    }
}