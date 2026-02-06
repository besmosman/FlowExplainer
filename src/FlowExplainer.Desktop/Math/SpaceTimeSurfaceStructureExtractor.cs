using ImGuiNET;

namespace FlowExplainer;


public class TrajectoryComparison : WorldService
{



    public override void Initialize()
    {

    }

    public Trajectory<Vec3> True;
    public Trajectory<Vec3> Ficticious;

    public override void Draw(View view)
    {
        var TotalFlux = DataService.LoadedDataset.VectorFields["Total Flux"];
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        var u = new ArbitraryField<Vec3, Vec2>(TotalFlux.Domain, p => TotalFlux.Evaluate(p) / ConvectiveTemp.Evaluate(p));

        var t_start = 1.0;
        var t_end = 1.3;

        True = IFlowOperator<Vec2, Vec3>.Default.ComputeTrajectory(t_start, t_end, view.MousePosition, u);
        // Ficticious = IFlowOperator<Vec2, Vec3>.Default.ComputeTrajectory(t_start, t_end, new Vec2(.5, .3f), u);



        foreach (ref var p in True.Entries.AsSpan())
        {
            Gizmos2D.Instanced.RegisterCircle(p.XY, 0.001f, Color.Red);
        }

        int steps = 0;
        var t = t_start;
        var fakeTime = 0;
        var pos = view.MousePosition;
        while (steps < 1000 && t < t_end)
        {
            var Q = TotalFlux.Evaluate(pos.Up(t));
            double T = ConvectiveTemp.Evaluate(pos.Up(t));
            var dt = double.Sign(T) * .1f;
            pos += Q * dt;
            t += T * dt;
            steps++;
        }
        Gizmos2D.Instanced.RegisterCircle(pos, 0.001f, t >= t_end ? Color.Green : Color.Blue);
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }
}

public class SpaceTimeSurfaceStructureExtractor : WorldService
{
    public IVectorField<Vec2, double> ScalerField;
    public double TargetValue => -0.0;
    public override string? Name => "Spacetime Surface Extractor";


    public class Ending
    {
        public bool IsStart;
        public Node AttachedPriorNode;
        public Node AttachedNode;
        public Node TemporaryNode;
        public double RestDistance;
    }

    public class Structure
    {
        public List<Node> InternalNodes = new List<Node>();
        public double TimeSinceLastExtention = 0;
        public bool Expanding = true;

        public Ending SnakeStart;
        public Ending SnakeEnd;

        public IEnumerable<Ending> GetEndings()
        {
            yield return SnakeStart;
            yield return SnakeEnd;
        }

        public IEnumerable<Node> GetAllNodes()
        {
            foreach (var n in InternalNodes)
                yield return n;

            yield return SnakeStart.TemporaryNode;
            yield return SnakeEnd.TemporaryNode;
        }

        public IEnumerable<(Node, Node)> GetNoneEndingNeighbors()
        {
            for (int i = 0; i < InternalNodes.Count - 1; i++)
            {
                yield return (InternalNodes[i], InternalNodes[i + 1]);
            }
            // yield return (Nodes[^1], Nodes[0]);
        }

        public IEnumerable<(Node, Node)> GetIndirectNeighbors()
        {
            for (int i = 0; i < InternalNodes.Count - 2; i++)
            {
                yield return (InternalNodes[i], InternalNodes[i + 2]);
            }
            // yield return (Nodes[^1], Nodes[0]);
        }
    }


    public class Node
    {
        public Vec2 Position;
        public Vec2 LastPosition;
        public Vec2 Velocity;

        public Node(Vec2 position)
        {
            Position = position;
            LastPosition = position;
        }
    }

    public List<Structure> Structures = new();
    public override void Initialize()
    {
        ScalerField = World.GetSelectableVectorFields<Vec2, double>().First().VectorField;

        Structures.Clear();
        var domain = ScalerField.Domain.RectBoundary;
        for (int i = 0; i < 1; i++)
        {
            var test = new Structure();

            var position = Utils.Random(domain);
            position = new Vec2(.2f, .24f);
            test.InternalNodes.Add(new Node(position));
            test.InternalNodes.Add(new Node(position + new Vec2(0.0001f, 0)));

            test.SnakeStart = new Ending()
            {
                TemporaryNode = new Node(position),
                AttachedNode = test.InternalNodes[0],
                AttachedPriorNode = test.InternalNodes[1],
                IsStart = true,
            };


            test.SnakeEnd = new Ending()
            {
                TemporaryNode = new Node(position),
                AttachedNode = test.InternalNodes[1],
                AttachedPriorNode = test.InternalNodes[0],
                IsStart = false,
            };
            Structures.Add(test);
        }
    }

    public float MaxRestDistance => .02f;

    public override void Draw(View view)
    {
        ScalerField = World.GetSelectableVectorFields<Vec2, double>().First().VectorField;
        var domain = ScalerField.Domain;

        var dt = .1f;
        /*foreach (var s in Structures)
        {
            s.TimeSinceLastExtention += FlowExplainer.DeltaTime;
            if (s.TimeSinceLastExtention > .15f)
            {
                var position = s.InternalNodes.Last().Position;
                var grad = ScalerField.FiniteDifferenceGradient(position, .001f).Normalized();
                grad = -new Vec2(grad.Y, grad.X);
                s.InternalNodes.Add(new Node(position + grad * .01f));
                s.TimeSinceLastExtention = 0;
            }
        }*/
        var rect = new Rect<Vec2>(Vec2.Zero, new Vec2(1, .5f));

        for (int i = 0; i < 1; i++)
        {
            foreach (var s in Structures)
            {
                foreach (var n in s.GetAllNodes())
                {
                    n.Position += n.Velocity * dt;

                    var grad = ScalerField.FiniteDifferenceGradient(n.Position, .0001f).NormalizedSafe();
                    var distanceToTarget = TargetValue - ScalerField.Evaluate(n.Position);
                    var C = distanceToTarget;
                    ApplyConstraint(n, 0.04f, -grad, C);
                }

                if (s.InternalNodes.Count > 3)
                {
                    var f1 = -(s.InternalNodes[2].LastPosition - s.InternalNodes[0].LastPosition).NormalizedSafe() * 0.2f * dt;
                    var f2 = (s.InternalNodes[^1].LastPosition - s.InternalNodes[^3].LastPosition).NormalizedSafe() * 0.2f * dt;
                    // s.InternalNodes[1].Position -= f2;
                    // s.InternalNodes[^2].Position -= f1;

                    s.InternalNodes[0].Position += f1;
                    s.InternalNodes[^1].Position += f2;
                }

                if (s.Expanding)
                    foreach (var ending in s.GetEndings())
                    {
                        var validDir = Vec2.Distance(ending.AttachedPriorNode.Position, ending.TemporaryNode.Position) > MaxRestDistance * 1.8f;

                        var snakingNode = ending.TemporaryNode;
                        if (ending.RestDistance < MaxRestDistance)
                        {
                            ending.RestDistance += FlowExplainer.DeltaTime / 20f;
                        }
                        else
                        {
                            var validScaler = double.Abs(ScalerField.Evaluate(ending.TemporaryNode.Position) - TargetValue) < .1f;
                            var validBounds = rect.Contains(ending.TemporaryNode.Position) && ending.TemporaryNode.Position.Y > 0.01f;

                            if (validDir && validBounds && validScaler)
                            {
                                ending.AttachedPriorNode = ending.AttachedNode;
                                ending.AttachedNode = ending.TemporaryNode;
                                if (ending.IsStart)
                                    s.InternalNodes.Insert(0, ending.TemporaryNode);
                                else
                                    s.InternalNodes.Add(ending.TemporaryNode);
                                ending.TemporaryNode = new Node(ending.TemporaryNode.Position);
                                ending.RestDistance = 0;
                            }
                        }
                        ending.RestDistance = double.Min(ending.RestDistance, MaxRestDistance);
                        var pushDirection = (ending.AttachedNode.Position - ending.AttachedPriorNode.Position).NormalizedSafe();
                        // ending.TemporaryNode.Position = ending.AttachedNode.Position - new Vec2(double.Cos(FlowExplainer.Time.TotalSeconds * 40), double.Sin(FlowExplainer.Time.TotalSeconds * 40)) * ending.RestDistance;
                        var grad = ScalerField.FiniteDifferenceGradient(ending.TemporaryNode.Position, .0001f).NormalizedSafe();

                        var dir = new Vec2(-grad.Y, grad.X);
                        if (Vec2.Dot(dir.NormalizedSafe(), pushDirection) < 0)
                            dir *= -1;

                        ending.TemporaryNode.Position = ending.AttachedNode.Position + dir * ending.RestDistance;
                        ending.TemporaryNode.Position = ending.AttachedNode.Position - new Vec2(double.Cos(FlowExplainer.Time.TotalSeconds * 40), double.Sin(FlowExplainer.Time.TotalSeconds * 40)) * ending.RestDistance;
                        // if (validDir)
                        //     Gizmos2D.Instanced.RegisterCircle(ending.TemporaryNode.Position, .01f, Color.White);


                        /*var dis = Vec2.Distance(ending.AttachedPriorNode.Position, ending.TemporaryNode.Position);
                        if (dis < MaxRestDistance)
                            ending.TemporaryNode.Position += pushDirection * .1f * dt;*/


                    }
            }


            foreach (var s in Structures)
            {
                /*for (int i = 1; i < s.Nodes.Count - 1; i++)
                {
                    var cur = s.Nodes[i];
                    var prev = s.Nodes[i + 1];
                    var next = s.Nodes[i - 1];

                    PointToSegment2D(cur.Position, prev.Position, next.Position, out var direction, out var closestPoint);

                    var gradient = direction;
                    var compliance = 2.1;
                    var a_ = compliance / (dt * dt);
                    var C = Vec2.Distance(closestPoint, direction);
                    var w = 1;
                    var deltaLambda = -C / (w + a_);
                    cur.Position += gradient * deltaLambda;
                }*/
            }

            void ApplyConstraint(Node node, double compliance, Vec2 grad, double C)
            {
                var a_ = compliance / (dt * dt);
                var deltaLambda = -C / (1 + a_);
                node.Position += grad * deltaLambda;
            }

            foreach (var s in Structures)
            {
                foreach (var con in s.GetNoneEndingNeighbors())
                {
                    var a = con.Item1;
                    var b = con.Item2;
                    var gradient = (a.Position - b.Position).NormalizedSafe();
                    var C = Vec2.Distance(a.Position, b.Position) - MaxRestDistance;
                    ApplyConstraint(a, 0.01, gradient, C);
                    ApplyConstraint(b, 0.01, -gradient, C);
                }

                foreach (var con in s.GetIndirectNeighbors())
                {
                    var a = con.Item1;
                    var b = con.Item2;
                    var gradient = (a.Position - b.Position).NormalizedSafe();
                    var C = Vec2.Distance(a.Position, b.Position) - MaxRestDistance * 2;
                    // ApplyConstraint(a, 0.01, gradient, C);
                    // ApplyConstraint(b, 0.01, -gradient, C);
                }
                foreach (var ending in s.GetEndings())
                {
                    var a = ending.TemporaryNode;
                    var b = ending.AttachedNode;
                    var gradient = (a.Position - b.Position).NormalizedSafe();
                    var C = Vec2.Distance(a.Position, b.Position) - ending.RestDistance;
                    //ApplyConstraint(a, 0.01, gradient, C);
                    //ApplyConstraint(b, 0.1, -gradient, C);

                }
            }
            foreach (var s in Structures)
            {
                foreach (var n in s.InternalNodes)
                {
                    //n.Velocity = (n.Position - n.LastPosition) / (dt);
                    //n.Velocity /= 2f;
                    n.Position = rect.Clamp(n.Position);
                    n.LastPosition = n.Position;
                }
            }
        }
        foreach (var s in Structures)
        {
            foreach (var con in s.GetNoneEndingNeighbors())
            {
                Gizmos2D.Instanced.RegisterLine(con.Item1.Position, con.Item2.Position, Color.Blue, .003f);
            }
        }

        foreach (var s in Structures)
        {

            if (s.Expanding)
                foreach (var n in s.GetEndings())
                {
                    Gizmos2D.Instanced.RegisterCircle(n.TemporaryNode.Position, .005f, Color.Red);
                    Gizmos2D.Instanced.RegisterLine(n.TemporaryNode.Position, n.AttachedNode.Position, Color.Red, .001f);
                }
            foreach (var n in s.InternalNodes)
            {
                Gizmos2D.Instanced.RegisterCircle(n.Position, .005f, Color.Blue);
            }
        }



        Gizmos2D.Instanced.RenderRects(view.Camera2D);
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Reset"))
        {
            Initialize();
        }

        if (ImGui.Button("Stop Expanding"))
        {
            foreach (var s in Structures)
            {
                s.Expanding = false;
            }
        }
        base.DrawImGuiSettings();
    }

    //source gpt:
    public static double PointToSegment2D(
        Vec2 p,
        Vec2 a,
        Vec2 b,
        out Vec2 direction, // from segment toward point
        out Vec2 closestPoint
    )
    {
        Vec2 ab = b - a;
        var abLenSq = Vec2.Dot(ab, ab);

        // Degenerate segment
        if (abLenSq < 1e-14f)
        {
            closestPoint = a;
            Vec2 diff = p - a;
            var dist = diff.Length();
            direction = dist > 0 ? diff * (1.0f / dist) : new Vec2(0, 0);
            return dist;
        }

        var t = Vec2.Dot(p - a, ab) / abLenSq;
        t = Math.Clamp(t, 0f, 1f);

        closestPoint = a + ab * t;

        Vec2 d = p - closestPoint;
        var distance = d.Length();
        direction = distance > 0 ? d * (1.0f / distance) : new Vec2(0, 0);

        return distance;
    }
}