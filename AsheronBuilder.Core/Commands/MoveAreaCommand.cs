// AsheronBuilder.Core/Commands/MoveAreaCommand.cs

using AsheronBuilder.Core.Dungeon;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core.Commands
{
    public class MoveAreaCommand : ICommand
    {
        private readonly DungeonArea _area;
        private readonly Vector3 _oldPosition;
        private readonly Vector3 _newPosition;

        public MoveAreaCommand(DungeonArea area, Vector3 newPosition)
        {
            _area = area;
            _oldPosition = area.Position;
            _newPosition = newPosition;
        }

        public void Execute()
        {
            _area.SetPosition(_newPosition);
        }

        public void Undo()
        {
            _area.SetPosition(_oldPosition);
        }
    }
}