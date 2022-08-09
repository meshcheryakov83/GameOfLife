using System.Collections.Generic;
using Godot;

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