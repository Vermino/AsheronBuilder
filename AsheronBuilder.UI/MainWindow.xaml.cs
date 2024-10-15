// File: AsheronBuilder.UI/MainWindow.xaml.cs

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AsheronBuilder.Core;
using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Rendering;
using AsheronBuilder.Core.Commands;
using Microsoft.Win32;
using OpenTK.Mathematics;
using AsheronBuilder.Core.Landblock;
using CommandManager = AsheronBuilder.Core.Commands.CommandManager;

namespace AsheronBuilder.UI
{
    public partial class MainWindow : Window
    {
        private LandblockManager _landblockManager;
        private uint _currentLandblockId;
        private AssetManager _assetManager;
        private DungeonLayout _dungeonLayout;
        private CommandManager _commandManager;
        private ManipulationMode _currentMode;
        private bool _snapToGrid;
        private bool _showWireframe;
        private bool _showCollision;
        private EnvCell _selectedEnvCell;
        private Point _lastMousePosition;

        public MainWindow()
        {
            InitializeComponent();
            _commandManager = new CommandManager();
            
            try
            {
                InitializeAssetManager();
                InitializeDungeonLayout();
                LoadAssetsAsync();
                
                _currentMode = ManipulationMode.Move;
                _snapToGrid = false;
                _showWireframe = false;
                _showCollision = false;

                SetupEventHandlers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during initialization: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void InitializeLandblockManager()
        {
            _landblockManager = new LandblockManager();
            _currentLandblockId = 0;
            UpdateLandblockDisplay();
        }
        
        private void UpdateLandblockDisplay()
        {
            LandblockInfoTextBlock.Text = $"0x{_currentLandblockId:X8}: (landblock) {_currentLandblockId} (0x{(_currentLandblockId >> 16):X4}, 0x{(_currentLandblockId & 0xFFFF):X4})";
        }

        private void LoadLandblock_Click(object sender, RoutedEventArgs e)
        {
            var landblock = _landblockManager.GetLandblock(_currentLandblockId);
            MainViewport.SetLandblock(landblock);
            UpdateLandblockDisplay();
        }

        private void SaveLandblock_Click(object sender, RoutedEventArgs e)
        {
            var landblock = MainViewport.GetCurrentLandblock();
            if (landblock != null)
            {
                _landblockManager.SaveLandblock(landblock);
                MessageBox.Show("Landblock saved successfully.", "Save Landblock", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearLandblock_Click(object sender, RoutedEventArgs e)
        {
            _landblockManager.ClearLandblock(_currentLandblockId);
            var landblock = _landblockManager.GetLandblock(_currentLandblockId);
            MainViewport.SetLandblock(landblock);
            UpdateLandblockDisplay();
        }

        private void GoToLandblock_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(LandblockIdTextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out uint landblockId))
            {
                _currentLandblockId = landblockId;
                var landblock = _landblockManager.GetLandblock(_currentLandblockId);
                MainViewport.SetLandblock(landblock);
                UpdateLandblockDisplay();
            }
            else
            {
                MessageBox.Show("Invalid landblock ID. Please enter a valid hexadecimal number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeAssetManager()
        {
            string datPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            if (!System.IO.Directory.Exists(datPath))
            {
                System.IO.Directory.CreateDirectory(datPath);
            }
            _assetManager = new AssetManager(datPath);
        }

        private void InitializeDungeonLayout()
        {
            _dungeonLayout = new DungeonLayout();
            // Add some sample EnvCells for demonstration
            _dungeonLayout.AddEnvCell(new EnvCell(1) { Position = new Vector3(0, 0, 0), Scale = Vector3.One });
            _dungeonLayout.AddEnvCell(new EnvCell(2) { Position = new Vector3(2, 0, 2), Scale = Vector3.One * 1.5f });
            _dungeonLayout.AddEnvCell(new EnvCell(3) { Position = new Vector3(-2, 0, -2), Scale = Vector3.One * 0.8f });

            UpdateViewports();
        }

        private async void LoadAssetsAsync()
        {
            var loadingDialog = new LoadingDialog();
            loadingDialog.Show();

            try
            {
                await _assetManager.LoadAssetsAsync();
                UpdateAssetBrowser();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assets: {ex.Message}", "Asset Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loadingDialog.Close();
            }
        }

        private void SetupEventHandlers()
        {
            MainViewport.MouseMove += MainViewport_MouseMove;
            MainViewport.MouseDown += MainViewport_MouseDown;
            MainViewport.MouseUp += MainViewport_MouseUp;
            MainViewport.MouseWheel += MainViewport_MouseWheel;

            AssetBrowserTreeView.SelectedItemChanged += AssetBrowserTreeView_SelectedItemChanged;
            DungeonHierarchyTreeView.SelectedItemChanged += DungeonHierarchyTreeView_SelectedItemChanged;
        }

        private void UpdateViewports()
        {
            MainViewport.SetDungeonLayout(_dungeonLayout);
            MiniMap.SetDungeonLayout(_dungeonLayout);
        }

        private void UpdateAssetBrowser()
        {
            AssetBrowserTreeView.Items.Clear();

            var texturesNode = new TreeViewItem { Header = "Textures" };
            foreach (var textureId in _assetManager.GetTextureFileIds())
            {
                texturesNode.Items.Add(new TreeViewItem { Header = $"Texture {textureId}", Tag = textureId });
            }
            AssetBrowserTreeView.Items.Add(texturesNode);

            var modelsNode = new TreeViewItem { Header = "Models" };
            foreach (var modelId in _assetManager.GetModelFileIds())
            {
                modelsNode.Items.Add(new TreeViewItem { Header = $"Model {modelId}", Tag = modelId });
            }
            AssetBrowserTreeView.Items.Add(modelsNode);

            var environmentsNode = new TreeViewItem { Header = "Environments" };
            foreach (var environmentId in _assetManager.GetEnvironmentFileIds())
            {
                environmentsNode.Items.Add(new TreeViewItem { Header = $"Environment {environmentId}", Tag = environmentId });
            }
            AssetBrowserTreeView.Items.Add(environmentsNode);
        }

        private void UpdateDungeonHierarchy()
        {
            DungeonHierarchyTreeView.Items.Clear();
            AddAreaToTreeView(_dungeonLayout.Hierarchy.RootArea, null);
        }

        private void AddAreaToTreeView(DungeonArea area, TreeViewItem parentItem)
        {
            var areaItem = new TreeViewItem { Header = area.Name, Tag = area };

            if (parentItem == null)
            {
                DungeonHierarchyTreeView.Items.Add(areaItem);
            }
            else
            {
                parentItem.Items.Add(areaItem);
            }

            foreach (var childArea in area.ChildAreas)
            {
                AddAreaToTreeView(childArea, areaItem);
            }

            foreach (var envCell in area.EnvCells)
            {
                var envCellItem = new TreeViewItem { Header = $"EnvCell {envCell.Id}", Tag = envCell };
                areaItem.Items.Add(envCellItem);
            }
        }

        private void MainViewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(MainViewport);
                if (_currentMode == ManipulationMode.Move && _selectedEnvCell != null)
                {
                    Vector3 newPosition = CalculateNewPosition(currentPos);
                    var command = new MoveEnvCellCommand(_dungeonLayout, _selectedEnvCell, newPosition);
                    _commandManager.ExecuteCommand(command);
                    UpdateViewports();
                }
                else
                {
                    // Camera rotation
                    MainViewport.RotateCamera((float)(currentPos.X - _lastMousePosition.X) * 0.1f,
                                              (float)(currentPos.Y - _lastMousePosition.Y) * 0.1f);
                }
                _lastMousePosition = currentPos;
            }
        }

        private void MainViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMousePosition = e.GetPosition(MainViewport);
                if (_currentMode == ManipulationMode.Select)
                {
                    _selectedEnvCell = MainViewport.PickObject(_lastMousePosition);
                    UpdatePropertyPanel();
                }
            }
        }

        private void MainViewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // Finish any ongoing operations
            }
        }

        private void MainViewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainViewport.ZoomCamera(e.Delta * 0.001f);
        }

        private void AssetBrowserTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is uint assetId)
            {
                // Display asset properties or preview
            }
        }

        private void DungeonHierarchyTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                if (item.Tag is DungeonArea area)
                {
                    // Display area properties
                }
                else if (item.Tag is EnvCell envCell)
                {
                    _selectedEnvCell = envCell;
                    UpdatePropertyPanel();
                }
            }
        }

        private void UpdatePropertyPanel()
        {
            if (_selectedEnvCell == null)
            {
                // Hide or clear property panel
                return;
            }

            // Update property panel with _selectedEnvCell properties
            ObjectNameTextBox.Text = $"EnvCell {_selectedEnvCell.Id}";
            ObjectTypeComboBox.SelectedItem = _selectedEnvCell.EnvironmentId.ToString();
            XCoordTextBox.Text = _selectedEnvCell.Position.X.ToString("F2");
            YCoordTextBox.Text = _selectedEnvCell.Position.Y.ToString("F2");
            ZCoordTextBox.Text = _selectedEnvCell.Position.Z.ToString("F2");
            // Update rotation and scale text boxes
        }

        private void ApplyObjectChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEnvCell == null) return;

            var newPosition = new Vector3(
                float.Parse(XCoordTextBox.Text),
                float.Parse(YCoordTextBox.Text),
                float.Parse(ZCoordTextBox.Text)
            );

            var command = new MoveEnvCellCommand(_dungeonLayout, _selectedEnvCell, newPosition);
            _commandManager.ExecuteCommand(command);

            // Apply rotation and scale changes

            UpdateViewports();
        }

        private void NewDungeon_Click(object sender, RoutedEventArgs e)
        {
            _dungeonLayout = new DungeonLayout();
            UpdateViewports();
            UpdateDungeonHierarchy();
        }

        private void OpenDungeon_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Dungeon Files (*.dungeon)|*.dungeon",
                DefaultExt = "dungeon"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _dungeonLayout = DungeonSerializer.LoadDungeon(openFileDialog.FileName);
                UpdateViewports();
                UpdateDungeonHierarchy();
            }
        }

        private void SaveDungeon_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Dungeon Files (*.dungeon)|*.dungeon",
                DefaultExt = "dungeon"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                DungeonSerializer.SaveDungeon(_dungeonLayout, saveFileDialog.FileName);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            _commandManager.Undo();
            UpdateViewports();
            UpdatePropertyPanel();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            _commandManager.Redo();
            UpdateViewports();
            UpdatePropertyPanel();
        }

        private void ToggleGrid_Click(object sender, RoutedEventArgs e)
        {
            MainViewport.ToggleGrid();
        }

        private void ManipulationTool_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                _currentMode = (ManipulationMode)Enum.Parse(typeof(ManipulationMode), radioButton.Tag.ToString());
            }
        }

        private void SnapToGrid_Checked(object sender, RoutedEventArgs e)
        {
            _snapToGrid = true;
        }

        private void SnapToGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            _snapToGrid = false;
        }

        private void ShowWireframe_Checked(object sender, RoutedEventArgs e)
        {
            _showWireframe = true;
            MainViewport.SetWireframeMode(true);
        }

        private void ShowWireframe_Unchecked(object sender, RoutedEventArgs e)
        {
            _showWireframe = false;
            MainViewport.SetWireframeMode(false);
        }

        private void ShowCollision_Checked(object sender, RoutedEventArgs e)
        {
            _showCollision = true;
            MainViewport.SetCollisionVisibility(true);
        }

        private void ShowCollision_Unchecked(object sender, RoutedEventArgs e)
        {
            _showCollision = false;
            MainViewport.SetCollisionVisibility(false);
        }

        private void AddEnvCell_Click(object sender, RoutedEventArgs e)
        {
            var addEnvCellDialog = new AddEnvCellDialog(_assetManager);
            if (addEnvCellDialog.ShowDialog() == true)
            {
                var newEnvCell = new EnvCell(addEnvCellDialog.SelectedEnvironmentId)
                {
                    Position = new Vector3(0, 0, 0),
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One
                };
                var command = new AddEnvCellCommand(_dungeonLayout, newEnvCell, "Root");
                _commandManager.ExecuteCommand(command);
                UpdateViewports();
                UpdateDungeonHierarchy();
            }
        }

        private void DeleteEnvCell_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEnvCell != null)
            {
                var command = new RemoveEnvCellCommand(_dungeonLayout, _selectedEnvCell, GetEnvCellPath(_selectedEnvCell));
                _commandManager.ExecuteCommand(command);
                _selectedEnvCell = null;
                UpdateViewports();
                UpdateDungeonHierarchy();
                UpdatePropertyPanel();
            }
        }

        private string GetEnvCellPath(EnvCell envCell)
        {
            // Implement this method to find the path of the EnvCell in the dungeon hierarchy
            return "Root";
        }

        private Vector3 CalculateNewPosition(Point currentPos)
        {
            // Implement this method to calculate the new position based on mouse movement and camera orientation
            // This is a placeholder implementation
            return new Vector3((float)currentPos.X, (float)currentPos.Y, 0);
        }
    }

    public enum ManipulationMode
    {
        Move,
        Rotate,
        Scale,
        Select
    }
}