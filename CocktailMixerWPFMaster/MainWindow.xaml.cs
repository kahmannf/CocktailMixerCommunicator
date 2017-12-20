using CocktailMixerCommunicator;
using CocktailMixerCommunicator.Web;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CocktailMixerCommunicator.Model;

namespace CocktailMixerWPFMaster
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel vm = new MainViewModel();

            this.DataContext = vm;

            vm.InitData();
        }

        private void ButtonAddRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.AddRecipe();
        }

        private void ButtonEditRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.IsEditingEnabled = (this.DataContext as MainViewModel).VMRecipe.SelectedRecipe != null;
        }

        private void ButtonRemoveBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.RemoveSelectedBeverage();
        }

        private void ButtonAddBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.AddBeverageToRecipe();
        }

        private void ButtonCancelRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.CancelEdit();
        }

        private void ButtonSaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.SaveRecipe();
        }

        private void ButtonDeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.DeleteRecipe();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMBeverage.IsEditEnabled = (this.DataContext as MainViewModel).VMBeverage.SelectedBeverage != null;
        }

        private void ButtonCancelBaverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMBeverage.CancelEdit();
        }

        private void ButtonSaveBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMBeverage.SaveBeverage();
        }

        private void ButtonAddNewBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMBeverage.AddBeverage();
        }

        private void ButtonDeleteBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMBeverage.DeleteBeverage();
        }

        private void ButtonFillSlot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is SupplySlotView slotView)
            {
                (this.DataContext as MainViewModel)?.VMSupply?.FillSlot(slotView);
            }
        }

        private void ButtonClearSlot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is SupplySlotView slotView)
            {
                (this.DataContext as MainViewModel)?.VMSupply?.ClearSlot(slotView);
            }
        }

        private void ButtonOpenSlot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is SupplySlotView slotView)
            {
                (this.DataContext as MainViewModel)?.VMSupply?.OpenSlot(slotView.SupplyItem.SupplySlotID);
            }
        }

        private void ButtonCloseSlot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is SupplySlotView slotView)
            {
                (this.DataContext as MainViewModel)?.VMSupply?.CloseSlot(slotView.SupplyItem.SupplySlotID);
            }
        }

        private void ButtonSaveSupplySettings_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel)?.VMSupply?.SaveSettings();
        }

        private void ButtonCloseAll_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vmmain = (this.DataContext as MainViewModel);

            vmmain.VMSupply.CloseAllSlots();
        }

        private void ButtonOpenCompressor_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vmmain = (this.DataContext as MainViewModel);

            CocktailMixerCommunicator.Model.CMGlobalState state = CocktailMixerCommunicator.Model.CMGlobalState.LoadStateFromFile(vmmain.Config.CMStateDirectory);

            vmmain.VMSupply.OpenSlot(state.CompressorPortId);
        }

        private void ButtonCloseCompressor_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vmmain = (this.DataContext as MainViewModel);

            CocktailMixerCommunicator.Model.CMGlobalState state = CocktailMixerCommunicator.Model.CMGlobalState.LoadStateFromFile(vmmain.Config.CMStateDirectory);

            vmmain.VMSupply.CloseSlot(state.CompressorPortId);
        }

        private void ButtonOpenWasteGate_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vmmain = (this.DataContext as MainViewModel);

            CocktailMixerCommunicator.Model.CMGlobalState state = CocktailMixerCommunicator.Model.CMGlobalState.LoadStateFromFile(vmmain.Config.CMStateDirectory);

            vmmain.VMSupply.OpenSlot(state.WasteGatePortId);
        }

        private void ButtonCloseWasteGate_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vmmain = (this.DataContext as MainViewModel);

            CocktailMixerCommunicator.Model.CMGlobalState state = CocktailMixerCommunicator.Model.CMGlobalState.LoadStateFromFile(vmmain.Config.CMStateDirectory);

            vmmain.VMSupply.CloseSlot(state.WasteGatePortId);
        }

        private void ButtonListener_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if ((this.DataContext as MainViewModel).ToggleListener())
            {
                b.Content = "Stop Listening";
                b.Background = new SolidColorBrush(Color.FromRgb(142, 22, 40));
            }
            else
            {
                b.Content = "Start Listening";
                b.Background = new SolidColorBrush(Color.FromRgb(22, 142, 80));
            }
        }

        private void ButtonExportState_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".json";

            if (sfd.ShowDialog() is bool b && b)
            {
                DataExport.ExportCmGlobalState(sfd.FileName, CMGlobalState.LoadStateFromFile((this.DataContext as MainViewModel).Config.CMStateDirectory));
            }
        }
    }
}
