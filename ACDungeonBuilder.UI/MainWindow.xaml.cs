using System;
using System.Diagnostics;
using System.Windows;
using ACDungeonBuilder.Core.Assets;
using ACDungeonBuilder.Core.Dungeon;
using ACDungeonBuilder.Rendering;
using Microsoft.Win32;

namespace ACDungeonBuilder.UI
{
    public partial class MainWindow : Window
    {
        private AssetManager _assetManager;
        private DungeonLayout _dungeonLayout;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAssetManager();
            LoadAssets();
            
        }

        private void InitializeAssetManager()
        {
            _assetManager = new AssetManager();
        }

        private void LoadAssets()
        {
            var textures = _assetManager.GetTextureFileIds();
            var models = _assetManager.GetModelFileIds();
            var environments = _assetManager.GetEnvironmentFileIds();

            // Populate AssetTreeView with the loaded assets
            var texturesNode = new System.Windows.Controls.TreeViewItem { Header = "Textures" };
            foreach (var textureId in textures)
            {
                texturesNode.Items.Add(new System.Windows.Controls.TreeViewItem { Header = $"Texture {textureId}" });
            }
            AssetTreeView.Items.Add(texturesNode);

            var modelsNode = new System.Windows.Controls.TreeViewItem { Header = "Models" };
            foreach (var modelId in models)
            {
                modelsNode.Items.Add(new System.Windows.Controls.TreeViewItem { Header = $"Model {modelId}" });
            }
            AssetTreeView.Items.Add(modelsNode);

            var environmentsNode = new System.Windows.Controls.TreeViewItem { Header = "Environments" };
            foreach (var environmentId in environments)
            {
                environmentsNode.Items.Add(new System.Windows.Controls.TreeViewItem { Header = $"Environment {environmentId}" });
            }
            AssetTreeView.Items.Add(environmentsNode);
        }

        private void LoadEnvironmentButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Load Environment button clicked");
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Raw Environment Files (*.raw)|*.raw"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Debug.WriteLine($"Selected file: {filePath}");
                
                try
                {
                    var environmentData = EnvironmentLoader.LoadFromRawFile(filePath);
                    Debug.WriteLine($"Environment data loaded: {environmentData.Vertices.Count} vertices, {environmentData.Indices.Count} indices");
                    
                    OpenGLControl.LoadEnvironment(environmentData);
                    Debug.WriteLine("Environment data passed to OpenGLControl");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading environment: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    MessageBox.Show($"Error loading environment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                Debug.WriteLine("File selection cancelled");
            }
        }
    }
}