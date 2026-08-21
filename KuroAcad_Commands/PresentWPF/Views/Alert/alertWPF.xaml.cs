using System.Windows;

namespace KuroAcad.UI
{
    /// <summary>
    /// Interaction logic for alertWPF.xaml
    /// </summary>
    public partial class alertWPF : Window
    {
        public alertWPF()
        {
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
    }
}
