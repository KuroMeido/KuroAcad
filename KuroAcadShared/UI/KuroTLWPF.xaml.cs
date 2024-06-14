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
            string ActionSelectionButtonName = (this.groupBox_Option.Content as System.Windows.Controls.Grid)
                                .Children.OfType<RadioButton>()
                                .FirstOrDefault(rb => rb.IsChecked.Value == true)
                                .Name;
            if (ActionSelectionButtonName == "radioButton2" && textBoxDensity!= null)
            {
                this.textBoxDensity.IsEnabled = false;
                this.textBoxFloors.IsEnabled = false;
                this.textBoxFAR.IsEnabled = false;
            }
            else if (ActionSelectionButtonName == "radioButton4" && textBoxDensity != null)
            {
                this.textBoxDensity.IsEnabled = true;
                this.textBoxFloors.IsEnabled = true;
                this.textBoxFAR.IsEnabled = false;
            }
            else if (ActionSelectionButtonName == "radioButton5" && textBoxDensity != null)
            {
                this.textBoxDensity.IsEnabled = true;
                this.textBoxFloors.IsEnabled = true;
                this.textBoxFAR.IsEnabled = true;
            }

        }
    }
}
