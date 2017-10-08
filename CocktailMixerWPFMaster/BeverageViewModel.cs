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
            IsEditEnabled = false;
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

        private Beverage _selectedBeverage;

        public Beverage SelectedBeverage
        {
            get { return _selectedBeverage; }
            set
            {
                _selectedBeverage = value;
                NotifyPropertyChanged();

                if (_selectedBeverage != null)
                {
                    SelectedName = _selectedBeverage.Name;
                    SelectedAlcVol = _selectedBeverage.AlcoholVolPercentage;
                    SelectedAmountTimeRatio = _selectedBeverage.AmountTimeCoefficient;

                }
                else
                {
                    SelectedName = string.Empty;
                }
            }
        }

        private string _selectedName;

        public string SelectedName
        {
            get { return _selectedName; }
            set
            {
                _selectedName = value;
                NotifyPropertyChanged();
            }
        }

        private double _selectedAlcVol;

        public double SelectedAlcVol
        {
            get { return _selectedAlcVol; }
            set
            {
                _selectedAlcVol = value;
                NotifyPropertyChanged();
            }
        }

        private double _selectedAmountTimeRatio;

        public double SelectedAmountTimeRatio
        {
            get { return _selectedAmountTimeRatio; }
            set
            {
                _selectedAmountTimeRatio = value;
                NotifyPropertyChanged();
            }
        }



        private bool _isEditEnabled;

        public bool IsEditEnabled
        {
            get { return _isEditEnabled; }
            set
            {
                _isEditEnabled = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsSelectionEnabled");
            }
        }

        public bool IsSelectionEnabled
        {
            get => !_isEditEnabled;
            set
            {
                IsEditEnabled = !value;
                NotifyPropertyChanged();
            }
        }


        public void LoadFromCMState(CMGlobalState state)
        {
            ListBeverages = new ObservableCollection<Beverage>(state.BeverageDataBase);
        }
    }
}
