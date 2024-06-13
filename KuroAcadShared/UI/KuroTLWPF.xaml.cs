using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;
using RadioButton = System.Windows.Controls.RadioButton;

namespace KuroAcad.UI
{
    public partial class KuroTLWPF: Window
    {
        public KuroTLWPF()
        {
            Application.ResourceAssembly = Assembly.GetExecutingAssembly();
            InitializeComponent();
        }
        private void button_close(object sender, RoutedEventArgs e)
        {
            Close();

        }

        private void buttonOk_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void radioButtonOption(object sender, RoutedEventArgs e)
        {

        }
    }
}
