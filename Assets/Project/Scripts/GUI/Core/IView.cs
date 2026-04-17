namespace Project.Scripts.GUI.Core
{
    public interface IView
    {
        void Open();
        void OpenImmediately();
        void Close();
        void CloseImmediately();
    }
}