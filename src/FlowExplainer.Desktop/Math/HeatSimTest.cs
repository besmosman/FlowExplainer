using ImGuiNET;

namespace FlowExplainer;

public class HeatSimTest : WorldService
{

    public class Cell
    {
        public float Temperature;
        public float StartWorldSpace;
        public float EndWorldSpace;
        public float CenterWorldSpace;
        public Cell Left;
        public Cell Right;
    }

    public float CellWidth;
    public float DomainWidth = 1;
    public int CellCount = 100;
    private Cell[] Cells;

    public override void Initialize()
    {
        Cells = new Cell[CellCount];
        CellWidth = DomainWidth / CellCount;
        for (int i = 0; i < CellCount; i++)
        {
            Cells[i] = new Cell()
            {
                Temperature = 0,
                StartWorldSpace = CellWidth * i,
                CenterWorldSpace = CellWidth * (i + .5f),
                EndWorldSpace = CellWidth * (i + 1),
            };

         // Cells[0] = 1;
        }
        for (int i = 0; i < CellCount; i++)
        {
            if (i == 0)
                Cells[i].Left = Cells[CellCount - 1];
            else
                Cells[i].Left = Cells[i - 1];

            if (i == CellCount - 1)
                Cells[i].Right = Cells[0];
            else
                Cells[i].Right = Cells[i + 1];
        }
    }


    public float Velocity(float p)
    {
        return 1;
    }

    public float dTdx(Cell cell)
    {
        return (cell.Right.Temperature - cell.Left.Temperature) / CellWidth;
    }


    public void Step()
    {
        foreach (var cell in Cells)
        {
            cell.Temperature += Velocity(cell.CenterWorldSpace) * dTdx(cell);
        }
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        foreach (var cell in Cells)
        {
            Gizmos2D.Instanced.RegisterRectCenterd(new Vec2(cell.CenterWorldSpace, .25f), new Vec2(CellWidth, .5f), new Color(cell.Temperature,0,0,1));
        }
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }

    public override void DrawImGuiEdit()
    {
        base.DrawImGuiEdit();
    }
}