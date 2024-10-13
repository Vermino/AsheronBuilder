// ACDungeonBuilder.UI/MainWindow.xaml.cs

using System.Windows;
using ACDungeonBuilder.Core.Assets;
using ACDungeonBuilder.Core.Dungeon;
using ACDungeonBuilder.Rendering;

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
    }
}