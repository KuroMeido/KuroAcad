namespace KuroAcad
{
    internal interface IAcadCommandExecutor
    {
        void Execute(string command);
        void WriteInfo(string message);
    }
}