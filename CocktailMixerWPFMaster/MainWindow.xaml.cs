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
    }
}
