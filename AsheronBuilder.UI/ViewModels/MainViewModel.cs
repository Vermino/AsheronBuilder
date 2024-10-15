// AsheronBuilder.UI/ViewModels/MainViewModel.cs

using AsheronBuilder.Core.Assets;
using AsheronBuilder.Core.Dungeon;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AsheronBuilder.Core;
using OpenTK.Mathematics;

namespace AsheronBuilder.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
        
        public void MoveArea(DungeonArea area, Vector3 newPosition)
        {
            Vector3 delta = newPosition - area.Position;
            area.Position = newPosition;
    
            // Update all EnvCells within the area
            foreach (var envCell in area.EnvCells)
            {
                envCell.Position += delta;
            }
    
            // Recursively update child areas
            foreach (var childArea in area.ChildAreas)
            {
                MoveArea(childArea, childArea.Position + delta);
            }
    
            OnPropertyChanged(nameof(DungeonLayout));
        }

        public void RenameArea(DungeonArea area, string newName)
        {
            area.Name = newName;
            OnPropertyChanged(nameof(DungeonLayout));
        }
    }
}