using CocktailMixerCLI;
using CocktailMixerCommunicator.Model;
using CocktailMixerWPFMaster.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CocktailMixerWPFMaster
{
    public class RecipeViewModel : INotifyPropertyChanged
    {
        private MainViewModel _mainVM;

        public RecipeViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<Recipe> _listRecipes;

        public ObservableCollection<Recipe> ListRecipes
        {
            get { return _listRecipes; }
            set
            {
                _listRecipes = value;
                NotifyPropertyChanged();
            }
        }

        private Recipe _selecteRecipe;

        public Recipe SelectedRecipe
        {
            get { return _selecteRecipe; }
            set
            {
                _selecteRecipe = value;
                NotifyPropertyChanged();
                SelectedName = _selecteRecipe.Name;
                SelectedIngredients = new ObservableCollection<Beverage>(_selecteRecipe.Ingredients);
            }
        }

        public void AddRecipe()
        {
            IEnumerable<Beverage> allBeverages = _mainVM.VMBeverage.ListBeverages;

            IEnumerable<Beverage> recipeBeverage = SelectedRecipe.Ingredients;

            IEnumerable<Beverage> inclusiveBeverages = from singleAllBev in allBeverages
                                                       join singleRecBev in recipeBeverage
                                                       on singleAllBev.GUID equals singleRecBev.GUID
                                                       select singleAllBev;

            IEnumerable<Beverage> exclusiveBeverages = allBeverages.Except(inclusiveBeverages);

            SelectBeverageDialog dialog = new SelectBeverageDialog(exclusiveBeverages);

            bool? dialogresult = dialog.ShowDialog();

            if (dialogresult.HasValue == dialogresult.Value)
            {
                Beverage newBeverage = (dialog.DataContext as SelectBeverageViewModel).SelectedBeverage;

                SelectedRecipe.Ingredients.Add(newBeverage);

                SelectedIngredients.Add(newBeverage);
            }
        }

        public void RemoveSelectedBeverage()
        {
            if (SelectedIngredient != null)
            {
                SelectedIngredients.Remove(SelectedIngredient);
            }
        }

        public void CancelEdit()
        {
            if (IsEditingEnabled)
            {
                SelectedRecipe = SelectedRecipe;
                IsEditingEnabled = false;
            }
        }

        public void SaveRecipe()
        {
            throw new NotImplementedException();
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

        private ObservableCollection<Beverage> _selectedIngredients;

        public ObservableCollection<Beverage> SelectedIngredients
        {
            get { return _selectedIngredients; }
            set
            {
                _selectedIngredients = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isEditingEnabled;

        public bool IsEditingEnabled
        {
            get { return _isEditingEnabled; }
            set
            {
                _isEditingEnabled = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsSelectionEnabled");
            }
        }

        public bool IsSelectionEnabled
        {
            get => !_isEditingEnabled;
            set
            {
                IsEditingEnabled = !value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsEditingEnabled");
            }
        }

        private Beverage _selectedIngredient;

        public Beverage SelectedIngredient
        {
            get { return _selectedIngredient; }
            set
            {
                _selectedIngredient = value;
                IsBeverageSelected = _selectedIngredients != null;
                NotifyPropertyChanged();
            }
        }


        private bool _isBeverageSelected;

        public bool IsBeverageSelected
        {
            get { return _isBeverageSelected; }
            set
            {
                _isBeverageSelected = value;
                NotifyPropertyChanged();
            }
        }


        public void LoadFromCMState(CMGlobalState state)
        {
            ListRecipes = new ObservableCollection<Recipe>(state.Recipes);
            this.IsEditingEnabled = false;
        }
    }
}
