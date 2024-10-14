using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Numerics;

namespace AsheronBuilder.Core.Dungeon
{
    public static class DungeonSerializer
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void SaveDungeon(DungeonLayout dungeon, string filePath)
        {
            var dungeonData = new SerializableDungeonData(dungeon);
            string jsonString = JsonSerializer.Serialize(dungeonData, _options);
            File.WriteAllText(filePath, jsonString);
        }

        public static DungeonLayout LoadDungeon(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            var dungeonData = JsonSerializer.Deserialize<SerializableDungeonData>(jsonString, _options);
            return dungeonData.ToDungeonLayout();
        }

        private class SerializableDungeonData
        {
            public List<SerializableEnvCell> EnvCells { get; set; }
            public SerializableDungeonArea RootArea { get; set; }

            public SerializableDungeonData() { }

            public SerializableDungeonData(DungeonLayout dungeonLayout)
            {
                EnvCells = new List<SerializableEnvCell>();
                foreach (var envCell in dungeonLayout.GetAllEnvCells())
                {
                    EnvCells.Add(new SerializableEnvCell(envCell));
                }

                RootArea = new SerializableDungeonArea(dungeonLayout.Hierarchy.RootArea);
            }

            public DungeonLayout ToDungeonLayout()
            {
                var dungeonLayout = new DungeonLayout();

                foreach (var serializableEnvCell in EnvCells)
                {
                    var envCell = serializableEnvCell.ToEnvCell();
                    dungeonLayout.AddEnvCell(envCell, "Root"); // Default to root, we'll update the hierarchy later
                }

                UpdateHierarchy(dungeonLayout, RootArea, "Root");

                return dungeonLayout;
            }

            private void UpdateHierarchy(DungeonLayout dungeonLayout, SerializableDungeonArea serializableArea, string path)
            {
                foreach (var envCellId in serializableArea.EnvCellIds)
                {
                    var envCell = dungeonLayout.GetEnvCellById(envCellId);
                    if (envCell != null)
                    {
                        dungeonLayout.Hierarchy.AddEnvCell(envCell, path);
                    }
                }

                foreach (var childArea in serializableArea.ChildAreas)
                {
                    string childPath = path + "/" + childArea.Name;
                    UpdateHierarchy(dungeonLayout, childArea, childPath);
                }
            }
        }

        private class SerializableEnvCell
        {
            public uint Id { get; set; }
            public uint EnvironmentId { get; set; }
            public float[] Position { get; set; }
            public float[] Rotation { get; set; }
            public float[] Scale { get; set; }

            public SerializableEnvCell() { }

            public SerializableEnvCell(EnvCell envCell)
            {
                Id = envCell.Id;
                EnvironmentId = envCell.EnvironmentId;
                Position = new float[] { envCell.Position.X, envCell.Position.Y, envCell.Position.Z };
                Rotation = new float[] { envCell.Rotation.X, envCell.Rotation.Y, envCell.Rotation.Z, envCell.Rotation.W };
                Scale = new float[] { envCell.Scale.X, envCell.Scale.Y, envCell.Scale.Z };
            }

            public EnvCell ToEnvCell()
            {
                var envCell = new EnvCell(EnvironmentId)
                {
                    Id = Id,
                    Position = new Vector3(Position[0], Position[1], Position[2]),
                    Rotation = new Quaternion(Rotation[0], Rotation[1], Rotation[2], Rotation[3]),
                    Scale = new Vector3(Scale[0], Scale[1], Scale[2])
                };
                return envCell;
            }
        }

        private class SerializableDungeonArea
        {
            public string Name { get; set; }
            public List<SerializableDungeonArea> ChildAreas { get; set; }
            public List<uint> EnvCellIds { get; set; }

            public SerializableDungeonArea() { }

            public SerializableDungeonArea(DungeonArea area)
            {
                Name = area.Name;
                ChildAreas = new List<SerializableDungeonArea>();
                foreach (var childArea in area.ChildAreas)
                {
                    ChildAreas.Add(new SerializableDungeonArea(childArea));
                }
                EnvCellIds = new List<uint>();
                foreach (var envCell in area.EnvCells)
                {
                    EnvCellIds.Add(envCell.Id);
                }
            }
        }
    }
}