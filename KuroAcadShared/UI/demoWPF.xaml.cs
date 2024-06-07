using System.Windows;
using System.Windows.Controls;

namespace KuroAcad.UI
{
    /// <summary>
    /// Interaction logic for demoWPF.xaml
    /// </summary>
    public partial class demoWPF : Page
    {
        public demoWPF()
        {
            InitializeComponent();
        }

        // public read/write property
        public string HelloText
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }
    }
}
