using MemoryPack;

namespace FlowExplainer;

[MemoryPackable]
public partial struct TrajectoryGroup<T> where T : IVec<T, double>
{
    public string DisplayName = "trajs";
    public Trajectory<T>[] Trajectories;
    public TrajectoryGroup(Trajectory<T>[] trajectories)
    {
        Trajectories = trajectories;
    }
}

public static class ArtifactSerializer
{

    public static void Save(IArtifact o, string path)
    {
        if (o.ValueObj is DiscretizedField<Vec2, Vec2i, Vec2> field)
        {
            field.GridField.RectDomain = new RectDomain<Vec2>(field.Domain.RectBoundary, GenBounding<Vec2>.None());
            field.DisplayName = o.DisplayName;
            field.GridField.Save(path + ".vec2_vec2_field");
        }
        else if (o.ValueObj is DiscretizedField<Vec2, Vec2i, double> sfield)
        {
            sfield.GridField.RectDomain = new RectDomain<Vec2>(sfield.Domain.RectBoundary, GenBounding<Vec2>.None());
            sfield.DisplayName = o.DisplayName;
            sfield.GridField.Save(path + ".vec2_vec1_field");
        }
        else if (o.ValueObj is TrajectoryGroup<Vec2> traj2d)
        {
            BinarySerializer.Save(path + ".vec2_traj", traj2d);
        }
        else
            throw new Exception();
    }

    public static IArtifact Load(string path)
    {
        var ext = Path.GetExtension(path);
        if (ext == ".vec2_vec2_field")
        {
            var regularGridVectorField = RegularGridVectorField<Vec2, Vec2i, Vec2>.Load(path);
            return new Artifact<IVectorField<Vec2, Vec2>>(regularGridVectorField, regularGridVectorField.DisplayName, "");
        }
        else if (ext == ".vec2_vec1_field")
        {
            var regularGridVectorField = RegularGridVectorField<Vec2, Vec2i, double>.Load(path);
            return new Artifact<IVectorField<Vec2, double>>(regularGridVectorField, regularGridVectorField.DisplayName, "");
        }
        else if (ext == ".vec2_traj")
        {
            var trajectoryGroup = BinarySerializer.Load<TrajectoryGroup<Vec2>>(path);
            for (int i = 0; i < trajectoryGroup.Trajectories.Length; i++)
            {
                var traj = trajectoryGroup.Trajectories[i];
                if (traj.Entries.Length > 1000)
                {
                    List<Vec2> points = new List<Vec2>();
                    for (int j = 0; j < traj.Entries.Length; j += 4)
                    {
                        points.Add(traj.Entries[j]);
                    }
                    trajectoryGroup.Trajectories[i] = new Trajectory<Vec2>(points.ToArray());
                }
            }
            return new Artifact<TrajectoryGroup<Vec2>>(trajectoryGroup, trajectoryGroup.DisplayName, "");
        }
        else
            throw new Exception();
    }
}