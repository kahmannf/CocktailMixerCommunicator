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

        public void AddRecipe()
        {
            if (!IsSelectionEnabled)
                return;

            Recipe newRecipe = new Recipe
            {
                Name = "New Recipe"
            };

            ListRecipes.Add(newRecipe);

            SelectedRecipe = newRecipe;

            IsEditingEnabled = true;
        }

        private Recipe _selecteRecipe;

        public Recipe SelectedRecipe
        {
            get { return _selecteRecipe; }
            set
            {
                _selecteRecipe = value;
                NotifyPropertyChanged();
                if (_selecteRecipe != null)
                {
                    SelectedName = _selecteRecipe.Name;
                    SelectedIngredients = new ObservableCollection<Beverage>(_selecteRecipe.Ingredients);
                }
                else
                {
                    IsEditingEnabled = false;
                    SelectedName = string.Empty;
                    SelectedIngredients = new ObservableCollection<Beverage>();
                }
            }
        }

        public void AddBeverageToRecipe()
        {
            //all loaded beverages
            IEnumerable<Beverage> allBeverages = _mainVM.VMBeverage.ListBeverages;

            //beverages inside the recipe: Waring: not the same instances as in allBeverages
            IEnumerable<Beverage> recipeBeverage = SelectedRecipe.Ingredients;

            //instance of allBeverages of beverages inside the recipe
            IEnumerable<Beverage> inclusiveBeverages = from singleAllBev in allBeverages
                                                       join singleRecBev in recipeBeverage
                                                       on singleAllBev.GUID equals singleRecBev.GUID
                                                       select singleAllBev;

            //all other beverages
            IEnumerable<Beverage> exclusiveBeverages = allBeverages.Except(inclusiveBeverages);

            SelectBeverageDialog dialog = new SelectBeverageDialog(exclusiveBeverages);

            bool? dialogresult = dialog.ShowDialog();

            if (dialogresult.HasValue == dialogresult.Value)
            {
                Beverage newBeverage = dialog.SelectedBeverage;

                SelectedRecipe.Ingredients.Add(newBeverage);

                SelectedIngredients.Add(newBeverage);
            }
        }

        public void DeleteRecipe()
        {
            if (SelectedRecipe == null)
                return;

            if (System.Windows.MessageBox.Show($"Delete recipe \"{SelectedRecipe.Name}\"? This cannot be undone!", "Are You Sure?", System.Windows.MessageBoxButton.YesNo) 
                == System.Windows.MessageBoxResult.Yes)
            {
                ListRecipes.Remove(SelectedRecipe);
                _mainVM.SaveCurrentState();
            }
        }

        public void RemoveSelectedBeverage()
        {
            if (SelectedIngredient != null)
            {
                SelectedIngredients.Remove(SelectedIngredient);
                //update original list
                SelectedRecipe.Ingredients = new List<Beverage>(from recIngred in SelectedRecipe.Ingredients
                                                                join vmIngred in SelectedIngredients 
                                                                on recIngred.GUID equals vmIngred.GUID
                                                                select recIngred);
                SelectedIngredient = null;
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
            if (SelectedIngredients == null || SelectedIngredients.Count == 0)
            {
                System.Windows.MessageBox.Show("There has to be at least one ingredient in each recipe!");
                return;
            }

            if (string.IsNullOrEmpty(SelectedName))
            {
                System.Windows.MessageBox.Show("Name cannot be empty");
                return;
            }

            //check if another recipe with that name exists
            if (ListRecipes.Any(x => x.Name == SelectedName && x != SelectedRecipe))
            {
                System.Windows.MessageBox.Show($"A recipe with the name \"{SelectedName}\" already exists");
                return;
            }

            SelectedRecipe.Ingredients = new List<Beverage>(SelectedIngredients);
            SelectedRecipe.Name = SelectedName;

            _mainVM.SaveCurrentState();

            //Update changed names
            ListRecipes = new ObservableCollection<Recipe>(ListRecipes);

            IsEditingEnabled = false;
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
                IsBeverageSelected = _selectedIngredient != null;
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

            if (SelectedRecipe != null)
            {
                SelectedRecipe = ListRecipes.FirstOrDefault(x => x.Name == SelectedRecipe.Name);
            }
        }
    }
}
