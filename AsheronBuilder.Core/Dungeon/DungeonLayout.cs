// AsheronBuilder.Core/Dungeon/DungeonLayout.cs
using System.Collections.Generic;
using System.Numerics;

namespace AsheronBuilder.Core.Dungeon
{
    public class DungeonLayout
    {
        public DungeonHierarchy Hierarchy { get; private set; }
        private Dictionary<uint, EnvCell> _envCells;
        private uint _nextEnvCellId;

        public DungeonLayout()
        {
            Hierarchy = new DungeonHierarchy();
            _envCells = new Dictionary<uint, EnvCell>();
            _nextEnvCellId = 1;
        }

        public void AddEnvCell(EnvCell envCell, string areaPath)
        {
            envCell.Id = _nextEnvCellId++;
            _envCells[envCell.Id] = envCell;
            Hierarchy.AddEnvCell(envCell, areaPath);
        }

        public void RemoveEnvCell(uint envCellId)
        {
            if (_envCells.TryGetValue(envCellId, out var envCell))
            {
                _envCells.Remove(envCellId);
                Hierarchy.RemoveEnvCell(envCell);
            }
        }

        public EnvCell GetEnvCellById(uint envCellId)
        {
            return _envCells.TryGetValue(envCellId, out var envCell) ? envCell : null;
        }

        public void UpdateEnvCell(EnvCell envCell)
        {
            if (_envCells.ContainsKey(envCell.Id))
            {
                _envCells[envCell.Id] = envCell;
            }
        }

        public IEnumerable<EnvCell> GetAllEnvCells()
        {
            return _envCells.Values;
        }
    }

    public class EnvCell
    {
        public uint Id { get; set; }
        public uint EnvironmentId { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public EnvCell(uint environmentId)
        {
            EnvironmentId = environmentId;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }

    public class DungeonHierarchy
    {
        public DungeonArea RootArea { get; private set; }

        public DungeonHierarchy()
        {
            RootArea = new DungeonArea("Root");
        }

        public void AddEnvCell(EnvCell envCell, string areaPath)
        {
            var area = GetOrCreateArea(areaPath);
            area.AddEnvCell(envCell);
        }

        public void RemoveEnvCell(EnvCell envCell)
        {
            RootArea.RemoveEnvCell(envCell);
        }

        public DungeonArea GetOrCreateArea(string path)
        {
            var pathParts = path.Split('/');
            var currentArea = RootArea;

            foreach (var part in pathParts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                var childArea = currentArea.GetChildArea(part);
                if (childArea == null)
                {
                    childArea = new DungeonArea(part);
                    currentArea.AddChildArea(childArea);
                }
                currentArea = childArea;
            }

            return currentArea;
        }

        public void RenameArea(string oldPath, string newName)
        {
            var area = GetArea(oldPath);
            if (area != null)
            {
                area.Name = newName;
            }
        }

        public void MoveArea(string sourcePath, string destinationPath)
        {
            var sourceArea = GetArea(sourcePath);
            var destinationArea = GetArea(destinationPath);

            if (sourceArea != null && destinationArea != null)
            {
                var parentPath = GetParentPath(sourcePath);
                var parentArea = GetArea(parentPath);

                if (parentArea != null)
                {
                    parentArea.RemoveChildArea(sourceArea);
                    destinationArea.AddChildArea(sourceArea);
                }
            }
        }

        private DungeonArea GetArea(string path)
        {
            var pathParts = path.Split('/');
            var currentArea = RootArea;

            foreach (var part in pathParts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                currentArea = currentArea.GetChildArea(part);
                if (currentArea == null) return null;
            }

            return currentArea;
        }

        private string GetParentPath(string path)
        {
            var lastSeparatorIndex = path.LastIndexOf('/');
            return lastSeparatorIndex > 0 ? path.Substring(0, lastSeparatorIndex) : "";
        }
    }

    public class DungeonArea
    {
        public string Name { get; set; }
        public List<DungeonArea> ChildAreas { get; private set; }
        public List<EnvCell> EnvCells { get; private set; }

        public DungeonArea(string name)
        {
            Name = name;
            ChildAreas = new List<DungeonArea>();
            EnvCells = new List<EnvCell>();
        }

        public void AddChildArea(DungeonArea area)
        {
            ChildAreas.Add(area);
        }

        public void RemoveChildArea(DungeonArea area)
        {
            ChildAreas.Remove(area);
        }

        public DungeonArea GetChildArea(string name)
        {
            return ChildAreas.Find(a => a.Name == name);
        }

        public void AddEnvCell(EnvCell envCell)
        {
            EnvCells.Add(envCell);
        }

        public void RemoveEnvCell(EnvCell envCell)
        {
            EnvCells.Remove(envCell);
            foreach (var childArea in ChildAreas)
            {
                childArea.RemoveEnvCell(envCell);
            }
        }

        public IEnumerable<DungeonArea> GetAllAreas()
        {
            yield return this;
            foreach (var childArea in ChildAreas)
            {
                foreach (var area in childArea.GetAllAreas())
                {
                    yield return area;
                }
            }
        }

        public string GetPath()
        {
            // This method assumes that each area has a unique name within its parent
            // You may need to implement a more robust method if this assumption doesn't hold
            return Name;
        }
    }
}