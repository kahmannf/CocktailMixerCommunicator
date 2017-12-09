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

        public void InitData()
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


            VMServingMode = new ServingModeViewModel(this);

            VMBeverage = new BeverageViewModel(this);

            VMRecipe = new RecipeViewModel(this);

            VMSupply = new SupplyViewModel(this);


            CMGlobalState state = CMGlobalState.LoadStateFromFile(_config.CMStateDirectory);

            UpdateCMState(state);

            CMGlobalState.StateChangesSaved += (sender, e) =>
            {
                if (sender is CMGlobalState senderState)
                {
                    UpdateCMState(senderState);
                }
            };
        }

        private void UpdateCMState(CMGlobalState state)
        {
            VMRecipe.LoadFromCMState(state);

            VMBeverage.LoadFromCMState(state);

            VMSupply.LoadFromCMState(state);

            VMServingMode.LoadFromCMState(state);
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

        private SupplyViewModel _vmSupply;

        public SupplyViewModel VMSupply
        {
            get { return _vmSupply; }
            set
            {
                _vmSupply = value;
                NotifyPropertyChanged();
            }
        }


        public void SaveCurrentState()
        {
            CMGlobalState state = CMGlobalState.LoadStateFromFile(Config.CMStateDirectory);

            state.BeverageDataBase = new List<Beverage>(_vmBeverage.ListBeverages);
            state.Recipes = new List<Recipe>(_vmRecipe.ListRecipes);

            //todo update this
            //state.Supply = new List<MixerSupplyItem>();

            state.ApplyChanges(Config.CMStateDirectory);

            //saving will now invoke a event which will cause the vms to refresh
            ////saving may update some properties
            //VMRecipe.LoadFromCMState(state);
            //VMBeverage.LoadFromCMState(state);
            //VMSupply.LoadFromCMState(state);
        }
    }
}
