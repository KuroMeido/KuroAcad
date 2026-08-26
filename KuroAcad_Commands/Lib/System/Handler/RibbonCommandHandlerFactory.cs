using System.Windows.Input;

namespace KuroAcad
{
    internal sealed class RibbonCommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly IAcadCommandExecutor commandExecutor;

        public RibbonCommandHandlerFactory(IAcadCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public ICommand Create()
        {
            return new RibbonCommandHandler(commandExecutor);
        }
    }
}