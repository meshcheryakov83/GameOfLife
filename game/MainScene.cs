using System.Linq;
using Godot;

namespace GameOfLife
{
    public class MainScene : Node2D
    {
        private const int MinCellSize = 1;

        private readonly Color CellColor = new Color(0, 1f, 0);
        private readonly Vector2 MousePositionOffset = new Vector2(-15, 0);

        private readonly Colony _colony = new Colony();
        private readonly float _stepTimeSec = 0.1f;

        private GameStates _gameState;
        private float _cellSize = 1;

        private float _timeFromLastUpdateSec;

        private Vector2 _cameraPosition = Vector2.Zero;
        private Vector2? _dragMapPrevPos;

        private Vector2? _lastAddedCellWhileMouseMove;

        private Button _startButton;
        private Button _stepButton;
        private Button _clearButton;
        private Label _generationsLabel;

        public override void _Ready()
        {
            _startButton = GetNode<Button>("StartButton");
            _startButton.Connect("pressed", this, nameof(OnStartButtonClick));

            _stepButton = GetNode<Button>("StepButton");
            _stepButton.Connect("pressed", this, nameof(OnStepButtonClick));

            _clearButton = GetNode<Button>("ClearButton");
            _clearButton.Connect("pressed", this, nameof(OnClearButtonClick));

            _generationsLabel = GetNode<Label>("GenerationsLabel");

            Reset();
        }

        private void OnClearButtonClick()
        {
            Reset();
            Update();
        }

        private void OnStepButtonClick()
        {
            _colony.Update();
            Update();
        }

        private void OnStartButtonClick()
        {
            switch (_gameState)
            {
                case GameStates.Initial:
                    _gameState = GameStates.Started;
                    break;
                case GameStates.Started:
                    _gameState = GameStates.Paused;
                    break;
                case GameStates.Paused:
                    _gameState = GameStates.Started;
                    break;
            }

            _timeFromLastUpdateSec = _stepTimeSec;

            UpdateButtonStates();
        }

        private void Reset()
        {
            _gameState = GameStates.Initial;
            _colony.Clear();

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            switch (_gameState)
            {
                case GameStates.Initial:
                    _startButton.Text = "Start";
                    _stepButton.Disabled = false;
                    break;
                case GameStates.Started:
                    _startButton.Text = "Pause";
                    _stepButton.Disabled = true;
                    break;
                case GameStates.Paused:
                    _startButton.Text = "Resume";
                    _stepButton.Disabled = false;
                    break;
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);

            if (@event is InputEventMouseMotion motionEvent)
            {
                var mousePosition = motionEvent.Position + MousePositionOffset;

                // Move map with right mouse button
                if (motionEvent.ButtonMask == (int)ButtonList.Right)
                {
                    if (_dragMapPrevPos.HasValue)
                    {
                        _cameraPosition += _dragMapPrevPos.Value - mousePosition;
                    }

                    _dragMapPrevPos = mousePosition;
                    Update();
                }

                // "Draw" cells with left mouse button
                if (motionEvent.ButtonMask == (int)ButtonList.Left)
                {
                    var newCell = ((mousePosition + _cameraPosition) / _cellSize).Floor();

                    // to provide "smooth drawing"
                    if (_lastAddedCellWhileMouseMove.HasValue)
                    {
                        var curCell = _lastAddedCellWhileMouseMove.Value;
                        while (curCell != newCell)
                        {
                            curCell = curCell.MoveToward(newCell, 1);
                            _colony.Add(curCell.Round());
                        }
                    }

                    _lastAddedCellWhileMouseMove = newCell;

                    _gameState = GameStates.Initial;
                    UpdateButtonStates();

                    Update();
                }
            }
            else
            {
                _lastAddedCellWhileMouseMove = null;
                _dragMapPrevPos = null;
            }

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                var mousePosition = mouseEvent.Position + MousePositionOffset;

                // add/remove one cell with single click
                if (mouseEvent.ButtonMask == (int)ButtonList.Left)
                {
                    var newCell = ((mousePosition + _cameraPosition) / _cellSize).Floor();

                    if (_colony.Contains(newCell))
                    {
                        _colony.Remove(newCell);
                    }
                    else
                    {
                        _colony.Add(newCell);
                    }

                    Update();
                    _gameState = GameStates.Initial;
                    UpdateButtonStates();
                }
                // Zoom In/Out
                else if (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp
                         || mouseEvent.ButtonIndex == (int)ButtonList.WheelDown)
                {
                    var newCellSize = _cellSize + (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp ? 1 : -1);

                    if (newCellSize > MinCellSize)
                    {
                        // to keep the mouse on the same position during scale
                        _cameraPosition += (mousePosition + _cameraPosition) * (newCellSize / _cellSize - 1);

                        _cellSize = newCellSize;
                        Update();
                    }
                }
            }
        }

        public override void _Process(float deltaSec)
        {
            if (_gameState == GameStates.Started)
            {
                _timeFromLastUpdateSec += deltaSec;

                if (_timeFromLastUpdateSec > _stepTimeSec)
                {
                    _colony.Update();

                    _timeFromLastUpdateSec = 0;

                    Update();
                }
            }

            var mousePos = ((GetViewport().GetMousePosition() + MousePositionOffset + _cameraPosition) / _cellSize)
                .Floor();

            _generationsLabel.Text =
                $"Generation: #{_colony.GenerationsCounter}      " +
                $"Colony size: {_colony.Count()}      " +
                $"X={mousePos.x} Y={mousePos.y}";
        }

        public override void _Draw()
        {
            var viewPortSize = GetViewport().Size;

            foreach (var cell in _colony)
            {
                var cameraRect = new Rect2(
                    _cameraPosition / _cellSize - Vector2.One * 2,
                    viewPortSize / _cellSize + Vector2.One * 2);

                if (cameraRect.HasPoint(cell))
                {
                    if (_cellSize < MinCellSize + 4)
                    {
                        var cellPos = cell * _cellSize - _cameraPosition;
                        DrawRect(
                            new Rect2(
                                cellPos,
                                width: _cellSize,
                                height: _cellSize),
                            CellColor,
                            filled: true);
                    }
                    else
                    {
                        var cellPos = cell * _cellSize - _cameraPosition + Vector2.One;
                        DrawRect(
                            new Rect2(
                                cellPos,
                                width: _cellSize - 2,
                                height: _cellSize - 2),
                            CellColor,
                            filled: true);
                    }
                }
            }
        }
    }
}