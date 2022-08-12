using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;

public class Colony : IEnumerable<Vector2>
{
    public int GenerationsCounter { get; private set; }

    private readonly HashSet<Vector2> _cells = new HashSet<Vector2>();

    public void Add(Vector2 cell)
    {
        _cells.Add(cell);
        GenerationsCounter = 0;
    }

    public void Remove(Vector2 cell)
    {
        _cells.Remove(cell);
        GenerationsCounter = 0;
    }

    public bool Contains(Vector2 cell)
    {
        return _cells.Contains(cell);
    }

    public void Update()
    {
        GenerationsCounter++;

        var cellsToDie = new HashSet<Vector2>();
        foreach (var cell in _cells)
        {
            var nearCount = NearCount(_cells, cell) - 1;
            if (nearCount < 2 || nearCount > 3)
            {
                cellsToDie.Add(cell);
            }
        }

        var possibleToBorn = new HashSet<Vector2>();
        foreach(var cell in _cells)
        {
            for(float x = cell.x - 1; x <= cell.x + 1; x++)
            {
                for(float y = cell.y - 1; y <= cell.y + 1; y++)
                {
                    var positionToCheck = new Vector2(x, y);
                    if (!_cells.Contains(positionToCheck))
                    {
                        possibleToBorn.Add(positionToCheck);
                    }
                }
            }
        }

        var newCells = new HashSet<Vector2>();
        foreach (var possibleNewCell in possibleToBorn)
        {
            var nearCount = NearCount(_cells, possibleNewCell);
            if (nearCount == 3)
            {
                newCells.Add(possibleNewCell);
            }
        }

        foreach (var cellToDie in cellsToDie)
        {
            _cells.Remove(cellToDie);
        }

        foreach (var cellToBorn in newCells)
        {
            _cells.Add(cellToBorn);
        }
    }

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

    public IEnumerator<Vector2> GetEnumerator()
    {
        return _cells.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Clear()
    {
        _cells.Clear();
        GenerationsCounter = 0;
    }
}