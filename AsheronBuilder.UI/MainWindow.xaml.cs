// AsheronBuilder.UI/MainWindow.xaml.cs

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Rendering;
using AsheronBuilder.Core.Commands;
using AsheronBuilder.Core.Utils;
using OpenTK.Mathematics;
using Ookii.Dialogs.Wpf;
using CommandManager = AsheronBuilder.Core.Commands.CommandManager;
using ICommand = AsheronBuilder.Core.Commands.ICommand;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace AsheronBuilder.UI
{
    public partial class MainWindow : Window
    {
        private AssetManager _assetManager;
        private readonly string _assetsFolderPath;
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
            _assetsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            Directory.CreateDirectory(_assetsFolderPath);
            _assetManager = new AssetManager(_assetsFolderPath);
            
            _commandManager = new CommandManager();
            
            Loaded += MainWindow_Loaded;

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
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
        
            if (!_assetManager.AreDatFilesPresent())
            {
                PromptForDatFiles();
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                await InitializeAssetManager();
                InitializeDungeonLayout();
            
                _currentMode = ManipulationMode.Move;
                _snapToGrid = false;
                _showWireframe = false;
                _showCollision = false;
                await _assetManager.LoadAssetsAsync();

                SetupEventHandlers();
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred during initialization: {ex.Message}", ex);
                MessageBox.Show($"An error occurred during initialization: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void PromptForDatFiles()
        {
            var result = MessageBox.Show("DAT files are not present in the Assets folder. Would you like to select DAT files to load?", 
                "DAT Files Missing", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var dialog = new VistaFolderBrowserDialog
                {
                    Description = "Select folder containing DAT files"
                };

                if (dialog.ShowDialog() == true) // WPF-friendly true/false instead of WinForms' DialogResult
                {
                    CopyDatFiles(dialog.SelectedPath);
                }
            }
        }

        private async void CopyDatFiles(string sourceFolderPath)
        {
            try
            {
                var datFiles = Directory.GetFiles(sourceFolderPath, "*.dat");
                foreach (var file in datFiles)
                {
                    var destFile = Path.Combine(_assetsFolderPath, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }

                MessageBox.Show("DAT files have been successfully copied to the Assets folder.", "Files Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        
                // Show loading dialog
                var loadingDialog = new LoadingDialog();
                loadingDialog.Show();

                // Reload assets
                await _assetManager.LoadAssetsAsync();
        
                loadingDialog.Close();

                if (_assetManager.AreAssetsLoaded())
                {
                    UpdateAssetBrowser();
                }
                else
                {
                    UpdateUIForNoAssets();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while copying DAT files: {ex.Message}", "File Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
         
        private void UpdateAssetBrowser()
        {
            AssetBrowserTreeView.Items.Clear();

            var texturesNode = new System.Windows.Controls.TreeViewItem { Header = "Textures" };
            foreach (var textureId in _assetManager.GetAllTextureFileIds())
            {
                texturesNode.Items.Add(new System.Windows.Controls.TreeViewItem { Header = $"Texture {textureId}", Tag = textureId });
            }
            AssetBrowserTreeView.Items.Add(texturesNode);

            var modelsNode = new System.Windows.Controls.TreeViewItem { Header = "Models" };
            foreach (var modelId in _assetManager.GetAllModelFileIds())
            {
                modelsNode.Items.Add(new System.Windows.Controls.TreeViewItem { Header = $"Model {modelId}", Tag = modelId });
            }
            AssetBrowserTreeView.Items.Add(modelsNode);

            var environmentsNode = new System.Windows.Controls.TreeViewItem { Header = "Environments" };
            foreach (var environmentId in _assetManager.GetAllEnvironmentFileIds())
            {
                environmentsNode.Items.Add(new System.Windows.Controls.TreeViewItem { Header = $"Environment {environmentId}", Tag = environmentId });
            }
            AssetBrowserTreeView.Items.Add(environmentsNode);
        }

        private void UpdateUIForNoAssets()
        {
            AssetBrowserTreeView.Items.Clear();
            AssetBrowserTreeView.Items.Add(new System.Windows.Controls.TreeViewItem { Header = "No assets loaded" });
        }

        private async Task InitializeAssetManager()
        {
            try
            {
                _assetManager = new AssetManager(_assetsFolderPath);
                await _assetManager.LoadAssetsAsync();
                UpdateAssetBrowser();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error initializing or loading assets: {ex.Message}", ex);
                throw;
            }
        }
        
        private async void LoadAssetsAsync()
        {
            var loadingDialog = new LoadingDialog();
            loadingDialog.Show();

            try
            {
                await _assetManager.LoadAssetsAsync();
                UpdateAssetBrowser();
                Logger.Log("Assets loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading assets", ex);
                MessageBox.Show($"Error loading assets: {ex.Message}", "Asset Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loadingDialog.Close();
            }
        }

        private void SaveDungeon_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Dungeon Files (*.dungeon)|*.dungeon",
                DefaultExt = "dungeon"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    DungeonSerializer.SaveDungeon(_dungeonLayout, saveFileDialog.FileName);
                    Logger.Log($"Dungeon saved successfully to: {saveFileDialog.FileName}");
                    MessageBox.Show("Dungeon saved successfully!", "Save Dungeon", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to save dungeon", ex);
                    MessageBox.Show($"Failed to save dungeon: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenDungeon_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Dungeon Files (*.dungeon)|*.dungeon",
                DefaultExt = "dungeon"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _dungeonLayout = DungeonSerializer.LoadDungeon(openFileDialog.FileName);
                    UpdateViewports();
                    UpdateDungeonHierarchy();
                    Logger.Log($"Dungeon loaded successfully from: {openFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to load dungeon", ex);
                    MessageBox.Show($"Failed to load dungeon: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void InitializeDungeonLayout()
        {
            _dungeonLayout = new DungeonLayout();
            UpdateViewports();
        }

        private void SetupEventHandlers()
        {
            MainViewport.MouseMove += MainViewport_MouseMove;
            MainViewport.MouseDown += MainViewport_MouseDown;
            MainViewport.MouseUp += MainViewport_MouseUp;
            MainViewport.MouseWheel += MainViewport_MouseWheel;

            AssetBrowserTreeView.SelectedItemChanged += AssetBrowserTreeView_SelectedItemChanged;
            DungeonHierarchyTreeView.SelectedItemChanged += DungeonHierarchyTreeView_SelectedItemChanged;
            
            ResetViewButton.Click += ResetView_Click;
            TopViewButton.Click += TopView_Click;
            SelectObjectCheckBox.Checked += SelectObject_Checked;
            SelectObjectCheckBox.Unchecked += SelectObject_Unchecked;
            SelectVertexCheckBox.Checked += SelectVertex_Checked;
            SelectVertexCheckBox.Unchecked += SelectVertex_Unchecked;
            SelectFaceCheckBox.Checked += SelectFace_Checked;
            SelectFaceCheckBox.Unchecked += SelectFace_Unchecked;
        }
        
        private EnvCell PickObject(MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(MainViewport);
            Vector2 mousePos = mousePosition.ToVector2();
            return MainViewport.PickObject(mousePos);
        }
        
        private Vector3 CalculateNewPosition(Point currentPos)
        {
            Vector2 mousePos = new Vector2((float)currentPos.X, (float)currentPos.Y);
            Ray ray = MainViewport.Camera.GetPickingRay(mousePos, (float)MainViewport.ActualWidth, (float)MainViewport.ActualHeight);
            Plane groundPlane = new Plane(Vector3.UnitY, 0);

            if (ray.Intersects(groundPlane, out float distance))
            {
                return ray.Origin + ray.Direction * distance;
            }

            return _selectedEnvCell?.Position ?? Vector3.Zero;
        }

        private void SetPosition_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEnvCell != null)
            {
                Vector3 newPosition = new Vector3(
                    float.Parse(XCoordTextBox.Text),
                    float.Parse(YCoordTextBox.Text),
                    float.Parse(ZCoordTextBox.Text)
                );

                var command = new MoveEnvCellCommand(_dungeonLayout, _selectedEnvCell, newPosition);
                _commandManager.ExecuteCommand(command);
                UpdateViewports();
            }
        }

        public void LoadLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement landblock loading logic
            MessageBox.Show("LoadLandblock functionality not implemented yet.");
        }

        public void SaveLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement landblock saving logic
            MessageBox.Show("SaveLandblock functionality not implemented yet.");
        }

        public void ClearLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement landblock clearing logic
            MessageBox.Show("ClearLandblock functionality not implemented yet.");
        }

        public void GoToLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement navigation to a specific landblock
            MessageBox.Show("GoToLandblock functionality not implemented yet.");
        }

        private void SetScale_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEnvCell != null)
            {
                float scale = float.Parse(ScaleTextBox.Text);
                Vector3 newScale = new Vector3(scale, scale, scale);

                var command = new ScaleEnvCellCommand(_dungeonLayout, _selectedEnvCell, newScale);
                _commandManager.ExecuteCommand(command);
                UpdateViewports();
            }
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            MainViewport.Camera.Position = new Vector3(0, 5, 10);
            MainViewport.Camera.Yaw = -90f;
            MainViewport.Camera.Pitch = 0f;
            MainViewport.Camera.UpdateVectors();
            MainViewport.InvalidateVisual();
        }
        
        private void TopView_Click(object sender, RoutedEventArgs e)
        {
            MainViewport.Camera.Position = new Vector3(0, 20, 0);
            MainViewport.Camera.Yaw = -90f;
            MainViewport.Camera.Pitch = -90f;
            MainViewport.Camera.UpdateVectors();
            MainViewport.InvalidateVisual();
        }

        private void SelectObject_Checked(object sender, RoutedEventArgs e)
        {
            _currentMode = ManipulationMode.Select;
        }

        private void SelectObject_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentMode == ManipulationMode.Select)
            {
                _currentMode = ManipulationMode.Move;
            }
        }

        private void SelectVertex_Checked(object sender, RoutedEventArgs e)
        {
            // Implement vertex selection mode
        }

        private void SelectVertex_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable vertex selection mode
        }

        private void SelectFace_Checked(object sender, RoutedEventArgs e)
        {
            // Implement face selection mode
        }

        private void SelectFace_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable face selection mode
        }

        private void MainViewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(MainViewport);
                float deltaX = (float)(currentPos.X - _lastMousePosition.X);
                float deltaY = (float)(currentPos.Y - _lastMousePosition.Y);
        
                MainViewport.Camera.HandleMouseInput(deltaX, deltaY);
                MainViewport.InvalidateVisual();
        
                _lastMousePosition = currentPos;
            }
            else if (e.LeftButton == MouseButtonState.Pressed && _currentMode == ManipulationMode.Move && _selectedEnvCell != null)
            {
                Vector3 newPosition = CalculateNewPosition(e.GetPosition(MainViewport));
                var command = new MoveEnvCellCommand(_dungeonLayout, _selectedEnvCell, newPosition);
                _commandManager.ExecuteCommand(command);
                UpdateViewports();
            }
        }

        private void UpdateViewports()
        {
            MainViewport.SetDungeonLayout(_dungeonLayout);
            MiniMap.SetDungeonLayout(_dungeonLayout);
        }

        private void UpdateDungeonHierarchy()
        {
            DungeonHierarchyTreeView.Items.Clear();
            AddAreaToTreeView(_dungeonLayout.Hierarchy.RootArea, null);
        }

        private void AddAreaToTreeView(DungeonArea area, System.Windows.Controls.TreeViewItem parentItem)
        {
            var areaItem = new System.Windows.Controls.TreeViewItem { Header = area.Name, Tag = area };

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
                var envCellItem = new System.Windows.Controls.TreeViewItem { Header = $"EnvCell {envCell.Id}", Tag = envCell };
                areaItem.Items.Add(envCellItem);
            }
        }

        private void MainViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMousePosition = e.GetPosition(MainViewport);
                if (_currentMode == ManipulationMode.Select)
                {
                    _selectedEnvCell = PickObject(e);
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
            MainViewport.Camera.MoveForward(e.Delta * 0.001f);
            MainViewport.InvalidateVisual();
        }

        private void AssetBrowserTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is System.Windows.Controls.TreeViewItem item && item.Tag is uint assetId)
            {
                // Display asset properties or preview
            }
        }

        private void DungeonHierarchyTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is System.Windows.Controls.TreeViewItem item)
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
            if (sender is System.Windows.Controls.RadioButton radioButton)
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
        
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                MainViewport.Camera.MoveUp(0.1f);
            }
            else if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                MainViewport.Camera.MoveUp(-0.1f);
            }
            MainViewport.InvalidateVisual();
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
    }
    
    public class ScaleEnvCellCommand : ICommand
    {
        private readonly DungeonLayout _dungeonLayout;
        private readonly EnvCell _envCell;
        private readonly Vector3 _oldScale;
        private readonly Vector3 _newScale;

        public ScaleEnvCellCommand(DungeonLayout dungeonLayout, EnvCell envCell, Vector3 newScale)
        {
            _dungeonLayout = dungeonLayout;
            _envCell = envCell;
            _oldScale = envCell.Scale;
            _newScale = newScale;
        }

        public void Execute()
        {
            _envCell.Scale = _newScale;
            _dungeonLayout.UpdateEnvCell(_envCell);
        }

        public void Undo()
        {
            _envCell.Scale = _oldScale;
            _dungeonLayout.UpdateEnvCell(_envCell);
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