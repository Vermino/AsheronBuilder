using System;
using System.Collections.Generic;
using System.Linq;

namespace ACDungeonBuilder.Core.Dungeon
{
    public class DungeonValidator
    {
        public static List<string> ValidateDungeon(DungeonLayout dungeonLayout)
        {
            var errors = new List<string>();

            // Check for overlapping rooms
            for (int i = 0; i < dungeonLayout.Rooms.Count; i++)
            {
                for (int j = i + 1; j < dungeonLayout.Rooms.Count; j++)
                {
                    if (RoomsOverlap(dungeonLayout.Rooms[i], dungeonLayout.Rooms[j]))
                    {
                        errors.Add($"Room {dungeonLayout.Rooms[i].Id} overlaps with Room {dungeonLayout.Rooms[j].Id}");
                    }
                }
            }

            // Check for disconnected rooms
            var connectedRooms = new HashSet<int>();
            if (dungeonLayout.Rooms.Count > 0)
            {
                DFS(dungeonLayout, dungeonLayout.Rooms[0].Id, connectedRooms);
            }

            if (connectedRooms.Count != dungeonLayout.Rooms.Count)
            {
                errors.Add("Not all rooms are connected");
            }

            return errors;
        }

        private static bool RoomsOverlap(Room r1, Room r2)
        {
            return r1.X < r2.X + r2.Width &&
                   r1.X + r1.Width > r2.X &&
                   r1.Y < r2.Y + r2.Height &&
                   r1.Y + r1.Height > r2.Y;
        }

        private static void DFS(DungeonLayout dungeonLayout, int roomId, HashSet<int> visited)
        {
            visited.Add(roomId);
            var connectedRooms = dungeonLayout.Corridors
                .Where(c => c.StartRoomId == roomId || c.EndRoomId == roomId)
                .Select(c => c.StartRoomId == roomId ? c.EndRoomId : c.StartRoomId);

            foreach (var connectedRoom in connectedRooms)
            {
                if (!visited.Contains(connectedRoom))
                {
                    DFS(dungeonLayout, connectedRoom, visited);
                }
            }
        }
    }
}