// File: AsheronBuilder.Core/Dungeon/DungeonLayout.cs

using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

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

        public void AddEnvCell(EnvCell envCell, string areaPath = "Root")
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

        public IEnumerable<EnvCell> GetAllEnvCells()
        {
            return _envCells.Values;
        }

        public void UpdateEnvCell(EnvCell envCell)
        {
            if (_envCells.ContainsKey(envCell.Id))
            {
                _envCells[envCell.Id] = envCell;
            }
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

        private DungeonArea GetOrCreateArea(string path)
        {
            var pathParts = path.Split('/');
            var currentArea = RootArea;

            foreach (var part in pathParts.Skip(1))
            {
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

        public DungeonArea GetChildArea(string name)
        {
            return ChildAreas.FirstOrDefault(a => a.Name == name);
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
}
