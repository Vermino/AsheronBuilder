// AsheronBuilder.UI/MainWindow.xaml.cs
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Rendering;
using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;

namespace AsheronBuilder.UI
{
    public partial class MainWindow : Window
    {
        private AssetManager _assetManager;
        private DungeonLayout _dungeonLayout;
        private ManipulationMode _currentMode;
        private bool _snapToGrid;
        private EnvCell _selectedEnvCell;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAssetManager();
            InitializeDungeonLayout();
            LoadAssets();
            
            _currentMode = ManipulationMode.Move;
            _snapToGrid = false;
            MoveButton.IsChecked = true;
        }

        // TODO Need help connecting the DAT manager to Trevis DatReaderWriter Library
        private void InitializeAssetManager()
        {
            string datFilePath = @"C:\Users\Vermino\RiderProjects\AsheronBuilder\Assets";
            if (!Directory.Exists(datFilePath))
            {
                throw new DirectoryNotFoundException($"The directory '{datFilePath}' does not exist.");
            }
            _assetManager = new AssetManager(datFilePath);
        }

        private void InitializeDungeonLayout()
        {
            _dungeonLayout = new DungeonLayout();
            UpdateHierarchyTreeView();
        }
        
        // TODO Need help connecting the DAT manager to Trevis DatReaderWriter Library
        private void LoadAssets()
        {
            var textures = _assetManager.GetTextureFileIds();
            var models = _assetManager.GetModelFileIds();
            var environments = _assetManager.GetEnvironmentFileIds();
        
            var texturesNode = new TreeViewItem { Header = "Textures" };
            foreach (var textureId in textures)
            {
                texturesNode.Items.Add(new TreeViewItem { Header = $"Texture {textureId}" });
            }
            AssetTreeView.Items.Add(texturesNode);
        
            var modelsNode = new TreeViewItem { Header = "Models" };
            foreach (var modelId in models)
            {
                modelsNode.Items.Add(new TreeViewItem { Header = $"Model {modelId}" });
            }
            AssetTreeView.Items.Add(modelsNode);
        
            var environmentsNode = new TreeViewItem { Header = "Environments" };
            foreach (var environmentId in environments)
            {
                environmentsNode.Items.Add(new TreeViewItem { Header = $"Environment {environmentId}" });
            }
            AssetTreeView.Items.Add(environmentsNode);
        }

        private void UpdateHierarchyTreeView()
        {
            HierarchyTreeView.Items.Clear();
            AddAreaToTreeView(_dungeonLayout.Hierarchy.RootArea, null);
        }

        private void AddAreaToTreeView(DungeonArea area, TreeViewItem parentItem)
        {
            var areaItem = new TreeViewItem { Header = area.Name, Tag = area };

            if (parentItem == null)
            {
                HierarchyTreeView.Items.Add(areaItem);
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

        private void OpenGLControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(OpenGLControl);
                _selectedEnvCell = PickEnvCell(mousePosition);
                if (_selectedEnvCell != null)
                {
                    UpdatePropertiesPanel(_selectedEnvCell);
                }
            }
            OpenGLControl.Focus();
        }

        private void OpenGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            ((OpenGLControl)sender).OnMouseMove(sender, e);
            if (e.LeftButton == MouseButtonState.Pressed && _selectedEnvCell != null)
            {
                var mousePosition = e.GetPosition(OpenGLControl);
                var worldPosition = ScreenToWorldPosition(mousePosition);

                switch (_currentMode)
                {
                    case ManipulationMode.Move:
                        MoveSelectedEnvCell(worldPosition);
                        break;
                    case ManipulationMode.Rotate:
                        RotateSelectedEnvCell(worldPosition);
                        break;
                    case ManipulationMode.Scale:
                        ScaleSelectedEnvCell(worldPosition);
                        break;
                }
            }
        }

        private void OpenGLControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((OpenGLControl)sender).OnMouseUp(sender, e);
            _selectedEnvCell = null;
        }

        private void OpenGLControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ((OpenGLControl)sender).OnMouseLeave(sender, e);
        }

        private void OpenGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            ((OpenGLControl)sender).OnKeyDown(sender, e);
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteSelectedEnvCell();
                    break;
            }
        }

        private void OpenGLControl_KeyUp(object sender, KeyEventArgs e)
        {
            ((OpenGLControl)sender).OnKeyUp(sender, e);
        }

        private void ManipulationMode_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == MoveButton) _currentMode = ManipulationMode.Move;
            else if (sender == RotateButton) _currentMode = ManipulationMode.Rotate;
            else if (sender == ScaleButton) _currentMode = ManipulationMode.Scale;
        }

        private void SnapToGridButton_Checked(object sender, RoutedEventArgs e)
        {
            _snapToGrid = true;
        }

        private void SnapToGridButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _snapToGrid = false;
        }

        private EnvCell PickEnvCell(Point mousePosition)
        {
            // Implement ray casting to pick EnvCell
            // This is a placeholder implementation
            return _dungeonLayout.Hierarchy.RootArea.GetAllAreas()
                .SelectMany(a => a.EnvCells)
                .FirstOrDefault();
        }

        private System.Numerics.Vector3 ScreenToWorldPosition(Point screenPosition)
        {
            // Implement screen to world position conversion
            // This is a placeholder implementation
            return new System.Numerics.Vector3((float)screenPosition.X, (float)screenPosition.Y, 0);
        }

        private void MoveSelectedEnvCell(System.Numerics.Vector3 worldPosition)
        {
            if (_snapToGrid)
            {
                worldPosition = SnapToGrid(worldPosition);
            }
            _selectedEnvCell.Position = worldPosition;
            UpdatePropertiesPanel(_selectedEnvCell);
            ((OpenGLControl)OpenGLControl).Invalidate();
        }

        private void RotateSelectedEnvCell(System.Numerics.Vector3 worldPosition)
        {
            // Implement rotation logic
            // This is a placeholder implementation
            var rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(0.1f, 0, 0);
            _selectedEnvCell.Rotation = rotation;
            UpdatePropertiesPanel(_selectedEnvCell);
            ((OpenGLControl)OpenGLControl).Invalidate();
        }

        private void ScaleSelectedEnvCell(System.Numerics.Vector3 worldPosition)
        {
            // Implement scaling logic
            // This is a placeholder implementation
            var scale = new System.Numerics.Vector3(1.1f, 1.1f, 1.1f);
            _selectedEnvCell.Scale = scale;
            UpdatePropertiesPanel(_selectedEnvCell);
            ((OpenGLControl)OpenGLControl).Invalidate();
        }

        private void DeleteSelectedEnvCell()
        {
            if (_selectedEnvCell != null)
            {
                _dungeonLayout.RemoveEnvCell(_selectedEnvCell.Id);
                _selectedEnvCell = null;
                UpdateHierarchyTreeView();
                ClearPropertiesPanel();
                ((OpenGLControl)OpenGLControl).Invalidate();
            }
        }

        private System.Numerics.Vector3 SnapToGrid(System.Numerics.Vector3 position)
        {
            float gridSize = 1.0f; // Adjust this value to change the grid size
            return new System.Numerics.Vector3(
                (float)Math.Round(position.X / gridSize) * gridSize,
                (float)Math.Round(position.Y / gridSize) * gridSize,
                (float)Math.Round(position.Z / gridSize) * gridSize
            );
        }

        private void UpdatePropertiesPanel(EnvCell envCell)
        {
            PropertiesPanel.Children.Clear();
            PropertiesPanel.Children.Add(new TextBlock { Text = "EnvCell Properties", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });

            AddProperty("ID", envCell.Id.ToString(), false);
            AddProperty("Environment ID", envCell.EnvironmentId.ToString());
            AddProperty("Position X", envCell.Position.X.ToString());
            AddProperty("Position Y", envCell.Position.Y.ToString());
            AddProperty("Position Z", envCell.Position.Z.ToString());
            AddProperty("Rotation X", envCell.Rotation.X.ToString());
            AddProperty("Rotation Y", envCell.Rotation.Y.ToString());
            AddProperty("Rotation Z", envCell.Rotation.Z.ToString());
            AddProperty("Rotation W", envCell.Rotation.W.ToString());
            AddProperty("Scale X", envCell.Scale.X.ToString());
            AddProperty("Scale Y", envCell.Scale.Y.ToString());
            AddProperty("Scale Z", envCell.Scale.Z.ToString());
        }

        private void AddProperty(string label, string value, bool isEditable = true)
        {
            var container = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            container.Children.Add(new TextBlock { Text = label + ": ", Width = 100 });
            if (isEditable)
            {
                var textBox = new TextBox { Text = value, Width = 100 };
                textBox.TextChanged += (sender, e) => UpdateEnvCellProperty(label, ((TextBox)sender).Text);
                container.Children.Add(textBox);
            }
            else
            {
                container.Children.Add(new TextBlock { Text = value, Width = 100 });
            }
            PropertiesPanel.Children.Add(container);
        }

        private void UpdateEnvCellProperty(string property, string value)
        {
            if (_selectedEnvCell == null) return;

            if (float.TryParse(value, out float floatValue))
            {
                switch (property)
                {
                    case "Environment ID":
                        _selectedEnvCell.EnvironmentId = (uint)floatValue;
                        break;
                    case "Position X":
                        _selectedEnvCell.Position = new System.Numerics.Vector3(floatValue, _selectedEnvCell.Position.Y, _selectedEnvCell.Position.Z);
                        break;
                    case "Position Y":
                        _selectedEnvCell.Position = new System.Numerics.Vector3(_selectedEnvCell.Position.X, floatValue, _selectedEnvCell.Position.Z);
                        break;
                    case "Position Z":
                        _selectedEnvCell.Position = new System.Numerics.Vector3(_selectedEnvCell.Position.X, _selectedEnvCell.Position.Y, floatValue);
                        break;
                    case "Rotation X":
                        _selectedEnvCell.Rotation = new System.Numerics.Quaternion(floatValue, _selectedEnvCell.Rotation.Y, _selectedEnvCell.Rotation.Z, _selectedEnvCell.Rotation.W);
                        break;
                    case "Rotation Y":
                        _selectedEnvCell.Rotation = new System.Numerics.Quaternion(_selectedEnvCell.Rotation.X, floatValue, _selectedEnvCell.Rotation.Z, _selectedEnvCell.Rotation.W);
                        break;
                    case "Rotation Z":
                        _selectedEnvCell.Rotation = new System.Numerics.Quaternion(_selectedEnvCell.Rotation.X, _selectedEnvCell.Rotation.Y, floatValue, _selectedEnvCell.Rotation.W);
                        break;
                    case "Rotation W":
                        _selectedEnvCell.Rotation = new System.Numerics.Quaternion(_selectedEnvCell.Rotation.X, _selectedEnvCell.Rotation.Y, _selectedEnvCell.Rotation.Z, floatValue);
                        break;
                    case "Scale X":
                        _selectedEnvCell.Scale = new System.Numerics.Vector3(floatValue, _selectedEnvCell.Scale.Y, _selectedEnvCell.Scale.Z);
                        break;
                    case "Scale Y":
                        _selectedEnvCell.Scale = new System.Numerics.Vector3(_selectedEnvCell.Scale.X, floatValue, _selectedEnvCell.Scale.Z);
                        break;
                    case "Scale Z":
                        _selectedEnvCell.Scale = new System.Numerics.Vector3(_selectedEnvCell.Scale.X, _selectedEnvCell.Scale.Y, floatValue);
                        break;
                }

                _dungeonLayout.UpdateEnvCell(_selectedEnvCell);
                ((OpenGLControl)OpenGLControl).Invalidate();
            }
        }

        private void ClearPropertiesPanel()
        {
            PropertiesPanel.Children.Clear();
            PropertiesPanel.Children.Add(new TextBlock { Text = "No EnvCell selected" });
        }

        private void HierarchyTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem != null)
            {
                if (selectedItem.Tag is DungeonArea)
                {
                    EnableAreaContextMenu();
                }
                else if (selectedItem.Tag is EnvCell)
                {
                    EnableEnvCellContextMenu();
                    _selectedEnvCell = selectedItem.Tag as EnvCell;
                    UpdatePropertiesPanel(_selectedEnvCell);
                }
            }
        }

        private void EnableAreaContextMenu()
        {
            AreaContextMenu.IsEnabled = true;
        }

        private void EnableEnvCellContextMenu()
        {
            AreaContextMenu.IsEnabled = false;
        }

        private void NewDungeon_Click(object sender, RoutedEventArgs e)
        {
            _dungeonLayout = new DungeonLayout();
            UpdateHierarchyTreeView();
            ClearPropertiesPanel();
            ((OpenGLControl)OpenGLControl).Invalidate();
        }

        private void LoadDungeon_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Dungeon Files (*.dungeon)|*.dungeon",
                DefaultExt = "dungeon"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _dungeonLayout = DungeonSerializer.LoadDungeon(openFileDialog.FileName);
                UpdateHierarchyTreeView();
                ClearPropertiesPanel();
                ((OpenGLControl)OpenGLControl).Invalidate();
                MessageBox.Show("Dungeon loaded successfully.", "Load Dungeon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveDungeon_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Dungeon Files (*.dungeon)|*.dungeon",
                DefaultExt = "dungeon",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                DungeonSerializer.SaveDungeon(_dungeonLayout, saveFileDialog.FileName);
                MessageBox.Show("Dungeon saved successfully.", "Save Dungeon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            // Implement undo functionality
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            // Implement redo functionality
        }

        private void ToggleGrid_Click(object sender, RoutedEventArgs e)
        {
            // Implement grid toggling
        }

        private void RenameArea_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = HierarchyTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag is DungeonArea area)
            {
                var dialog = new RenameDialog(area.Name);
                if (dialog.ShowDialog() == true)
                {
                    string oldPath = GetAreaPath(selectedItem);
                    _dungeonLayout.Hierarchy.RenameArea(oldPath, dialog.NewName);
                    UpdateHierarchyTreeView();
                }
            }
        }

        private void MoveArea_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = HierarchyTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag is DungeonArea area)
            {
                var dialog = new MoveDialog(GetAreaPath(selectedItem), _dungeonLayout.Hierarchy);
                if (dialog.ShowDialog() == true)
                {
                    string sourcePath = GetAreaPath(selectedItem);
                    _dungeonLayout.Hierarchy.MoveArea(sourcePath, dialog.DestinationPath);
                    UpdateHierarchyTreeView();
                }
            }
        }

        private void AddEnvCell_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = HierarchyTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag is DungeonArea area)
            {
                var envCell = new EnvCell(1); // Use a default EnvironmentId of 1
                string areaPath = GetAreaPath(selectedItem);
                _dungeonLayout.AddEnvCell(envCell, areaPath);
                UpdateHierarchyTreeView();
            }
        }

        private void AddArea_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Add Area", "Enter area name:");
            if (dialog.ShowDialog() == true)
            {
                var selectedItem = HierarchyTreeView.SelectedItem as TreeViewItem;
                string parentPath = selectedItem != null ? GetAreaPath(selectedItem) : "";
                _dungeonLayout.Hierarchy.GetOrCreateArea(parentPath + "/" + dialog.InputText);
                UpdateHierarchyTreeView();
            }
        }

        private string GetAreaPath(TreeViewItem item)
        {
            var path = new List<string>();
            while (item != null && item.Tag is DungeonArea)
            {
                path.Insert(0, ((DungeonArea)item.Tag).Name);
                item = item.Parent as TreeViewItem;
            }
            return string.Join("/", path);
        }
    }

    public enum ManipulationMode
    {
        Move,
        Rotate,
        Scale
    }
}