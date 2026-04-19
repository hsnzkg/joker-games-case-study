using System.Collections.Generic;

namespace Project.Scripts.Command
{
    public static class CommandManager
    {
        private static readonly Stack<ICommand> s_history = new();

        public static bool CanUndo => s_history.Count > 0;
        public static int Count => s_history.Count;

        public static bool Execute(ICommand command)
        {
            if (command == null)
            {
                return false;
            }

            if (!command.Execute())
            {
                return false;
            }

            s_history.Push(command);
            return true;
        }

        public static bool Undo()
        {
            if (!CanUndo)
            {
                return false;
            }

            ICommand command = s_history.Pop();
            command.Undo();
            return true;
        }

        public static void Clear()
        {
            while (CanUndo)
            {
                Undo();
            }
        }

        public static void ForceClear()
        {
            s_history.Clear();
        }
    }
}
