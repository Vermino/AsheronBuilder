// AsheronBuilder.UI/Controls/DungeonLayoutEditor.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AsheronBuilder.Core.Dungeon;


namespace AsheronBuilder.UI.Controls
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
        
        private void UpdateTreeView()
        {
            DungeonHierarchyTreeView.Items.Clear();
            AddAreaToTreeView(_dungeonLayout.Hierarchy.RootArea, null);
        }

        private void AddAreaToTreeView(DungeonArea area, TreeViewItem parentItem)
        {
            var item = new TreeViewItem { Header = area.Name };
            if (parentItem == null)
                DungeonHierarchyTreeView.Items.Add(item);
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
            var startArea = FindAreaAtPoint(_corridorStartPoint);
            var endArea = FindAreaAtPoint(_corridorPoints.Last());

            if (startArea != null && endArea != null && startArea != endArea)
            {
                // Create a new EnvCell to represent the corridor
                var corridorEnvCell = new EnvCell(0); // Use appropriate EnvironmentId
                corridorEnvCell.Position = new System.Numerics.Vector3((float)_corridorStartPoint.X, (float)_corridorStartPoint.Y, 0);
                
                // Add the corridor EnvCell to the DungeonLayout
                DungeonLayout.AddEnvCell(corridorEnvCell, startArea.GetPath());

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
            var area = FindAreaAtPoint(clickPoint);
            if (area != null)
            {
                // Create a new EnvCell to represent the door
                var doorEnvCell = new EnvCell(0); // Use appropriate EnvironmentId
                doorEnvCell.Position = new System.Numerics.Vector3((float)clickPoint.X, (float)clickPoint.Y, 0);
                
                // Add the door EnvCell to the DungeonLayout
                DungeonLayout.AddEnvCell(doorEnvCell, area.GetPath());

                SaveState();
                RedrawDungeon();
            }
            _isPlacingDoor = false;
        }

        private DungeonArea FindAreaAtPoint(Point point)
        {
            // Implement area detection logic
            // This is a placeholder implementation
            return DungeonLayout.Hierarchy.RootArea;
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