using Godot;
using System;
using System.Collections.Generic;

public class MainScene : Node2D
{
    private readonly Color BorderColor = new Color(00, 00, 00);
    private readonly Color CellFillColor = new Color(0, 250, 0);
    
    private readonly HashSet<Vector2> _cells = new HashSet<Vector2>();
    
    private int _cellSize = 20;
    private float _lastUpdate = 0;

    private Vector2 _cameraPosition = Vector2.Zero;
    private Vector2? _dragPrevPos;

    public override void _Ready()
    {
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        if (@event is InputEventMouseMotion motionEvent)
        {
            if (motionEvent.ButtonMask == (int)ButtonList.Right)
            {
                if (_dragPrevPos.HasValue)
                {
                    _cameraPosition += _dragPrevPos.Value - motionEvent.Position;
                }
                
                _dragPrevPos = motionEvent.Position;
                Update();
            }
        }
        else
        {
            _dragPrevPos = null;    
        }
        
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left)
            {
                var pos = GetViewport().GetMousePosition();
                var x = (int)((pos.x + _cameraPosition.x) / _cellSize);
                var y = (int)((pos.y + _cameraPosition.y) / _cellSize);

                var newCell = new Vector2(x, y);
                if (_cells.Contains(newCell))
                {
                    _cells.Remove(newCell);
                }
                else
                {
                    _cells.Add(newCell);
                }

                Update();
                _lastUpdate = -1;    
            }
            else if (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp)
            {
                _cellSize += 1;
                Update();
            }
            else if (mouseEvent.ButtonIndex == (int)ButtonList.WheelDown)
            {
                if (_cellSize > 3)
                {
                    _cellSize -= 1;
                    Update();
                }
            }
        }
    }

    public override void _Process(float delta)
    {
        _lastUpdate += delta;
        
        if (_lastUpdate > 1)
        {
            Colony.Update(_cells);

            _lastUpdate = 0;
            
            Update();
        }
    }

    public override void _Draw()
    {
        var viewPortSize = GetViewport().Size;
        
        for (float x = -1 * (_cameraPosition.x % _cellSize); x < viewPortSize.x; x += _cellSize)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, viewPortSize.y), BorderColor, 1);
        }

        for (float y = -1 * (_cameraPosition.y % _cellSize); y < viewPortSize.y; y += _cellSize)
        {
            DrawLine(new Vector2(0, y), new Vector2(viewPortSize.x, y), BorderColor, 1);
        }

        foreach (var cell in _cells)
        {
            var cameraRect = new Rect2(
                x: _cameraPosition.x / _cellSize - 1,
                y: _cameraPosition.y / _cellSize - 1,
                width: viewPortSize.x / _cellSize + 1,
                height: viewPortSize.y / _cellSize + 1);

            if (cameraRect.HasPoint(cell))
            {
                DrawRect(
                    new Rect2(
                        x: cell.x * _cellSize - _cameraPosition.x,
                        y: cell.y * _cellSize - _cameraPosition.y,
                        width: _cellSize,
                        height: _cellSize),
                    CellFillColor,
                    filled: true);    
            }
        }
    }
}
