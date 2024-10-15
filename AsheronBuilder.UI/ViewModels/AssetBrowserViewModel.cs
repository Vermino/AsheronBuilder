using System.Collections.ObjectModel;
using AsheronBuilder.Core.Assets;

namespace AsheronBuilder.UI.ViewModels
{
    public class AssetBrowserViewModel
    {
        private AssetManager _assetManager;
        public ObservableCollection<AssetCategoryViewModel> Categories { get; } = new ObservableCollection<AssetCategoryViewModel>();

        public AssetBrowserViewModel(AssetManager assetManager)
        {
            _assetManager = assetManager;
            LoadAssets();
        }
        // TODO Need help connecting the DAT manager to Trevis DatReaderWriter Library
        private void LoadAssets()
        {
            Categories.Add(new AssetCategoryViewModel("Textures", new ObservableCollection<uint>(_assetManager.GetTextureFileIds())));
            Categories.Add(new AssetCategoryViewModel("Models", new ObservableCollection<uint>(_assetManager.GetModelFileIds())));
            Categories.Add(new AssetCategoryViewModel("Environments", new ObservableCollection<uint>(_assetManager.GetEnvironmentFileIds())));
        }
    }

    public class AssetCategoryViewModel
    {
        public string Name { get; }
        public ObservableCollection<uint> Assets { get; }

        public AssetCategoryViewModel(string name, ObservableCollection<uint> assets)
        {
            Name = name;
            Assets = assets;
        }
    }
}