using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ACDungeonBuilder.Core.Dungeon;
using Point = System.Windows.Point;

namespace ACDungeonBuilder.UI.Controls
{
    public partial class DungeonLayoutEditor : UserControl
    {
        public DungeonLayout DungeonLayout { get; private set; }
        public string CurrentFilePath { get; set; }

        private Stack<DungeonLayout> _undoStack = new Stack<DungeonLayout>();
        private Stack<DungeonLayout> _redoStack = new Stack<DungeonLayout>();
        private bool _showGrid = true;
        private bool _isDrawingCorridor;
        private Point _corridorStartPoint;
        private List<Point> _corridorPoints;
        private bool _isPlacingDoor;

        public DungeonLayoutEditor()
        {
            InitializeComponent();
            NewDungeon();
        }

        public void NewDungeon()
        {
            DungeonLayout = new DungeonLayout();
            CurrentFilePath = null;
            _undoStack.Clear();
            _redoStack.Clear();
            RedrawDungeon();
        }

        public void LoadDungeon(DungeonLayout dungeonLayout)
        {
            DungeonLayout = dungeonLayout;
            _undoStack.Clear();
            _redoStack.Clear();
            RedrawDungeon();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _redoStack.Push(DungeonLayout);
                DungeonLayout = _undoStack.Pop();
                RedrawDungeon();
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _undoStack.Push(DungeonLayout);
                DungeonLayout = _redoStack.Pop();
                RedrawDungeon();
            }
        }

        public void ToggleGrid()
        {
            _showGrid = !_showGrid;
            RedrawDungeon();
        }

        public void StartDoorPlacement()
        {
            _isPlacingDoor = true;
        }

        private void RedrawDungeon()
        {
            EditorCanvas.Children.Clear();

            if (_showGrid)
            {
                DrawGrid();
            }

            foreach (var room in DungeonLayout.Rooms)
            {
                DrawRoom(room);
            }

            foreach (var corridor in DungeonLayout.Corridors)
            {
                DrawCorridor(corridor);
            }

            foreach (var door in DungeonLayout.Doors)
            {
                DrawDoor(door);
            }
        }

        private void DrawGrid()
        {
            double gridSize = 20;
            for (double x = 0; x < EditorCanvas.ActualWidth; x += gridSize)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = EditorCanvas.ActualHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                EditorCanvas.Children.Add(line);
            }
            for (double y = 0; y < EditorCanvas.ActualHeight; y += gridSize)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = EditorCanvas.ActualWidth,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                EditorCanvas.Children.Add(line);
            }
        }

        private void DrawRoom(Room room)
        {
            var rectangle = new Rectangle
            {
                Width = room.Width,
                Height = room.Height,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rectangle, room.X);
            Canvas.SetTop(rectangle, room.Y);

            EditorCanvas.Children.Add(rectangle);
        }

        private void DrawCorridor(Corridor corridor)
        {
            for (int i = 1; i < corridor.Path.Count; i++)
            {
                var line = new Line
                {
                    X1 = corridor.Path[i - 1].X,
                    Y1 = corridor.Path[i - 1].Y,
                    X2 = corridor.Path[i].X,
                    Y2 = corridor.Path[i].Y,
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 3
                };
                EditorCanvas.Children.Add(line);
            }
        }

        private void DrawDoor(Door door)
        {
            var room = DungeonLayout.Rooms.FirstOrDefault(r => r.Id == door.RoomId);
            if (room != null)
            {
                var rectangle = new Rectangle
                {
                    Width = 10,
                    Height = 20,
                    Fill = Brushes.Brown,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rectangle, room.X + door.X - 5);
                Canvas.SetTop(rectangle, room.Y + door.Y - 10);

                EditorCanvas.Children.Add(rectangle);
            }
        }

        private void EditorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var clickPoint = e.GetPosition(EditorCanvas);
                
                if (_isPlacingDoor)
                {
                    PlaceDoor(clickPoint);
                }
                else if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    StartCorridorDrawing(clickPoint);
                }
                else
                {
                    StartRoomPlacement(clickPoint);
                }
            }
        }

        private void EditorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawingCorridor)
            {
                var currentPoint = e.GetPosition(EditorCanvas);
                _corridorPoints.Add(currentPoint);
                DrawTemporaryCorridor();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Room dragging or resizing logic here
            }
        }

        private void EditorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingCorridor)
            {
                FinishCorridorDrawing();
            }
            else
            {
                FinishRoomPlacement();
            }
        }

        private void StartRoomPlacement(Point startPoint)
        {
            // Implement room placement logic here
        }

        private void FinishRoomPlacement()
        {
            // Implement room placement finalization logic here
        }

        private void StartCorridorDrawing(Point startPoint)
        {
            _isDrawingCorridor = true;
            _corridorStartPoint = startPoint;
            _corridorPoints = new List<Point> { startPoint };
        }

        private void DrawTemporaryCorridor()
        {
            // Remove previous temporary corridor
            var elementsToRemove = EditorCanvas.Children.OfType<Line>()
                .Where(line => "TempCorridor".Equals(line.Tag as string))
                .ToList();
            foreach (var element in elementsToRemove)
            {
                EditorCanvas.Children.Remove(element);
            }

            for (int i = 1; i < _corridorPoints.Count; i++)
            {
                var line = new Line
                {
                    X1 = _corridorPoints[i - 1].X,
                    Y1 = _corridorPoints[i - 1].Y,
                    X2 = _corridorPoints[i].X,
                    Y2 = _corridorPoints[i].Y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Tag = "TempCorridor"
                };
                EditorCanvas.Children.Add(line);
            }
        }

        private void FinishCorridorDrawing()
        {
            _isDrawingCorridor = false;
            var startRoom = FindRoomAtPoint(_corridorStartPoint);
            var endRoom = FindRoomAtPoint(_corridorPoints.Last());

            if (startRoom != null && endRoom != null && startRoom != endRoom)
            {
                var corridor = new Corridor
                {
                    Id = DungeonLayout.Corridors.Count,
                    StartRoomId = startRoom.Id,
                    EndRoomId = endRoom.Id,
                    Path = _corridorPoints.Select(p => new ACDungeonBuilder.Core.Dungeon.Point((int)p.X, (int)p.Y)).ToList()
                };
                DungeonLayout.Corridors.Add(corridor);
                SaveState();
                RedrawDungeon();
            }
            else
            {
                var elementsToRemove = EditorCanvas.Children.OfType<Line>()
                    .Where(line => "TempCorridor".Equals(line.Tag as string))
                    .ToList();
                foreach (var element in elementsToRemove)
                {
                    EditorCanvas.Children.Remove(element);
                }
            }

            _corridorPoints = null;
        }

        private void PlaceDoor(Point clickPoint)
        {
            var room = FindRoomAtPoint(clickPoint);
            if (room != null)
            {
                var door = new Door
                {
                    Id = DungeonLayout.Doors.Count,
                    RoomId = room.Id,
                    X = (int)(clickPoint.X - room.X),
                    Y = (int)(clickPoint.Y - room.Y),
                    Type = DoorType.Normal
                };
                DungeonLayout.Doors.Add(door);
                SaveState();
                RedrawDungeon();
            }
            _isPlacingDoor = false;
        }

        private Room FindRoomAtPoint(Point point)
        {
            return DungeonLayout.Rooms.FirstOrDefault(r =>
                point.X >= r.X && point.X <= r.X + r.Width &&
                point.Y >= r.Y && point.Y <= r.Y + r.Height);
        }

        private void SaveState()
        {
            _undoStack.Push(DeepCopyDungeonLayout(DungeonLayout));
            _redoStack.Clear();
        }

        private DungeonLayout DeepCopyDungeonLayout(DungeonLayout original)
        {
            // Implement a deep copy method for DungeonLayout
            // This is a placeholder and needs to be properly implemented
            return new DungeonLayout();
        }
    }
}