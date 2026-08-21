using System;
using System.Windows;
using KuroAcad.ModelItems;

namespace KuroAcad.UI
{
    public partial class TemDatView : Window
    {
        private readonly TemDatVM viewModel;

        public TemDatView()
        {
            InitializeComponent();
            viewModel = new TemDatVM();
            viewModel.RequestClose += OnRequestClose;
            DataContext = viewModel;
        }

        public TemDatDialogResult DialogData => viewModel.Result;

        private void OnRequestClose(bool result)
        {
            DialogResult = result;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.RequestClose -= OnRequestClose;
            base.OnClosed(e);
        }
    }
}
