using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace KuroAcad.UI
{
    class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Evénement déclenché lorsqu'une propriété change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Méthode appelée dans le 'setter' des propriétés dont on veut notifier le changement.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
