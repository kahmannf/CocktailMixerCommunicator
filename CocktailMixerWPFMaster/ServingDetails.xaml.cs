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
    /// Interaktionslogik für ServingDetails.xaml
    /// </summary>
    public partial class ServingDetails : UserControl
    {
        public ServingDetails()
        {
            InitializeComponent();
        }

        private void ButtonMix_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ServingModeViewModel)?.MixIt();
        }
    }
}
