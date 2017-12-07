using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerWPFMaster
{
    public class ServingModeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChange(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private MainViewModel _vmMain;

        public MainViewModel VMMain => _vmMain;

        public ServingModeViewModel(MainViewModel vmmain)
        {
            _vmMain = vmmain;
        }


    }
}
