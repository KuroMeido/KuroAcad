using System.Windows.Input;

namespace KuroAcad
{
    internal interface ICommandHandlerFactory
    {
        ICommand Create();
    }
}