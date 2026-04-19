namespace Project.Scripts.Command
{
    public interface ICommand
    {
        bool Execute();
        void Undo();
    }
}
