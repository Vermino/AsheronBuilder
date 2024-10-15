// AsheronBuilder.Core/Commands/AddRoomCommand.cs

using AsheronBuilder.Core.Dungeon;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core.Commands
{
    public class AddRoomCommand : ICommand
    {
        private readonly DungeonLayout _dungeonLayout;
        private readonly DungeonArea _newRoom;
        private readonly DungeonArea _parentArea;

        public AddRoomCommand(DungeonLayout dungeonLayout, DungeonArea newRoom, DungeonArea parentArea)
        {
            _dungeonLayout = dungeonLayout;
            _newRoom = newRoom;
            _parentArea = parentArea;
        }

        public void Execute()
        {
            _parentArea.AddChildArea(_newRoom);
        }

        public void Undo()
        {
            _parentArea.ChildAreas.Remove(_newRoom);
        }
    }
}