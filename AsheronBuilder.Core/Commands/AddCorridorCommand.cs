// AsheronBuilder.Core/Commands/AddCorridorCommand.cs

using AsheronBuilder.Core.Dungeon;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core.Commands
{
    public class AddCorridorCommand : ICommand
    {
        private readonly DungeonLayout _dungeonLayout;
        private readonly List<Vector3> _corridorPoints;
        private readonly DungeonArea _parentArea;
        private EnvCell _corridorCell;

        public AddCorridorCommand(DungeonLayout dungeonLayout, List<Vector3> corridorPoints, DungeonArea parentArea)
        {
            _dungeonLayout = dungeonLayout;
            _corridorPoints = corridorPoints;
            _parentArea = parentArea;
        }

        public void Execute()
        {
            _corridorCell = new EnvCell(0) // Use appropriate EnvironmentId for corridors
            {
                Position = _corridorPoints[0],
                // Set other properties as needed
            };
            _parentArea.AddEnvCell(_corridorCell);
        }

        public void Undo()
        {
            _parentArea.RemoveEnvCell(_corridorCell);
        }
    }
}