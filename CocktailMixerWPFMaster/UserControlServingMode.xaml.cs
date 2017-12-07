using CocktailMixerCommunicator.Communication;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CocktailMixerWPFMaster
{
    /// <summary>
    /// Interaktionslogik für UserControlServingMode.xaml
    /// </summary>
    public partial class UserControlServingMode : UserControl
    {
        public MainViewModel VmMain { get; set; }


        public UserControlServingMode()
        {
            InitializeComponent();
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            VmMain = (this.DataContext as ServingModeViewModel).VMMain;

            string dir = VmMain.Config.CMStateDirectory;
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

            state.SetSupplySlot(wasser1.GUID, 1, dir);
            state.SetSupplySlot(wasser2.GUID, 2, dir);

            SerialCommunicator com = new SerialCommunicator(VmMain.Config.COMPort, VmMain.Config.BaudRate);

            com.SendRequest(wasser1, 100, state);
            com.SendRequest(wasser2, 400, state);

        }
    }
}
