using Window = System.Windows.Window;

namespace KuroAcad.UI
{
    public partial class PaletteTabView : Window
    {
        public PaletteTabView()
        {
            InitializeComponent();
            DataContext = new PaletteTabViewModel();
        }
    }
}
