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

            vm.LoadData();
        }

        private void ButtonEditRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.IsEditingEnabled = true;
        }

        private void ButtonRemoveBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.RemoveSelectedBeverage();
        }

        private void ButtonAddBeverage_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.AddRecipe();
        }

        private void ButtonCancelRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.CancelEdit();
        }

        private void ButtonSaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).VMRecipe.SaveRecipe();
        }
    }
}
