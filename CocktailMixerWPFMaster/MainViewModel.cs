using CocktailMixerCLI;
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

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public void LoadData()
        {
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string cmcliConfigFileName = Path.Combine(baseDir, CMCLI.CONFIG_FILE_NAME);

            if (!File.Exists(cmcliConfigFileName))
            {
                SetCLIConfigDialog dialog = new SetCLIConfigDialog();

                bool? dialogresult = dialog.ShowDialog();

                if (!dialogresult.HasValue || !dialogresult.Value)
                {
                    Environment.Exit(0);
                }
                else
                {
                    SetCLIConfig(dialog.DataContext as SetCLIConfigViewModel);
                }
            }

            if (CMCLI.Configuration == null)
            {
                CMCLI.LoadOrCreateConfig();
            }

            VMRecipe = new RecipeViewModel(this);

            VMBeverage = new BeverageViewModel(this);

            CMGlobalState state = CMGlobalState.LoadStateFromFile(CMCLI.Configuration.CMStateDirectory);

            VMRecipe.LoadFromCMState(state);

            VMBeverage.LoadFromCMState(state);
        }


        public void SetCLIConfig(SetCLIConfigViewModel vm)
        {
            string[] args = new string[] { "config", "set", "", "" };

            args[2] = "serialport";
            args[3] = vm.COMPortName;

            CMCLI.Main(args);

            args[2] = "baudrate";
            args[3] = vm.BaudRate.ToString();

            CMCLI.Main(args);

            args[2] = "statedir";
            args[3] = vm.CMStateDirectory;

            CMCLI.Main(args);
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


    }
}
