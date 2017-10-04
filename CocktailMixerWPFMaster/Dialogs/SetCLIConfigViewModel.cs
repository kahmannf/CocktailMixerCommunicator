using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerWPFMaster.Dialogs
{
    public class SetCLIConfigViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private int _baudRate;

        public int BaudRate
        {
            get { return _baudRate; }
            set
            {
                _baudRate = value;
                NotifyPropertyChanged();
            }
        }

        private string _cmStateDirectory;

        public string CMStateDirectory
        {
            get { return _cmStateDirectory; }
            set
            {
                _cmStateDirectory = value;
                NotifyPropertyChanged();
            }
        }

        private string _COMPortName;

        public string COMPortName
        {
            get { return _COMPortName; }
            set
            {
                _COMPortName = value;
                NotifyPropertyChanged();
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }


        public bool Validate()
        {
            bool valid = !string.IsNullOrEmpty(COMPortName) &&
                !string.IsNullOrEmpty(CMStateDirectory) &&
                BaudRate != 0;

            if (!valid)
            {
                ErrorMessage = "Please fill in all values.";
            }
            else
            {
                ErrorMessage = "";
            }

            return valid;
        }
    }
}
