using System.ComponentModel;
using Autodesk.AutoCAD.Windows.Data;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace KuroAcad.UI
{
    class PaletteTabViewModel : ObservableObject
    {
        // private fields
        ICustomTypeDescriptor layer;
        double radius;
        string txtRad;
        bool validRad;

        /// <summary>
        /// Gets the Command object bound to the OK button.
        /// The button is automatically grayed out if the CanExecute predicate returns false.
        /// </summary>
        public RelayCommand DrawCircleCommand =>
            new RelayCommand((_) => DrawCircle(), (_) => validRad);

        /// <summary>
        /// Gets the Command object bound to the Radius button (>).
        /// </summary>
        public RelayCommand GetRadiusCommand =>
            new RelayCommand((_) => GetRadius(), (_) => true);

        /// <summary>
        /// Gets or sets the selected layer.
        /// </summary>
        public ICustomTypeDescriptor Layer
        {
            get { return layer; }
            set { layer = value; OnPropertyChanged(nameof(Layer)); }
        }

        /// <summary>
        /// Gets the layers collection.
        /// </summary>
        public DataItemCollection Layers => AcAp.UIBindings.Collections.Layers;

        /// <summary>
        /// Gets or sets the value of the radius appearing in the text box.
        /// </summary>
        public string TextRadius
        {
            get { return txtRad; }
            set
            {
                txtRad = value;
                validRad = double.TryParse(value, out radius) && radius > 0.0;
                OnPropertyChanged(nameof(TextRadius));
            }
        }

        /// <summary>
        /// Creates a new instance of PaletteTabViewModel.
        /// </summary>
        public PaletteTabViewModel()
        {
            TextRadius = "10";
            Layer = Layers.CurrentItem;
            Layers.CollectionChanged += (s, e) => Layer = Layers.CurrentItem;
        }

        /// <summary>
        /// Method called by DrawCircleCommand.
        /// Calls the KuroCircleWPF command with the current options
        /// </summary>
        private async void DrawCircle()
        {
            var docs = AcAp.DocumentManager;
            var ed = docs.MdiActiveDocument.Editor;
            await docs.ExecuteInCommandContextAsync(
                async (ojb) =>
                {
                    await ed.CommandAsync("KuroCircleWPF", ((INamedValue)Layer).Name, radius);
                },
                null);
        }
        // With versions prior to AutoCAD 2016, use Document.SendStringToExecute.
        //private void DrawCircle() =>
        // AcAp.DocumentManager.MdiActiveDocument?.SendStringToExecute(
        // $"KuroCircleWPF \"{((INamedValue)Layer).Name}\" {TextRadius} ", false, false, false);

        /// <summary>
        /// Method called by GetRadiusCommand.
        /// </summary>
        private void GetRadius()
        {
            // prompt the user to specify a distance
            var ed = AcAp.DocumentManager.MdiActiveDocument.Editor;
            var opts = new PromptDistanceOptions("\nSpecify the radius: ");
            opts.AllowNegative = false;
            opts.AllowZero = false;
            var pdr = ed.GetDistance(opts);
            if (pdr.Status == PromptStatus.OK)
                TextRadius = pdr.Value.ToString();
        }
    
    }
}
