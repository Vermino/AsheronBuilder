// AsheronBuilder.Core/Dungeon/DungeonSerializer.cs

using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core.Dungeon
{
    public static class DungeonSerializer
    {
        public static void SaveDungeon(DungeonLayout dungeonLayout, string filePath)
        {
            try
            {
                var dungeonData = new DungeonData
                {
                    Hierarchy = SerializeHierarchy(dungeonLayout.Hierarchy)
                };

                string jsonString = JsonSerializer.Serialize(dungeonData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving dungeon: {ex.Message}", ex);
            }
        }

        public static DungeonLayout LoadDungeon(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var dungeonData = JsonSerializer.Deserialize<DungeonData>(jsonString);

                if (dungeonData == null)
                {
                    throw new Exception("Failed to deserialize dungeon data");
                }

                var dungeonLayout = new DungeonLayout();
                dungeonLayout.SetHierarchy(DeserializeHierarchy(dungeonData.Hierarchy));
                return dungeonLayout;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading dungeon: {ex.Message}", ex);
            }
        }

        private static DungeonArea DeserializeArea(AreaData areaData)
        {
            var area = new DungeonArea(areaData.Name);
            area.SetPosition(areaData.Position);
            area.SetRotation(areaData.Rotation);
            area.SetScale(areaData.Scale);

            foreach (var childAreaData in areaData.ChildAreas)
            {
                area.AddChildArea(DeserializeArea(childAreaData));
            }

            foreach (var envCellData in areaData.EnvCells)
            {
                area.AddEnvCell(new EnvCell(envCellData.EnvironmentId)
                {
                    Id = envCellData.Id,
                    Position = envCellData.Position,
                    Rotation = envCellData.Rotation,
                    Scale = envCellData.Scale
                });
            }

            return area;
        }

        private static HierarchyData SerializeHierarchy(DungeonHierarchy hierarchy)
        {
            return new HierarchyData
            {
                RootArea = SerializeArea(hierarchy.RootArea)
            };
        }
        
        private static DungeonHierarchy DeserializeHierarchy(HierarchyData hierarchyData)
        {
            return new DungeonHierarchy
            {
                RootArea = DeserializeArea(hierarchyData.RootArea)
            };
        }

        private static AreaData SerializeArea(DungeonArea area)
        {
            var areaData = new AreaData
            {
                Name = area.Name,
                Position = area.Position,
                Rotation = area.Rotation,
                Scale = area.Scale,
                ChildAreas = new List<AreaData>(),
                EnvCells = new List<EnvCellData>()
            };

            foreach (var childArea in area.ChildAreas)
            {
                areaData.ChildAreas.Add(SerializeArea(childArea));
            }

            foreach (var envCell in area.EnvCells)
            {
                areaData.EnvCells.Add(new EnvCellData
                {
                    Id = envCell.Id,
                    EnvironmentId = envCell.EnvironmentId,
                    Position = envCell.Position,
                    Rotation = envCell.Rotation,
                    Scale = envCell.Scale
                });
            }

            return areaData;
        }
    }

    [Serializable]
    public class DungeonData
    {
        public HierarchyData Hierarchy { get; set; }
    }

    [Serializable]
    public class HierarchyData
    {
        public AreaData RootArea { get; set; }
    }

    [Serializable]
    public class AreaData
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public List<AreaData> ChildAreas { get; set; }
        public List<EnvCellData> EnvCells { get; set; }
    }

    [Serializable]
    public class EnvCellData
    {
        public uint Id { get; set; }
        public uint EnvironmentId { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }
}