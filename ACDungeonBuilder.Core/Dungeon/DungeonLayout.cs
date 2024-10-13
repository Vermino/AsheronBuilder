// ACDungeonBuilder.Core/Dungeon/DungeonLayout.cs

using System.Collections.Generic;

namespace ACDungeonBuilder.Core.Dungeon
{
    public class DungeonLayout
    {
        public List<Room> Rooms { get; } = new List<Room>();
        public List<Corridor> Corridors { get; } = new List<Corridor>();
        public List<Door> Doors { get; } = new List<Door>();

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        public void AddCorridor(Corridor corridor)
        {
            Corridors.Add(corridor);
        }

        public void AddDoor(Door door)
        {
            Doors.Add(door);
        }

        public void RemoveRoom(Room room)
        {
            Rooms.Remove(room);
        }

        public void RemoveCorridor(Corridor corridor)
        {
            Corridors.Remove(corridor);
        }

        public void RemoveDoor(Door door)
        {
            Doors.Remove(door);
        }
    }

    public class Room
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public uint EnvironmentId { get; set; }
    }

    public class Corridor
    {
        public int Id { get; set; }
        public int StartRoomId { get; set; }
        public int EndRoomId { get; set; }
        public List<Point> Path { get; set; } = new List<Point>();
    }

    public class Door
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DoorType Type { get; set; }
    }

    public enum DoorType
    {
        Normal,
        Secret,
        PortalDoor
    }

    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}