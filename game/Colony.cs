using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class Colony : IEnumerable<Vector2>
{
    public int GenerationsCounter { get; private set; }

    private readonly Dictionary<Vector2, int> _aliveCells = new Dictionary<Vector2, int>();
    private readonly Dictionary<Vector2, int> _emptyCells = new Dictionary<Vector2, int>();

    private readonly HashSet<Vector2> _aliveCellsToDie = new HashSet<Vector2>();
    private readonly HashSet<Vector2> _emptyCellsToBorn = new HashSet<Vector2>();

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
        _emptyCells.Remove(cell);
        _emptyCellsToBorn.Remove(cell);

        foreach (var nearestCell in cell.GetNearest())
        {
            var tmp = 0;
            if (_aliveCells.ContainsKey(nearestCell))
            {
                tmp = ++_aliveCells[cell];
                if (tmp == 3 || tmp == 2)
                {
                    _aliveCellsToDie.Remove(cell);
                }
                else
                {
                    _aliveCellsToDie.Add(cell);
                }

                tmp = ++_aliveCells[nearestCell];
                if (tmp == 3 || tmp == 2)
                {
                    _aliveCellsToDie.Remove(nearestCell);
                }
                else
                {
                    _aliveCellsToDie.Add(nearestCell);
                }

                continue;
            }

            if (!_emptyCells.ContainsKey(nearestCell))
            {
                _emptyCells[nearestCell] = 0;
            }

            tmp = ++_emptyCells[nearestCell];
            if (tmp == 3)
            {
                _emptyCellsToBorn.Add(nearestCell);
            }
            else
            {
                _emptyCellsToBorn.Remove(nearestCell);
            }
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
        _aliveCellsToDie.Remove(cell);

        _emptyCells[cell] = 0;
        foreach (var nearestCell in cell.GetNearest())
        {
            var tmp = 0;
            if (_aliveCells.ContainsKey(nearestCell))
            {
                tmp = ++_emptyCells[cell];
                if (tmp == 3)
                {
                    _emptyCellsToBorn.Add(cell);
                }
                else
                {
                    _emptyCellsToBorn.Remove(cell);
                }

                tmp = --_aliveCells[nearestCell];
                if (tmp == 3 || tmp == 2)
                {
                    _aliveCellsToDie.Remove(nearestCell);
                }
                else
                {
                    _aliveCellsToDie.Add(nearestCell);
                }

                continue;
            }

            tmp = --_emptyCells[nearestCell];
            if (tmp == 3)
            {
                _emptyCellsToBorn.Add(cell);
            }
            else
            {
                _emptyCellsToBorn.Remove(cell);
            }

            if (tmp == 0)
            {
                _emptyCells.Remove(nearestCell);
            }
        }
    }

    public void Clear()
    {
        _aliveCells.Clear();
        _emptyCells.Clear();
        _aliveCellsToDie.Clear();
        _emptyCellsToBorn.Clear();
        GenerationsCounter = 0;
    }

    public bool Contains(Vector2 cell)
    {
        return _aliveCells.ContainsKey(cell);
    }

    public void Update()
    {
        GenerationsCounter++;

        var toDie = _aliveCellsToDie.ToArray();
        var toBorn = _emptyCellsToBorn.ToArray();

        foreach (var cellToDie in toDie)
        {
            remove(cellToDie);
        }

        foreach (var cellToBorn in toBorn)
        {
            add(cellToBorn);
        }
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
    public static Vector2[] GetNearest(this Vector2 cell)
    {
        return new Vector2[]
        {
            new Vector2(cell.x - 1, cell.y - 1),
            new Vector2(cell.x, cell.y - 1),
            new Vector2(cell.x + 1, cell.y - 1),

            new Vector2(cell.x - 1, cell.y),
            new Vector2(cell.x + 1, cell.y),

            new Vector2(cell.x - 1, cell.y + 1),
            new Vector2(cell.x, cell.y + 1),
            new Vector2(cell.x + 1, cell.y + 1)
        };
    }
}