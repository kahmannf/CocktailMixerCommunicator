using CocktailMixerCommunicator.Communication;
using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CocktailMixerWPFMaster
{
    public class SupplyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private MainViewModel _vmMain;

        public SupplyViewModel(MainViewModel vmmain)
        {
            _vmMain = vmmain;
        }

        public void LoadFromCMState(CMGlobalState state)
        {
            SupplySlotAmount = state.SupplySlotAmount;
            WasteGatePortId = state.WasteGatePortId;
            CompressorPortId = state.CompressorPortId;
            _mixerSupplyItems = state.Supply;
            _beverages = state.BeverageDataBase;

            NotifyPropertyChanged("SupplyItemViews");
        }

        private List<MixerSupplyItem> _mixerSupplyItems;

        private List<Beverage> _beverages;

        private int _supplySlotAmount;

        public int SupplySlotAmount
        {
            get { return _supplySlotAmount; }
            set
            {
                _supplySlotAmount = value;
                NotifyPropertyChanged();
            }
        }

        private byte _compressorPortId;

        public byte CompressorPortId
        {
            get { return _compressorPortId; }
            set
            {
                _compressorPortId = value;
                NotifyPropertyChanged();
            }
        }

        private byte _wasteGatePortId;

        public byte WasteGatePortId
        {
            get { return _wasteGatePortId; }
            set
            {
                _wasteGatePortId = value;
                NotifyPropertyChanged();
            }
        }



        public IEnumerable<SupplySlotView> SupplyItemViews => from slot in _mixerSupplyItems
                                                              select new SupplySlotView()
                                                              {
                                                                  SupplyItem = slot,
                                                                  Beverage = _beverages.FirstOrDefault(x => x.GUID == slot.GUID_Beverage)
                                                              };

        public void ClearSlot(SupplySlotView slotView)
        {
            if (slotView != null && slotView.Beverage != null && slotView.SupplyItem != null)
            {
                CMGlobalState state = CMGlobalState.LoadStateFromFile(_vmMain.Config.CMStateDirectory);

                state.ClearSupplySlot(slotView.SupplyItem.SupplySlotID, _vmMain.Config.CMStateDirectory);
            }
        }

        public void FillSlot(SupplySlotView slotView)
        {
            if (slotView != null && slotView.SupplyItem != null)
            {
                CMGlobalState state = CMGlobalState.LoadStateFromFile(_vmMain.Config.CMStateDirectory);

                Dialogs.SelectBeverageDialog dialog = new Dialogs.SelectBeverageDialog(state.BeverageDataBase);

                bool? dialogResult = dialog.ShowDialog();

                if(dialogResult.HasValue && dialogResult.Value && dialog.SelectedBeverage is Beverage b)
                {
                    if (b.RatioAmount <= 1)
                    {
                        MessageBox.Show("You have to enter a amount larger than 1 ml");
                    }
                    else
                    {
                        state.SetSupplySlot(b.GUID, b.RatioAmount, slotView.SupplyItem.SupplySlotID, _vmMain.Config.CMStateDirectory);
                    }
                }

                
            }
        }

        public void SaveSettings()
        {
            if (SupplySlotAmount > 0)
            {
                CMGlobalState state = CMGlobalState.LoadStateFromFile(_vmMain.Config.CMStateDirectory);

                state.SupplySlotAmount = SupplySlotAmount;
                state.CompressorPortId = CompressorPortId;
                state.WasteGatePortId = WasteGatePortId;

                state.ApplyChanges(_vmMain.Config.CMStateDirectory);
                
            }
        }

        public void OpenSlot(int slotId)
        {
            SerialCommunicator com = new SerialCommunicator(_vmMain.Config.COMPort, _vmMain.Config.BaudRate);
            com.OpenSlot(slotId);
        }

        public void CloseSlot(int slotId)
        {
            SerialCommunicator com = new SerialCommunicator(_vmMain.Config.COMPort, _vmMain.Config.BaudRate);
            com.CloseSlot(slotId);
        }

        public void CloseAllSlots()
        {
            try
            {
                CMGlobalState state = CMGlobalState.LoadStateFromFile(_vmMain.Config.CMStateDirectory);

                SerialCommunicator com = new SerialCommunicator(_vmMain.Config.COMPort, _vmMain.Config.BaudRate);
                com.ResetSlots(state.SupplySlotAmount);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}
