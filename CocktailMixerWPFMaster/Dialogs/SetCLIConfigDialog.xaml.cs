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
    /// Interaktionslogik für SetCLIConfigDialog.xaml
    /// </summary>
    public partial class SetCLIConfigDialog : Window
    {
        public SetCLIConfigDialog()
        {
            InitializeComponent();
            this.DataContext = new SetCLIConfigViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as SetCLIConfigViewModel).Validate())
            {
                this.DialogResult = true;
            }
        }
    }
}
