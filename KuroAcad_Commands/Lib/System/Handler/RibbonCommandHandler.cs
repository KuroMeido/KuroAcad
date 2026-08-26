using System.Windows.Input;
using Autodesk.Windows;

namespace KuroAcad
{
    internal sealed class RibbonCommandHandler : ICommand
    {
        private readonly IAcadCommandExecutor commandExecutor;

        public RibbonCommandHandler(IAcadCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var command = ResolveCommand(parameter);

            if (string.IsNullOrWhiteSpace(command))
            {
                commandExecutor.WriteInfo("Ribbon click received but no command found.");
                return;
            }

            var cleanedCommand = command
                .Replace("^C", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (string.IsNullOrWhiteSpace(cleanedCommand))
            {
                commandExecutor.WriteInfo("Ribbon click received but command was empty after cleanup.");
                return;
            }

            commandExecutor.WriteInfo($"Ribbon clicked. Running: {cleanedCommand}");
            commandExecutor.Execute(cleanedCommand);
        }

        private static string? ResolveCommand(object? parameter)
        {
            if (parameter is RibbonButton button)
            {
                return button.CommandParameter as string;
            }

            return parameter as string;
        }
    }
}