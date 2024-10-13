using System.Collections.ObjectModel;

namespace ACDungeonBuilder.UI.ViewModels
{
    public class AssetBrowserViewModel
    {
        public ObservableCollection<AssetCategoryViewModel> Categories { get; } = new ObservableCollection<AssetCategoryViewModel>();

        public AssetBrowserViewModel()
        {
            // TODO: Implement asset loading logic
            Categories.Add(new AssetCategoryViewModel("Textures", new ObservableCollection<uint>()));
            Categories.Add(new AssetCategoryViewModel("Models", new ObservableCollection<uint>()));
            Categories.Add(new AssetCategoryViewModel("Environments", new ObservableCollection<uint>()));
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