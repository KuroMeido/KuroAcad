namespace KuroAcad
{
    internal sealed class RibbonButtonDefinition
    {
        public string Id { get; }
        public string Text { get; }
        public string CommandName { get; }
        public string IconPath { get; }

        public RibbonButtonDefinition(string id, string text, string commandName, string iconPath)
        {
            Id = id;
            Text = text;
            CommandName = commandName;
            IconPath = iconPath;
        }
    }
}