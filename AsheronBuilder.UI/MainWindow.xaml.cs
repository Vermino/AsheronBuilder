using System;
using System.Windows;
using System.Windows.Controls;
using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Rendering;
using Microsoft.Win32;
using SelectionMode = AsheronBuilder.Core.SelectionMode;

namespace AsheronBuilder.UI
{
    public partial class MainWindow : Window
    {
        private AssetManager _assetManager;
        private DungeonLayout _dungeonLayout;
        private ManipulationMode _currentMode;
        private bool _snapToGrid;
        private bool _showWireframe;
        private bool _showCollision;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                InitializeAssetManager();
                InitializeDungeonLayout();
                LoadAssets();
    
                _currentMode = ManipulationMode.Move;
                _snapToGrid = false;
                _showWireframe = false;
                _showCollision = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during initialization: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeAssetManager()
        {
            _assetManager = new AssetManager("C:/Users/Vermino/RiderProjects/AsheronBuilder/Assets");
        }

        private void InitializeDungeonLayout()
        {
            _dungeonLayout = new DungeonLayout();
        }
        
        
        private void LoadAssets()
        {
            // Load assets using _assetManager
            // For example:
            // var textureIds = _assetManager.GetTextureFileIds();
            // foreach (var id in textureIds)
            // {
            //     _assetManager.LoadTexture(id);
            // }
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            MainViewport.ResetCamera();
        }

        private void TopView_Click(object sender, RoutedEventArgs e)
        {
            MainViewport.SetTopView();
        }

        private void SelectObject_Checked(object sender, RoutedEventArgs e)
        {
            MainViewport.SetSelectionMode(SelectionMode.Object);
        }

        private void SelectObject_Unchecked(object sender, RoutedEventArgs e)
        {
            MainViewport.SetSelectionMode(SelectionMode.None);
        }

        private void SelectVertex_Checked(object sender, RoutedEventArgs e)
        {
            MainViewport.SetSelectionMode(SelectionMode.Vertex);
        }

        private void SelectVertex_Unchecked(object sender, RoutedEventArgs e)
        {
            MainViewport.SetSelectionMode(SelectionMode.None);
        }

        private void SelectFace_Checked(object sender, RoutedEventArgs e)
        {
            MainViewport.SetSelectionMode(SelectionMode.Face);
        }

        private void SelectFace_Unchecked(object sender, RoutedEventArgs e)
        {
            MainViewport.SetSelectionMode(SelectionMode.None);
        }

        private void ApplyObjectChanges_Click(object sender, RoutedEventArgs e)
        {
            // Implement applying changes to the selected object
            string objectName = ObjectNameTextBox.Text;
            string objectType = ObjectTypeComboBox.SelectedItem as string;
            string material = MaterialComboBox.SelectedItem as string;

            // Apply changes to the selected object in the scene
            MainViewport.ApplyObjectChanges(objectName, objectType, material);
        }

        private void SetPosition_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(XCoordTextBox.Text, out float x) &&
                float.TryParse(YCoordTextBox.Text, out float y) &&
                float.TryParse(ZCoordTextBox.Text, out float z))
            {
                MainViewport.SetSelectedObjectPosition(x, y, z);
            }
            else
            {
                MessageBox.Show("Invalid coordinate values. Please enter valid numbers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetScale_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(ScaleTextBox.Text, out float scale))
            {
                MainViewport.SetSelectedObjectScale(scale);
            }
            else
            {
                MessageBox.Show("Invalid scale value. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToLandblock_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(LandblockIdTextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out uint landblockId))
            {
                // Implement navigation to the specified landblock
                MainViewport.GoToLandblock(landblockId);
            }
            else
            {
                MessageBox.Show("Invalid landblock ID. Please enter a valid hexadecimal number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        
        private void NewDungeon_Click(object sender, RoutedEventArgs e)
        {
            // Implement new dungeon creation
        }

        private void OpenDungeon_Click(object sender, RoutedEventArgs e)
        {
            // Implement dungeon opening
        }

        private void SaveDungeon_Click(object sender, RoutedEventArgs e)
        {
            // Implement dungeon saving
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

        private void ManipulationTool_Checked(object sender, RoutedEventArgs e)
        {
            // Implement manipulation tool selection
        }

        private void SnapToGrid_Checked(object sender, RoutedEventArgs e)
        {
            _snapToGrid = true;
        }

        private void SnapToGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            _snapToGrid = false;
        }

        private void LoadLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement landblock loading
        }

        private void SaveLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement landblock saving
        }

        private void ClearLandblock_Click(object sender, RoutedEventArgs e)
        {
            // Implement landblock clearing
        }
    }

    public enum ManipulationMode
    {
        Move,
        Rotate,
        Scale
    }
}