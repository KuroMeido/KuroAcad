using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
