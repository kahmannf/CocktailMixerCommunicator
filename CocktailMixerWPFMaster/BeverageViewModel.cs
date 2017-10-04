using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerWPFMaster
{
    public class BeverageViewModel : INotifyPropertyChanged
    {
        private MainViewModel _mainVM;

        public BeverageViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<Beverage> _listBeverages;

        public ObservableCollection<Beverage> ListBeverages
        {
            get { return _listBeverages; }
            set
            {
                _listBeverages = value;
                NotifyPropertyChanged();
            }
        }

        public void LoadFromCMState(CMGlobalState state)
        {
            ListBeverages = new ObservableCollection<Beverage>(state.BeverageDataBase);
        }
    }
}
