using System;
using KuroAcad.ModelItems;
using System.Windows.Input;
using PropertyChanged;

namespace KuroAcad.UI
{
    [AddINotifyPropertyChangedInterface]
    class TemDatVM : ObservableObject
    {
        private int selectedOption;
        private string tagName;
        private string tagArea;
        private string tagDensity;
        private string tagFloors;
        private string tagFar;
        private string prefix;
        private string startNumber;
        private string densityValue;
        private string floorsValue;
        private string farValue;
        private string blockName;

        public event Action<bool> RequestClose;

        public TemDatVM()
        {
            selectedOption = 2;
            tagName = "TL";
            tagArea = "DT";
            tagDensity = "MD";
            tagFloors = "TC";
            tagFar = "HS";

            prefix = "C";
            startNumber = "1";
            densityValue = "80";
            floorsValue = "5";
            farValue = "4";
            blockName = "TEMDAT";
        }

        public TemDatDialogResult Result { get; private set; }

        public RelayCommand OkCommand =>
            new RelayCommand(_ => Confirm(), _ => CanConfirm());

        public RelayCommand CancelCommand =>
            new RelayCommand(_ => Cancel(), _ => true);

        public bool IsOption2
        {
            get { return SelectedOption == 2; }
            set
            {
                if (value)
                {
                    SelectedOption = 2;
                }
            }
        }

        public bool IsOption4
        {
            get { return SelectedOption == 4; }
            set
            {
                if (value)
                {
                    SelectedOption = 4;
                }
            }
        }

        public bool IsOption5
        {
            get { return SelectedOption == 5; }
            set
            {
                if (value)
                {
                    SelectedOption = 5;
                }
            }
        }

        public int SelectedOption
        {
            get { return selectedOption; }
            set
            {
                if (selectedOption == value)
                {
                    return;
                }

                selectedOption = value;
                OnPropertyChanged(nameof(SelectedOption));
                OnPropertyChanged(nameof(IsOption2));
                OnPropertyChanged(nameof(IsOption4));
                OnPropertyChanged(nameof(IsOption5));
                OnPropertyChanged(nameof(IsDensityEnabled));
                OnPropertyChanged(nameof(IsFarEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsDensityEnabled => SelectedOption >= 4;
        public bool IsFarEnabled => SelectedOption == 5;

        public string TagName
        {
            get { return tagName; }
            set
            {
                tagName = value;
                OnPropertyChanged(nameof(TagName));
            }
        }

        public string TagArea
        {
            get { return tagArea; }
            set
            {
                tagArea = value;
                OnPropertyChanged(nameof(TagArea));
            }
        }

        public string TagDensity
        {
            get { return tagDensity; }
            set
            {
                tagDensity = value;
                OnPropertyChanged(nameof(TagDensity));
            }
        }

        public string TagFloors
        {
            get { return tagFloors; }
            set
            {
                tagFloors = value;
                OnPropertyChanged(nameof(TagFloors));
            }
        }

        public string TagFAR
        {
            get { return tagFar; }
            set
            {
                tagFar = value;
                OnPropertyChanged(nameof(TagFAR));
            }
        }

        public string Prefix
        {
            get { return prefix; }
            set
            {
                prefix = value;
                OnPropertyChanged(nameof(Prefix));
            }
        }

        public string StartNumber
        {
            get { return startNumber; }
            set
            {
                startNumber = value;
                OnPropertyChanged(nameof(StartNumber));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string DensityValue
        {
            get { return densityValue; }
            set
            {
                densityValue = value;
                OnPropertyChanged(nameof(DensityValue));
            }
        }

        public string FloorsValue
        {
            get { return floorsValue; }
            set
            {
                floorsValue = value;
                OnPropertyChanged(nameof(FloorsValue));
            }
        }

        public string FARValue
        {
            get { return farValue; }
            set
            {
                farValue = value;
                OnPropertyChanged(nameof(FARValue));
            }
        }

        public string BlockName
        {
            get { return blockName; }
            set
            {
                blockName = value;
                OnPropertyChanged(nameof(BlockName));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool CanConfirm()
        {
            int start;
            return !string.IsNullOrWhiteSpace(BlockName)
                && int.TryParse(StartNumber, out start);
        }

        private void Confirm()
        {
            Result = BuildResult();
            RequestClose?.Invoke(true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }

        private TemDatDialogResult BuildResult()
        {
            int start;
            int.TryParse(StartNumber, out start);

            return new TemDatDialogResult
            {
                TagCount = SelectedOption,
                BlockName = BlockName,
                Prefix = Prefix,
                StartNumber = start,
                TagName = TagName,
                TagArea = TagArea,
                TagDensity = TagDensity,
                TagFloors = TagFloors,
                TagFAR = TagFAR,
                ValueDensity = IsDensityEnabled ? DensityValue : string.Empty,
                ValueFloors = IsDensityEnabled ? FloorsValue : string.Empty,
                ValueFAR = IsFarEnabled ? FARValue : string.Empty
            };
        }
    }
}