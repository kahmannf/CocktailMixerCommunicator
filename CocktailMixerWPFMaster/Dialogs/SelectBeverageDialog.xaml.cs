using CocktailMixerCommunicator.Model;
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
using System.Windows.Shapes;

namespace CocktailMixerWPFMaster.Dialogs
{
    /// <summary>
    /// Interaktionslogik für SelectBeverageDialog.xaml
    /// </summary>
    public partial class SelectBeverageDialog : Window
    {
        public SelectBeverageDialog(IEnumerable<Beverage> selectFrom)
        {
            InitializeComponent();
            this.DataContext = new SelectBeverageViewModel(selectFrom);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SelectBeverageViewModel vm)
            {
                SelectedBeverage = vm.SelectedBeverage;
                SelectedBeverage.Amount = vm.Amount;
                this.DialogResult = SelectedBeverage != null;
            }
        }

        public Beverage SelectedBeverage { get; private set; }
    }
}
