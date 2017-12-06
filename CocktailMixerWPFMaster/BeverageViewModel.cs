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

        public void SaveBeverage()
        {
            if (string.IsNullOrEmpty(_selectedName))
            {
                System.Windows.MessageBox.Show("Name cannot be empty");
                return;
            }

            if (_selectedAlcVol < 0.0 || _selectedAlcVol > 100.0)
            {
                System.Windows.MessageBox.Show("Alcohol volume % has to be a value between 0.0 and 100.0");
                return;
            }

            if (SelectedAmountTimeRatio <= 0.0)
            {
                System.Windows.MessageBox.Show("Amount time ratio has to be larger than 0.0");
                return;
            }

            SelectedBeverage.Name = SelectedName;
            SelectedBeverage.AlcoholVolPercentage = SelectedAlcVol;
            SelectedBeverage.AmountTimeCoefficient = SelectedAmountTimeRatio;
            SelectedBeverage.RatioAmount = 0;

            _mainVM.SaveCurrentState();

            ListBeverages = new ObservableCollection<Beverage>(ListBeverages);

            IsEditEnabled = false;
        }

        public void DeleteBeverage()
        {
            if (System.Windows.MessageBox.Show($"Delete beverage \"{SelectedBeverage.Name}\"? This cannot be undone!", "Are You Sure?", System.Windows.MessageBoxButton.YesNo)
                == System.Windows.MessageBoxResult.Yes)
            {
                IEnumerable<Recipe> relatedRecipes = _mainVM.VMRecipe.ListRecipes.Where(x => x.Ingredients.Any(b => b.GUID == SelectedBeverage.GUID));

                int count = relatedRecipes.Count();

                if (count > 0)
                {
                    IEnumerable<Recipe> deleteRecipes = relatedRecipes.Where(x => x.Ingredients.Count == 1);
                    IEnumerable<Recipe> modifyRecipes = relatedRecipes.Except(deleteRecipes);


                    System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show($"By deleting this beverage {modifyRecipes.Count()} recipies will be modified and {deleteRecipes.Count()} recipies will be deleted. Proceed?", "Waring", System.Windows.MessageBoxButton.YesNo);

                    if (result != System.Windows.MessageBoxResult.Yes)
                        return;

                    foreach (Recipe r in modifyRecipes)
                    {
                        Beverage reference = r.Ingredients.First(x => x.GUID == SelectedBeverage.GUID);

                        r.Ingredients.Remove(reference);
                    }

                    foreach (Recipe removeR in (new List<Recipe>(deleteRecipes)))
                    {
                        _mainVM.VMRecipe.ListRecipes.Remove(removeR);
                    }
                }

                ListBeverages.Remove(SelectedBeverage);
                _mainVM.SaveCurrentState();
            }
        }

        public void AddBeverage()
        {
            Beverage newBeverage = new Beverage
            {
                AmountTimeCoefficient = 1,
                GUID = System.Guid.NewGuid().ToString(),
                Name = "New Beverage"
            };

            ListBeverages.Add(newBeverage);

            SelectedBeverage = newBeverage;

            IsEditEnabled = true;
        }

        public void CancelEdit()
        {
            this.IsEditEnabled = false;
            this.SelectedBeverage = this.SelectedBeverage;
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
            IsEditEnabled = false;

            if (SelectedBeverage != null)
            {
                SelectedBeverage = ListBeverages.FirstOrDefault(x => x.GUID == SelectedBeverage.GUID);
            }
        }
    }
}
