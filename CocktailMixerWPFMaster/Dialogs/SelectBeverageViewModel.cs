using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using CocktailMixerCommunicator.Model;

namespace CocktailMixerWPFMaster.Dialogs
{
    public class SelectBeverageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public SelectBeverageViewModel(IEnumerable<Beverage> selectFrom)
        {
            this.SourceList = new List<Beverage>(selectFrom);
        }

        private List<Beverage> _sourceList;

        public List<Beverage> SourceList
        {
            get { return _sourceList; }
            set
            {
                _sourceList = value;
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
                IsSelectionValid = _selectedBeverage != null && _amount != 0;
            }
        }

        private bool _isSelecteionValid;

        public bool IsSelectionValid
        {
            get { return _isSelecteionValid; }
            set
            {
                _isSelecteionValid = value;
                NotifyPropertyChanged();
            }
        }

        private int _amount;

        public int Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                NotifyPropertyChanged();
                IsSelectionValid = _amount != 0 && _selectedBeverage != null; 
            }
        }


    }
}
