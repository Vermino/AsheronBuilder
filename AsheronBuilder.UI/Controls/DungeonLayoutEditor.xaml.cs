// AsheronBuilder.UI/Controls/DungeonLayoutEditor.xaml.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AsheronBuilder.Core.Commands;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Core.Utils;
using OpenTK.Mathematics;
using ICommand = AsheronBuilder.Core.Commands.ICommand;

namespace AsheronBuilder.UI.Controls
{
    public partial class DungeonLayoutEditor : UserControl
    {
        public DungeonLayout DungeonLayout { get; private set; }
        public string CurrentFilePath { get; set; }

        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();
        private readonly Stack<DungeonLayout> _layoutUndoStack = new Stack<DungeonLayout>();
        private readonly Stack<DungeonLayout> _layoutRedoStack = new Stack<DungeonLayout>();
        private bool _showGrid = true;
        private bool _isDrawingCorridor;
        private Point _corridorStartPoint;
        private List<Point> _corridorPoints;
        private bool _isPlacingDoor;
        private Point _roomStartPoint;
        private Rectangle _tempRoom;

        public DungeonLayoutEditor()
        {
            InitializeComponent();
            NewDungeon();

            // Add keyboard shortcuts for undo/redo
            this.KeyDown += DungeonLayoutEditor_KeyDown;
            this.Focusable = true;
            this.Focus();
        }
        
        private void DeleteKeyFromNode(DungeonArea node, uint keyToDelete, int keyIndexInNode)
        {
            if (node == DungeonLayout.Hierarchy.RootArea)
            {
                DungeonLayout.Hierarchy.RootArea.ChildAreas.RemoveAt(keyIndexInNode);
            }
            else
            {
                node.ParentArea?.ChildAreas.RemoveAt(keyIndexInNode);
            }
        }
        
        private void DungeonLayoutEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    Undo();
                }
                else if (e.Key == Key.Y)
                {
                    Redo();
                }
            }
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
                RedrawDungeon();
                Logger.Log("Undo operation performed");
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
                RedrawDungeon();
                Logger.Log("Redo operation performed");
            }
        }
        
        private void SaveState()
        {
            _layoutUndoStack.Push(DeepCopyDungeonLayout(DungeonLayout));
            _layoutRedoStack.Clear();
        }

        private void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            RedrawDungeon();
        }

        private void FinishRoomPlacement()
        {
            if (_tempRoom != null)
            {
                try
                {
                    var newRoom = new DungeonArea($"Room_{DungeonLayout.Hierarchy.RootArea.ChildAreas.Count + 1}")
                    {
                        Position = new Vector3((float)Canvas.GetLeft(_tempRoom), (float)Canvas.GetTop(_tempRoom), 0),
                        Scale = new Vector3((float)_tempRoom.Width, (float)_tempRoom.Height, 1)
                    };

                    var command = new AddRoomCommand(DungeonLayout, newRoom, DungeonLayout.Hierarchy.RootArea);
                    ExecuteCommand(command);

                    EditorCanvas.Children.Remove(_tempRoom);
                    _tempRoom = null;

                    Logger.Log($"Room placed: {newRoom.Name} at position {newRoom.Position}");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to place room", ex);
                    MessageBox.Show($"Failed to place room: {ex.Message}", "Room Placement Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FinishCorridorDrawing()
        {
            _isDrawingCorridor = false;
            var startArea = FindAreaAtPoint(_corridorStartPoint);
            var endArea = FindAreaAtPoint(_corridorPoints.Last());

            if (startArea != null && endArea != null && startArea != endArea)
            {
                try
                {
                    var corridorPoints = _corridorPoints.Select(p => new Vector3((float)p.X, (float)p.Y, 0)).ToList();
                    var command = new AddCorridorCommand(DungeonLayout, corridorPoints, startArea);
                    ExecuteCommand(command);

                    Logger.Log($"Corridor added between {startArea.Name} and {endArea.Name}");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to add corridor", ex);
                    MessageBox.Show($"Failed to add corridor: {ex.Message}", "Corridor Placement Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            ClearTemporaryCorridor();
        }

        private void ClearTemporaryCorridor()
        {
            var elementsToRemove = EditorCanvas.Children.OfType<Line>()
                .Where(line => "TempCorridor".Equals(line.Tag as string))
                .ToList();
            foreach (var element in elementsToRemove)
            {
                EditorCanvas.Children.Remove(element);
            }
            _corridorPoints = null;
        }
        
        private void StartRoomPlacement(Point startPoint)
        {
            _roomStartPoint = startPoint;
            _tempRoom = new Rectangle
            {
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Opacity = 0.5
            };
            Canvas.SetLeft(_tempRoom, startPoint.X);
            Canvas.SetTop(_tempRoom, startPoint.Y);
            EditorCanvas.Children.Add(_tempRoom);
        }

        private void UpdateRoomPlacement(Point currentPoint)
        {
            if (_tempRoom != null)
            {
                double width = Math.Abs(currentPoint.X - _roomStartPoint.X);
                double height = Math.Abs(currentPoint.Y - _roomStartPoint.Y);

                _tempRoom.Width = width;
                _tempRoom.Height = height;

                Canvas.SetLeft(_tempRoom, Math.Min(_roomStartPoint.X, currentPoint.X));
                Canvas.SetTop(_tempRoom, Math.Min(_roomStartPoint.Y, currentPoint.Y));
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
            else if (e.LeftButton == MouseButtonState.Pressed && _tempRoom != null)
            {
                UpdateRoomPlacement(e.GetPosition(EditorCanvas));
            }
        }

        private void EditorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingCorridor)
            {
                FinishCorridorDrawing();
            }
            else if (_tempRoom != null)
            {
                FinishRoomPlacement();
            }
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

        public void ToggleGrid()
        {
            _showGrid = !_showGrid;
            RedrawDungeon();
        }

        public void StartDoorPlacement()
        {
            _isPlacingDoor = true;
        }
        
        private void UpdateTreeView()
        {
            DungeonHierarchy.Items.Clear();
            AddAreaToTreeView(DungeonLayout.Hierarchy.RootArea, null);
        }

        private void AddAreaToTreeView(DungeonArea area, TreeViewItem parentItem)
        {
            var item = new TreeViewItem { Header = area.Name };
            if (parentItem == null)
                DungeonHierarchy.Items.Add(item);
            else
                parentItem.Items.Add(item);

            foreach (var childArea in area.ChildAreas)
                AddAreaToTreeView(childArea, item);
        }

        private string GetAreaPath(DungeonArea area)
        {
            if (area.ParentArea == null)
                return area.Name;
            return GetAreaPath(area.ParentArea) + "/" + area.Name;
        }

        private void RedrawDungeon()
        {
            EditorCanvas.Children.Clear();

            if (_showGrid)
            {
                DrawGrid();
            }

            foreach (var area in DungeonLayout.Hierarchy.RootArea.GetAllAreas())
            {
                foreach (var envCell in area.EnvCells)
                {
                    DrawEnvCell(envCell);
                }
            }
        }

        private void DrawEnvCell(EnvCell envCell)
        {
            var rectangle = new Rectangle
            {
                Width = 50, // Placeholder size
                Height = 50, // Placeholder size
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rectangle, envCell.Position.X);
            Canvas.SetTop(rectangle, envCell.Position.Y);

            EditorCanvas.Children.Add(rectangle);
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

        private void PlaceDoor(Point clickPoint)
        {
            var area = FindAreaAtPoint(clickPoint);
            if (area != null)
            {
                // Create a new EnvCell to represent the door
                var doorEnvCell = new EnvCell(0) // Use appropriate EnvironmentId
                {
                    Position = new Vector3((float)clickPoint.X, (float)clickPoint.Y, 0)
                };
                
                // Add the door EnvCell to the DungeonLayout
                DungeonLayout.AddEnvCell(doorEnvCell, GetAreaPath(area));

                SaveState();
                RedrawDungeon();
            }
            _isPlacingDoor = false;
        }

        private DungeonArea FindAreaAtPoint(Point point)
        {
            return FindAreaAtPointRecursive(DungeonLayout.Hierarchy.RootArea, point);
        }

        private DungeonArea FindAreaAtPointRecursive(DungeonArea area, Point point)
        {
            // Check if the point is within the current area
            if (IsPointInArea(area, point))
            {
                // Check child areas first
                foreach (var childArea in area.ChildAreas)
                {
                    var foundArea = FindAreaAtPointRecursive(childArea, point);
                    if (foundArea != null)
                    {
                        return foundArea;
                    }
                }

                // If not in any child area, return the current area
                return area;
            }

            return null;
        }

        private bool IsPointInArea(DungeonArea area, Point point)
        {
            // This is a simple rectangular check. You may need to implement more complex
            // area shapes depending on your requirements.
            var areaPos = new Point(area.Position.X, area.Position.Y);
            var areaSize = new Vector2(area.Scale.X, area.Scale.Y);

            return point.X >= areaPos.X && point.X <= areaPos.X + areaSize.X &&
                   point.Y >= areaPos.Y && point.Y <= areaPos.Y + areaSize.Y;
        }

        private DungeonLayout DeepCopyDungeonLayout(DungeonLayout original)
        {
            var copy = new DungeonLayout();

            // Deep copy the hierarchy
            copy.Hierarchy = DeepCopyHierarchy(original.Hierarchy);

            return copy;
        }

        private DungeonHierarchy DeepCopyHierarchy(DungeonHierarchy original)
        {
            var copy = new DungeonHierarchy();
            copy.RootArea = DeepCopyArea(original.RootArea);
            return copy;
        }

        private DungeonArea DeepCopyArea(DungeonArea original)
        {
            var copy = new DungeonArea(original.Name)
            {
                Position = original.Position,
                Rotation = original.Rotation,
                Scale = original.Scale
            };

            foreach (var childArea in original.ChildAreas)
            {
                copy.AddChildArea(DeepCopyArea(childArea));
            }

            foreach (var envCell in original.EnvCells)
            {
                copy.AddEnvCell(DeepCopyEnvCell(envCell));
            }

            return copy;
        }

        private EnvCell DeepCopyEnvCell(EnvCell original)
        {
            return new EnvCell(original.EnvironmentId)
            {
                Id = original.Id,
                Position = original.Position,
                Rotation = original.Rotation,
                Scale = original.Scale
            };
        }
    }
}