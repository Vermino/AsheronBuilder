// File: AsheronBuilder.Core/DungeonSerializer.cs

using System.IO;
using AsheronBuilder.Core.Dungeon;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core
{
    public static class DungeonSerializer
    {
        public static void SaveDungeon(DungeonLayout dungeonLayout, string filePath)
        {
            using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // Write version
                writer.Write((byte)1);

                // Write EnvCells
                var envCells = dungeonLayout.GetAllEnvCells().ToList();
                writer.Write(envCells.Count);
                foreach (var envCell in envCells)
                {
                    writer.Write(envCell.Id);
                    writer.Write(envCell.EnvironmentId);
                    WriteVector3(writer, envCell.Position);
                    WriteQuaternion(writer, envCell.Rotation);
                    WriteVector3(writer, envCell.Scale);
                }

                // Write Hierarchy
                WriteHierarchy(writer, dungeonLayout.Hierarchy.RootArea);
            }
        }

        public static DungeonLayout LoadDungeon(string filePath)
        {
            var dungeonLayout = new DungeonLayout();

            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                // Read version
                byte version = reader.ReadByte();
                if (version != 1) throw new Exception("Unsupported dungeon file version.");

                // Read EnvCells
                int envCellCount = reader.ReadInt32();
                for (int i = 0; i < envCellCount; i++)
                {
                    uint id = reader.ReadUInt32();
                    uint environmentId = reader.ReadUInt32();
                    Vector3 position = ReadVector3(reader);
                    Quaternion rotation = ReadQuaternion(reader);
                    Vector3 scale = ReadVector3(reader);

                    var envCell = new EnvCell(environmentId)
                    {
                        Id = id,
                        Position = position,
                        Rotation = rotation,
                        Scale = scale
                    };
                    dungeonLayout.AddEnvCell(envCell);
                }

                // Read Hierarchy
                ReadHierarchy(reader, dungeonLayout.Hierarchy.RootArea, dungeonLayout);
            }

            return dungeonLayout;
        }

        private static void WriteVector3(BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }

        private static void WriteQuaternion(BinaryWriter writer, Quaternion quaternion)
        {
            writer.Write(quaternion.X);
            writer.Write(quaternion.Y);
            writer.Write(quaternion.Z);
            writer.Write(quaternion.W);
        }

        private static Quaternion ReadQuaternion(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return new Quaternion(x, y, z, w);
        }

        private static void WriteHierarchy(BinaryWriter writer, DungeonArea area)
        {
            writer.Write(area.Name);
            writer.Write(area.EnvCells.Count);
            foreach (var envCell in area.EnvCells)
            {
                writer.Write(envCell.Id);
            }
            writer.Write(area.ChildAreas.Count);
            foreach (var childArea in area.ChildAreas)
            {
                WriteHierarchy(writer, childArea);
            }
        }

        private static void ReadHierarchy(BinaryReader reader, DungeonArea area, DungeonLayout dungeonLayout)
        {
            area.Name = reader.ReadString();
            int envCellCount = reader.ReadInt32();
            for (int i = 0; i < envCellCount; i++)
            {
                uint envCellId = reader.ReadUInt32();
                var envCell = dungeonLayout.GetEnvCellById(envCellId);
                if (envCell != null)
                {
                    area.AddEnvCell(envCell);
                }
            }
            int childAreaCount = reader.ReadInt32();
            for (int i = 0; i < childAreaCount; i++)
            {
                var childArea = new DungeonArea("");
                ReadHierarchy(reader, childArea, dungeonLayout);
                area.AddChildArea(childArea);
            }
        }
    }
}