using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public class Colony : IEnumerable<Vector2>
{
    public int GenerationsCounter { get; private set; }

    private readonly Dictionary<Vector2, int> _aliveCells = new Dictionary<Vector2, int>();
    private readonly Dictionary<Vector2, int> _emptyCells = new Dictionary<Vector2, int>();
    public long LastUpdateDurationMs { get; private set; }

    public void Add(Vector2 cell)
    {
        add(cell);
        GenerationsCounter = 0;
    }

    private void add(Vector2 cell)
    {
        if (_aliveCells.ContainsKey(cell))
        {
            return;
        }

        _aliveCells.Add(cell, 0);
        if (_emptyCells.ContainsKey(cell))
        {
            _emptyCells.Remove(cell);
        }

        foreach (var nearestCell in cell.GetNearest())
        {
            if (_aliveCells.ContainsKey(nearestCell))
            {
                _aliveCells[cell]++;
                _aliveCells[nearestCell]++;
                continue;
            }

            if (!_emptyCells.ContainsKey(nearestCell))
            {
                _emptyCells[nearestCell] = 0;
            }

            _emptyCells[nearestCell]++;
        }
    }

    public void Remove(Vector2 cell)
    {
        remove(cell);
        GenerationsCounter = 0;
    }

    private void remove(Vector2 cell)
    {
        if (!_aliveCells.ContainsKey(cell))
        {
            return;
        }

        _aliveCells.Remove(cell);
        _emptyCells[cell] = 0;
        foreach (var nearestCell in cell.GetNearest())
        {
            if (_aliveCells.ContainsKey(nearestCell))
            {
                _emptyCells[cell]++;
                _aliveCells[nearestCell]--;
                continue;
            }

            _emptyCells[nearestCell]--;
            if (_emptyCells[nearestCell] == 0)
            {
                _emptyCells.Remove(nearestCell);
            }
        }
    }

    public void Clear()
    {
        _aliveCells.Clear();
        _emptyCells.Clear();
        GenerationsCounter = 0;
    }

    public bool Contains(Vector2 cell)
    {
        return _aliveCells.ContainsKey(cell);
    }

    public void Update()
    {
        var sw = Stopwatch.StartNew();
        GenerationsCounter++;

        var cellsToDie = new HashSet<Vector2>();
        foreach (var cell in _aliveCells)
        {
            if (cell.Value < 2 || cell.Value > 3)
            {
                cellsToDie.Add(cell.Key);
            }
        }

        var newCells = new HashSet<Vector2>();
        foreach (var emptyCell in _emptyCells)
        {
            if (emptyCell.Value == 3)
            {
                newCells.Add(emptyCell.Key);
            }
        }

        foreach (var cellToDie in cellsToDie)
        {
            remove(cellToDie);
        }

        foreach (var cellToBorn in newCells)
        {
            add(cellToBorn);
        }

        LastUpdateDurationMs = sw.ElapsedMilliseconds;
    }

    public IEnumerator<Vector2> GetEnumerator()
    {
        return _aliveCells.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class Vector2Extension
{
    public static IEnumerable<Vector2> GetNearest(this Vector2 cell)
    {
        yield return new Vector2(cell.x - 1, cell.y - 1);
        yield return new Vector2(cell.x, cell.y - 1);
        yield return new Vector2(cell.x + 1, cell.y - 1);

        yield return new Vector2(cell.x - 1, cell.y);
        //yield return new Vector2(cell.x, cell.y);
        yield return new Vector2(cell.x + 1, cell.y);

        yield return new Vector2(cell.x - 1, cell.y + 1);
        yield return new Vector2(cell.x, cell.y + 1);
        yield return new Vector2(cell.x + 1, cell.y + 1);
    }
}
