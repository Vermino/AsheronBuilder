using System.Collections.ObjectModel;
using AsheronBuilder.Core.Dungeon;
using System.Linq;

namespace AsheronBuilder.UI.ViewModels
{
    public class DungeonLayoutViewModel
    {
        private DungeonLayout _dungeonLayout;
        public ObservableCollection<AreaViewModel> Areas { get; } = new ObservableCollection<AreaViewModel>();

        public DungeonLayoutViewModel(DungeonLayout dungeonLayout)
        {
            _dungeonLayout = dungeonLayout;
            UpdateAreas();
        }

        public void UpdateAreas()
        {
            Areas.Clear();
            AddAreaToCollection(_dungeonLayout.Hierarchy.RootArea, null);
        }

        private void AddAreaToCollection(DungeonArea area, AreaViewModel parent)
        {
            var areaViewModel = new AreaViewModel(area);
            if (parent == null)
            {
                Areas.Add(areaViewModel);
            }
            else
            {
                parent.ChildAreas.Add(areaViewModel);
            }

            foreach (var childArea in area.ChildAreas)
            {
                AddAreaToCollection(childArea, areaViewModel);
            }

            foreach (var envCell in area.EnvCells)
            {
                areaViewModel.EnvCells.Add(new EnvCellViewModel(envCell));
            }
        }
    }

    public class AreaViewModel
    {
        public string Name { get; }
        public ObservableCollection<AreaViewModel> ChildAreas { get; } = new ObservableCollection<AreaViewModel>();
        public ObservableCollection<EnvCellViewModel> EnvCells { get; } = new ObservableCollection<EnvCellViewModel>();

        public AreaViewModel(DungeonArea area)
        {
            Name = area.Name;
        }
    }

    public class EnvCellViewModel
    {
        public uint Id { get; }
        public uint EnvironmentId { get; }

        public EnvCellViewModel(EnvCell envCell)
        {
            Id = envCell.Id;
            EnvironmentId = envCell.EnvironmentId;
        }
    }
}