using Godot;
using System;
using System.Collections.Generic;

public class Main : Node2D
{
    private int cellSize = 20;
    private Color color = new Color(00, 00, 00);
    private Color cellColor = new Color(0, 250, 0);
    private float lastUpdate = 0;
    private HashSet<Vector2> cells = new HashSet<Vector2>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cells.Add(new Vector2(0, 1));
        cells.Add(new Vector2(1, 1));
        cells.Add(new Vector2(2, 1));

        //cells.Add(new Vector2(1, 0));
        cells.Add(new Vector2(1, 2));
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == 1)
            {
                var pos = GetViewport().GetMousePosition();
                var x = (int)(pos.x / cellSize);
                var y = (int)(pos.y / cellSize);

                var newCell = new Vector2(x, y);
                if (cells.Contains(newCell))
                {
                    cells.Remove(newCell);
                }
                else
                {
                    cells.Add(newCell);
                }

                //needUpdate = true;
                Update();
                lastUpdate = -1;
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        bool needUpdate = false;

        lastUpdate += delta;
        if (lastUpdate > 1)
        {
            Colony.Update(cells);

            lastUpdate = 0;

            needUpdate = true;
        }

        if (needUpdate)
        {
            Update();
        }
    }

    public override void _Draw()
    {
        var size = GetViewport().Size;

        for (int x = 0; x < size.x; x+=cellSize)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, size.y), color, 1);
        }

        for (int y = 0; y < size.y; y+=cellSize)
        {
            DrawLine(new Vector2(0, y), new Vector2(size.x, y), color, 1);
        }

        foreach (var cell in cells)
        {
            this.DrawRect(new Rect2((float)cell.x * cellSize, (float)cell.y * cellSize, (float)cellSize, (float)cellSize), cellColor, true);
        }
    }
}

public static class Colony
{
    private static int NearCount(IEnumerable<Vector2> cells, Vector2 cell)
    {
        var nearCount = 0;
        foreach(var nearCell in cells)
        {
            var deltaX = nearCell.x - cell.x;
            var deltaY = nearCell.y - cell.y;
            if (deltaX >= -1 && deltaX <= 1 && deltaY >= -1 && deltaY <= 1)
            {
                nearCount++;
            }
        }

        return nearCount;
    }

    public static void Update(HashSet<Vector2> cells)
    {
        var cellsToDie = new HashSet<Vector2>();
        foreach (var cell in cells)
        {
            var nearCount = NearCount(cells, cell) - 1;
            if (nearCount < 2 || nearCount > 3)
            {
                cellsToDie.Add(cell);
            }
        }

        var possibleToBorn = new HashSet<Vector2>();
        foreach(var cell in cells)
        {
            for(float x = cell.x - 1; x <= cell.x + 1; x++)
            {
                for(float y = cell.y - 1; y <= cell.y + 1; y++)
                {
                    var positionToCheck = new Vector2(x, y);
                    if (!cells.Contains(positionToCheck))
                    {
                        possibleToBorn.Add(positionToCheck);
                    }
                }
            }
        }

        var toBorn = new HashSet<Vector2>();
        foreach (var possibleNewCell in possibleToBorn)
        {
            var nearCount = NearCount(cells, possibleNewCell);
            if (nearCount == 3)
            {
                toBorn.Add(possibleNewCell);
            }
        }

        foreach (var cellToDie in cellsToDie)
        {
            cells.Remove(cellToDie);
        }

        foreach (var cellToBorn in toBorn)
        {
            cells.Add(cellToBorn);
        }
    }
}
