namespace FlowExplainer;

public class SurfacesTest : WorldService
{

    public class Structure
    {
        public List<Vec2> points;
    }

    public Structure structure;

    public override void Initialize()
    {
        structure = new Structure()
        {
            points = new(),
        };

        for (int i = 0; i < 10; i++)
        {
            structure.points.Add(new Vec2(.2, .25 + i / 50f));
        }
    }
    public override void Draw(View view)
    {
        var transportField = GetRequiredWorldService<DataService>().VectorField;

        for (int i = 0; i < structure.points.Count; i++)
        {
            var c = structure.points[i].Up(0);
            var delta = 0.01f;

            var mag_pdx = transportField.Evaluate(c + new Vec3(delta, 0, 0)).Length();
            var mag_mdx = transportField.Evaluate(c - new Vec3(delta, 0, 0)).Length();
            var mag_pdy = transportField.Evaluate(c + new Vec3(0, delta, 0)).Length();
            var mag_mdy = transportField.Evaluate(c - new Vec3(0, delta, 0)).Length();

            var dx = FD.Derivative(mag_mdx, mag_pdx, delta);
            var dy = FD.Derivative(mag_mdy, mag_pdy, delta);

            Vec2 gradMag = new Vec2(dx, dy);

            var mag = transportField.Evaluate(c).Length();
            var target = 0;

            Vec2 force = -20 * (mag - target) * gradMag;

            if (i > 0)
                force += (structure.points[i] - structure.points[i - 1]) / Vec2.DistanceSquared(structure.points[i], structure.points[i - 1]);

            if (i < structure.points.Count - 1)
                force += (structure.points[i] - structure.points[i + 1]) / Vec2.DistanceSquared(structure.points[i], structure.points[i + 1]);

            structure.points[i] += force * .0001f;

        }

        // structure.points[0] = new Vec2(.2, .25);
        var col = Color.Blue;
        for (int i = 0; i < structure.points.Count; i++)
        {
            Gizmos2D.Circle(view.Camera2D, structure.points[i], col, .01f);
        }
    }
}