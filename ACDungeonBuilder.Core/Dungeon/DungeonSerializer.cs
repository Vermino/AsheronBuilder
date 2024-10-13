// ACDungeonBuilder.Core/Dungeon/DungeonSerializer.cs

using System.IO;
using System.Text.Json;

namespace ACDungeonBuilder.Core.Dungeon
{
    public static class DungeonSerializer
    {
        public static void SaveDungeon(DungeonLayout dungeon, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(dungeon, options);
            File.WriteAllText(filePath, jsonString);
        }

        public static DungeonLayout LoadDungeon(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<DungeonLayout>(jsonString);
        }
    }
}