using CocktailMixerCommunicator.Model;
using CocktailMixerWPFMaster.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerWPFMaster
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _configFileName => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.xml");

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Configuration _config;

        public Configuration Config => _config;

        public void LoadData()
        {
            if (!File.Exists(_configFileName))
            {
                SetCLIConfigDialog dialog = new SetCLIConfigDialog();

                bool? dialogresult = dialog.ShowDialog();

                if (!dialogresult.HasValue || !dialogresult.Value)
                {
                    Environment.Exit(0);
                }
                else
                {
                    SetConfig(dialog.DataContext as SetCLIConfigViewModel);
                }
            }
            else
            {
                _config = Configuration.LoadFromPath(_configFileName);
            }
            

            VMRecipe = new RecipeViewModel(this);

            VMBeverage = new BeverageViewModel(this);

            VMServingMode = new ServingModeViewModel(this);


            CMGlobalState state = CMGlobalState.LoadStateFromFile(_config.CMStateDirectory);

            VMRecipe.LoadFromCMState(state);

            VMBeverage.LoadFromCMState(state);
        }


        public void SetConfig(SetCLIConfigViewModel vm)
        {
            Configuration config = new Configuration()
            {
                BaudRate = vm.BaudRate,
                CMStateDirectory = vm.CMStateDirectory,
                COMPort = vm.COMPortName,
            };

            _config = config;

            Configuration.SaveToPath(_configFileName, config);
        }

        private RecipeViewModel _vmRecipe;

        public RecipeViewModel VMRecipe
        {
            get { return _vmRecipe; }
            set
            {
                _vmRecipe = value;
                NotifyPropertyChanged();
            }
        }

        private BeverageViewModel _vmBeverage;

        public BeverageViewModel VMBeverage
        {
            get { return _vmBeverage; }
            set
            {
                _vmBeverage = value;
                NotifyPropertyChanged();
            }
        }

        private ServingModeViewModel _vmServingMode;

        public ServingModeViewModel VMServingMode
        {
            get { return _vmServingMode; }
            set
            {
                _vmServingMode = value;
                NotifyPropertyChanged();
            }
        }


        public void SaveCurrentState()
        {
            CMGlobalState state = new CMGlobalState();

            state.BeverageDataBase = new List<Beverage>(_vmBeverage.ListBeverages);
            state.Recipes = new List<Recipe>(_vmRecipe.ListRecipes);

            //todo update this
            state.Supply = new List<MixerSupplyItem>();

            throw new Exception();
            state.ApplyChanges(string.Empty);

            //saving may update some properties
            VMRecipe.LoadFromCMState(state);
            VMBeverage.LoadFromCMState(state);
        }
    }
}
