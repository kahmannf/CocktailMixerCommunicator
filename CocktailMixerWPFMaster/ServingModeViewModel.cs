using CocktailMixerCommunicator.Communication;
using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CocktailMixerWPFMaster
{
    public class ServingModeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private MainViewModel _vmMain;

        public ServingModeViewModel(MainViewModel vmmain)
        {
            _vmMain = vmmain;

            BackToSelection();

            MissingIngredientsVisibility = Visibility.Collapsed;

            GlassSizeInMl = 300;
        }

        #region Visibilities

        private Visibility _modeSelectionVisibility;

        public Visibility ModeSelectionVisibility
        {
            get { return _modeSelectionVisibility; }
            set
            {
                _modeSelectionVisibility = value;
                NotifyPropertyChanged("ModeSelectionVisibility");
            }
        }

        private Visibility _contentRecipesVisibility;

        public Visibility ContentRecipesVisibility
        {
            get { return _contentRecipesVisibility; }
            set
            {
                _contentRecipesVisibility = value;
                NotifyPropertyChanged("ContentRecipesVisibility");
            }
        }

        private Visibility _contentBeveragesVisibility;

        public Visibility ContentBeveragesVisibility
        {
            get { return _contentBeveragesVisibility; }
            set
            {
                _contentBeveragesVisibility = value;
                NotifyPropertyChanged("ContentBeveragesVisibility");
            }
        }

        private Visibility _servingVisibility;

        public Visibility ServingVisibility
        {
            get { return _servingVisibility; }
            set
            {
                _servingVisibility = value;
                NotifyPropertyChanged("ServingVisibility");
            }
        }

        private Visibility _availableIngredientsVisibility;

        public Visibility AvailableIngredientsVisibility
        {
            get { return _availableIngredientsVisibility; }
            set
            {
                _availableIngredientsVisibility = value;
                NotifyPropertyChanged("AvailableIngredientsVisibility");
            }
        }

        private Visibility _missingIngredientsVisibility;

        public Visibility MissingIngredientsVisibility
        {
            get { return _missingIngredientsVisibility; }
            set
            {
                _missingIngredientsVisibility = value;
                NotifyPropertyChanged("MissingIngredientsVisibility");
            }
        }



        #endregion

        private List<Recipe> _recipes;

        public List<Recipe> Recipes
        {
            get { return _recipes; }
            set
            {
                _recipes = value;
                NotifyPropertyChanged("Recipes");
            }
        }

        private Recipe _selectedRecipe;

        public Recipe SelectedRecipe
        {
            get { return _selectedRecipe; }
            set
            {
                _selectedRecipe = value;
                NotifyPropertyChanged("SelectedRecipe");
                SetMixStatus();
            }
        }


        private List<Beverage> _beverages;

        public List<Beverage> Beverages
        {
            get { return _beverages; }
            set
            {
                _beverages = value;
                NotifyPropertyChanged("Beverages");
            }
        }

        private List<MixerSupplyItem> _supply;

        private int _glassSizeInMl;

        public int GlassSizeInMl
        {
            get { return _glassSizeInMl; }
            set
            {
                _glassSizeInMl = value;
                NotifyPropertyChanged("GlassSizeInMl");
                SetMixStatus(true);
            }
        }

        private bool _canBeMixed;

        public bool CanBeMixed
        {
            get { return _canBeMixed; }
            set
            {
                _canBeMixed = value;
                NotifyPropertyChanged("CanBeMixed");
            }
        }


        private string _mixStatus;

        public string MixStatus
        {
            get { return _mixStatus; }
            set
            {
                _mixStatus = value;
                NotifyPropertyChanged("MixStatus");
            }
        }

        private List<Beverage> _missingIngredients;

        public List<Beverage> MissingIngredients
        {
            get { return _missingIngredients; }
            set
            {
                _missingIngredients = value;
                NotifyPropertyChanged("MissingIngredients");
            }
        }

        private List<Beverage> _availableIngredients;

        public List<Beverage> AvailableIngredients
        {
            get { return _availableIngredients; }
            set
            {
                _availableIngredients = value;
                NotifyPropertyChanged("AvailableIngredients");
            }
        }

        private ObservableCollection<Beverage> _remainingIngredients;

        public ObservableCollection<Beverage> RemainingIngredients
        {
            get { return _remainingIngredients; }
            set
            {
                _remainingIngredients = value;
                NotifyPropertyChanged("RemainingIngredients");
            }
        }

        private ObservableCollection<Beverage> _pouringIngredients;

        public ObservableCollection<Beverage> PouringIngredients
        {
            get { return _pouringIngredients; }
            set
            {
                _pouringIngredients = value;
                NotifyPropertyChanged("PouringIngredients");
            }
        }

        private ObservableCollection<Beverage> _servedIngredient;

        public ObservableCollection<Beverage> ServedIngredients
        {
            get { return _servedIngredient; }
            set
            {
                _servedIngredient = value;
                NotifyPropertyChanged("ServedIngredients");
            }
        }


        public void LoadFromCMState(CMGlobalState state)
        {
            Beverages = new List<Beverage>(state.BeverageDataBase);
            Recipes = new List<Recipe>(state.Recipes);
            _supply = new List<MixerSupplyItem>(state.Supply);
        }


        public void ShowBeverages()
        {
            ModeSelectionVisibility = ContentRecipesVisibility = ServingVisibility = Visibility.Collapsed;
            ContentBeveragesVisibility = Visibility.Visible;
        }

        public void ShowRecipes()
        {
            ModeSelectionVisibility = ContentBeveragesVisibility = ServingVisibility = Visibility.Collapsed;
            ContentRecipesVisibility = Visibility.Visible;
        }

        public void BackToSelection()
        {
            ContentRecipesVisibility = ContentBeveragesVisibility = ServingVisibility = Visibility.Collapsed;
            ModeSelectionVisibility = Visibility.Visible;
        }

        private void SetMixStatus(bool glassSizeSender = false)
        {
            bool result = false;
            if (GlassSizeInMl > 0)
            {
                if (SelectedRecipe != null && SelectedRecipe.Ingredients != null && SelectedRecipe.Ingredients.Count > 0)
                {
                    if (SelectedRecipe.DefaultAmountML != 0 && !glassSizeSender)
                    {
                        GlassSizeInMl = SelectedRecipe.DefaultAmountML;
                        return; // Setting the glasssize will call this method again
                    }

                    CMGlobalState state = CMGlobalState.LoadStateFromFile(_vmMain.Config.CMStateDirectory);

                    int totalParts = SelectedRecipe.Ingredients.Select(x => x.RatioAmount).Aggregate((y, z) => y + z);

                    IEnumerable<Beverage> missingIngredients = SelectedRecipe.Ingredients.Where(x => !state.HasAmount(x, (int)((GlassSizeInMl) * ((x.RatioAmount) / ((double)totalParts)))));

                    MissingIngredients = new List<Beverage>(missingIngredients);
                    MissingIngredientsVisibility = MissingIngredients.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                    AvailableIngredients = new List<Beverage>(SelectedRecipe.Ingredients.Except(missingIngredients));
                    AvailableIngredientsVisibility = AvailableIngredients.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                    if (missingIngredients.Count() == 0)
                    {
                        result = true;
                        MixStatus = "Can be mixed.";
                    }
                    else
                    {
                        MixStatus = "Insufficient Supplies!";
                    }
                }
                else
                {
                    MixStatus = "Please select a Recipe/Beverage.";
                }
            }
            else
            {
                MixStatus = "Please specify a glass size thatis larger than zero!";
            }

            CanBeMixed = result;
        }

        public async void MixIt()
        {
            SetMixStatus();

            if (CanBeMixed)
            {
                try
                {
                    ModeSelectionVisibility = ContentRecipesVisibility = ContentBeveragesVisibility = Visibility.Collapsed;
                    ServingVisibility = Visibility.Visible;

                    RemainingIngredients = new ObservableCollection<Beverage>(SelectedRecipe.Ingredients);
                    PouringIngredients = new ObservableCollection<Beverage>();
                    ServedIngredients = new ObservableCollection<Beverage>();

                    CMGlobalState state = CMGlobalState.LoadStateFromFile(_vmMain.Config.CMStateDirectory);

                    SerialCommunicator com = new SerialCommunicator(_vmMain.Config.COMPort, _vmMain.Config.BaudRate);

                    int totalParts = RemainingIngredients.Select(x => x.RatioAmount).Aggregate((x, y) => x + y);

                    bool first = true;

                    while (RemainingIngredients.Count > 0)
                    {
                        Beverage b = RemainingIngredients.First();
                        RemainingIngredients.Remove(b);
                        PouringIngredients.Add(b);

                        int amount = (int)((double)GlassSizeInMl * b.RatioAmount / (double)totalParts);

                        await com.SendRequestAsync(b, amount, state, _vmMain.Config.CMStateDirectory, first, RemainingIngredients.Count == 0);

                        if(first)
                            first = false;

                        PouringIngredients.Remove(b);

                        ServedIngredients.Add(b);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error");
                }

                BackToSelection();
            }
        }

        public void OrderDummyTest()
        {


            string dir = _vmMain.Config.CMStateDirectory;
            CMGlobalState.CreateNew(dir);

            CMGlobalState state = CMGlobalState.LoadStateFromFile(dir);

            Beverage wasser1 = new Beverage()
            {
                AlcoholVolPercentage = 0,
                AmountTimeCoefficient = 1,
                GUID = Guid.NewGuid().ToString(),
                Name = "Wasser1",
                RatioAmount = 0
            };

            Beverage wasser2 = new Beverage()
            {
                AlcoholVolPercentage = 0,
                AmountTimeCoefficient = 1,
                GUID = Guid.NewGuid().ToString(),
                Name = "Wasser2",
                RatioAmount = 0
            };

            state.AddBeverageToDatabase(wasser1, dir);
            state.AddBeverageToDatabase(wasser2, dir);

            state.CompressorPortId = 0;
            state.ApplyChanges(dir);

            state.SetSupplySlot(wasser1.GUID, 100, 1, dir);
            state.SetSupplySlot(wasser2.GUID, 400, 2, dir);

            SerialCommunicator com = new SerialCommunicator(_vmMain.Config.COMPort, _vmMain.Config.BaudRate);

            com.SendRequest(wasser1, 100, state, _vmMain.Config.CMStateDirectory);
            com.SendRequest(wasser2, 400, state, _vmMain.Config.CMStateDirectory);
        }
    }
}
