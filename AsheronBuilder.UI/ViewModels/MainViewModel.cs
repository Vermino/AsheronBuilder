// AsheronBuilder.UI/ViewModels/MainViewModel.cs
using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using System.IO;

namespace AsheronBuilder.UI.ViewModels
{
    public class MainViewModel
    {
        public AssetBrowserViewModel AssetBrowser { get; }
        public DungeonLayoutViewModel DungeonLayout { get; }

        private AssetManager _assetManager;
        private DungeonLayout _dungeonLayout;

        public MainViewModel()
        {
            string datFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DatFiles");
            _assetManager = new AssetManager(datFilePath);
            _dungeonLayout = new DungeonLayout();

            AssetBrowser = new AssetBrowserViewModel(_assetManager);
            DungeonLayout = new DungeonLayoutViewModel(_dungeonLayout);
        }

        public void NewDungeon()
        {
            _dungeonLayout = new DungeonLayout();
            DungeonLayout.UpdateAreas();
        }

        public void LoadDungeon(string filePath)
        {
            _dungeonLayout = DungeonSerializer.LoadDungeon(filePath);
            DungeonLayout.UpdateAreas();
        }

        public void SaveDungeon(string filePath)
        {
            DungeonSerializer.SaveDungeon(_dungeonLayout, filePath);
        }

        public void AddEnvCell(uint environmentId, string areaPath)
        {
            var envCell = new EnvCell(environmentId);
            _dungeonLayout.AddEnvCell(envCell, areaPath);
            DungeonLayout.UpdateAreas();
        }

        public void RemoveEnvCell(uint envCellId)
        {
            _dungeonLayout.RemoveEnvCell(envCellId);
            DungeonLayout.UpdateAreas();
        }

        public void MoveArea(string sourcePath, string destinationPath)
        {
            _dungeonLayout.Hierarchy.MoveArea(sourcePath, destinationPath);
            DungeonLayout.UpdateAreas();
        }

        public void RenameArea(string path, string newName)
        {
            _dungeonLayout.Hierarchy.RenameArea(path, newName);
            DungeonLayout.UpdateAreas();
        }
    }
}