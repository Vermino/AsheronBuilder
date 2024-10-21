// AsheronBuilder.UI/ViewModels/AssetBrowserViewModel.cs

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

        private void LoadAssets()
        {
            Categories.Add(new AssetCategoryViewModel("Textures", new ObservableCollection<uint>(_assetManager.GetAllTextureFileIds())));
            Categories.Add(new AssetCategoryViewModel("Models", new ObservableCollection<uint>(_assetManager.GetAllModelFileIds())));
            Categories.Add(new AssetCategoryViewModel("Environments", new ObservableCollection<uint>(_assetManager.GetAllEnvironmentFileIds())));
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