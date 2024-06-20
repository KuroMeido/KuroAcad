using System.Reflection;
using System.Windows;

namespace KuroAcad.UI
{
    public partial class SqlWPF : Window
    {
        public SqlWPF()
        {
            System.Windows.Application.ResourceAssembly = Assembly.GetExecutingAssembly();
            InitializeComponent();
        }
        private void buttonLoadLine_click(object sender, EventArgs e)
        {
            this.DialogResult = true;
        }
        private void button_close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
