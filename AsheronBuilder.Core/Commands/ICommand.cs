// AsheronBuilder.Core/Commands/ICommand.cs

namespace AsheronBuilder.Core.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}