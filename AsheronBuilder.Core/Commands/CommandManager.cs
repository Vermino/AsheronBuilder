// File: AsheronBuilder.Core/Commands/CommandManager.cs

using System.Collections.Generic;
using AsheronBuilder.Core.Dungeon;
using OpenTK.Mathematics;

namespace AsheronBuilder.Core.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }
    }

    public class AddEnvCellCommand : ICommand
    {
        private readonly DungeonLayout _dungeonLayout;
        private readonly EnvCell _envCell;
        private readonly string _areaPath;

        public AddEnvCellCommand(DungeonLayout dungeonLayout, EnvCell envCell, string areaPath)
        {
            _dungeonLayout = dungeonLayout;
            _envCell = envCell;
            _areaPath = areaPath;
        }

        public void Execute()
        {
            _dungeonLayout.AddEnvCell(_envCell, _areaPath);
        }

        public void Undo()
        {
            _dungeonLayout.RemoveEnvCell(_envCell.Id);
        }
    }

    public class RemoveEnvCellCommand : ICommand
    {
        private readonly DungeonLayout _dungeonLayout;
        private readonly EnvCell _envCell;
        private readonly string _areaPath;

        public RemoveEnvCellCommand(DungeonLayout dungeonLayout, EnvCell envCell, string areaPath)
        {
            _dungeonLayout = dungeonLayout;
            _envCell = envCell;
            _areaPath = areaPath;
        }

        public void Execute()
        {
            _dungeonLayout.RemoveEnvCell(_envCell.Id);
        }

        public void Undo()
        {
            _dungeonLayout.AddEnvCell(_envCell, _areaPath);
        }
    }

    public class MoveEnvCellCommand : ICommand
    {
        private readonly DungeonLayout _dungeonLayout;
        private readonly EnvCell _envCell;
        private readonly Vector3 _oldPosition;
        private readonly Vector3 _newPosition;

        public MoveEnvCellCommand(DungeonLayout dungeonLayout, EnvCell envCell, Vector3 newPosition)
        {
            _dungeonLayout = dungeonLayout;
            _envCell = envCell;
            _oldPosition = envCell.Position;
            _newPosition = newPosition;
        }

        public void Execute()
        {
            _envCell.Position = _newPosition;
            _dungeonLayout.UpdateEnvCell(_envCell);
        }

        public void Undo()
        {
            _envCell.Position = _oldPosition;
            _dungeonLayout.UpdateEnvCell(_envCell);
        }
    }
}