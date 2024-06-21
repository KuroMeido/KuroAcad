using UserControl = System.Windows.Controls.UserControl;

namespace KuroAcad.UI
{
    public partial class PaletteTabView : UserControl
    {
        public PaletteTabView()
        {
            InitializeComponent();
            DataContext = new PaletteTabViewModel();
        }
    }
}
