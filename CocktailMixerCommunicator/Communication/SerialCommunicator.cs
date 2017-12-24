using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Communication
{
    /// <summary>
    /// Communicates via a serial port
    /// </summary>
    public class SerialCommunicator
    {
        /// <summary>
        /// The predifined Header fot the serial communication
        /// 
        /// KAFACMH => Kahmann Felix Arduino Cocktail Mixer Header;
        /// </summary>
        [Obsolete("This header is not in use any longer", true)]
        private readonly byte[] HEADER_TEMPLATE = "KAFACMH".Select(x => (byte)x).ToArray();

        public const int COMPRESSOR_PREASURE_BUILD_TIME_MS = 1000;

        /// <summary>
        /// Reference to the serialport object that is used for communication
        /// </summary>
        private SerialPort _serialPort;
        public string PortName => _serialPort.PortName;
        public int BaudRate => _serialPort.BaudRate;

        public event EventHandler<string> StatusMessageChanged;

        public SerialCommunicator(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
        }

        public Task<int> SendRequestAsync(Beverage b, int amountInMl, CMGlobalState state, string stateDir, bool first, bool last)
        {
            return Task.Run(() => SendRequest(b, amountInMl, state, stateDir, first, last));
        }

        public void OpenSlot(int slotId)
        {
            OpenPort(CMGlobalState.MapSlotToPort(slotId), true); 
        }

        public void CloseSlot(int slotId)
        {
            ClosePort(CMGlobalState.MapSlotToPort(slotId), true);
        }

        private void OpenPort(byte portId, bool openPort = false)
        {
            if (openPort)
                _serialPort.Open();

            if (portId >= 0)
            {
                _serialPort.Write(new byte[] { portId }, 0, 1);
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 1 }, 0, 1);
                _serialPort.BaseStream.Flush();
            }

            if (openPort)
                _serialPort.Close();
        }

        private void ClosePort(byte portId, bool openPort = false)
        {
            if(openPort)
                _serialPort.Open();

            if (portId >= 0)
            {

                _serialPort.Write(new byte[] { portId }, 0, 1);
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 0 }, 0, 1);
                _serialPort.BaseStream.Flush();
            }

            if(openPort)
                _serialPort.Close();
        }

        public int SendRequest(Beverage b, int amountInMl, CMGlobalState state, string stateDir, bool first = true, bool last = true)
        {
            try
            {
                if (!state.HasAmount(b, amountInMl))
                    throw new InvalidOperationException($"Requested {amountInMl}ml of {b.Name}: Not enough Beverage in supply");

                _serialPort.Open();
                
                MixerSupplyItem supplyItem = state.Supply.FirstOrDefault(x => x.GUID_Beverage == b.GUID);

                if (supplyItem == null)
                {
                    throw new ArgumentException("Can only use beverages that are in the supply");
                }

                StatusMessageChanged?.Invoke(this, $"Dispnsing Beverage: {b.Name}, {amountInMl} ml");

                byte portId = CMGlobalState.MapSlotToPort(supplyItem.SupplySlotID);


                OpenPort((byte)state.AirFlowValvePortId);


                OpenPort((byte)state.CompressorPortId);

                Thread.Sleep(COMPRESSOR_PREASURE_BUILD_TIME_MS); //build up some preasure

                OpenPort(portId);
            
                int remainingTime = Convert.ToInt32(b.AmountTimeCoefficient * amountInMl * 5.9); 

                remainingTime -= remainingTime % 10;


                for (; remainingTime > 0; remainingTime -= 10)
                {

                    StatusMessageChanged?.Invoke(this, remainingTime / 10 + " seconds remaining");

                    Thread.Sleep(100);
                }

                ClosePort((byte)state.CompressorPortId);

                ClosePort((byte)state.AirFlowValvePortId);

                OpenPort((byte)state.WasteGatePortId);

                Thread.Sleep(RandomUtilities.MathUtil.Util.TakeLarger(2000, (int)(amountInMl * ((b.AmountTimeCoefficient - 1 * 2) + 1) * 18))); //reduce preasure

                ClosePort(portId);

                ClosePort((byte)state.WasteGatePortId);

                state.RemoveAmount(b, amountInMl, stateDir);

                _serialPort.Close();

                return 0;

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send the Request", ex);
            }
        }

        /// <summary>
        /// Sets all slots to disabled
        /// </summary>
        /// <param name="slotCount"></param>
        /// <returns></returns>
        public int ResetSlots(int slotCount)
        {
            _serialPort.Open();

            for (int i = 0; i < slotCount; i++)
            {
                _serialPort.Write(new byte[] { (byte)i }, 0, 1);
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 0 }, 0, 1);
                _serialPort.BaseStream.Flush();
            }

            return 0;
        }

        /// <summary>
        /// Waits for a resultcode transmitted through the serial port
        /// </summary>
        /// <returns>return code. if serialPort is not Open -1</returns>
        private int WaitForResult()
        {
            if (!_serialPort.IsOpen)
                return -1;

            while (true)
            {
                if (_serialPort.BytesToRead > 0)
                {
                    return _serialPort.ReadByte();
                }

                System.Threading.Thread.Sleep(1);
            }
        }
    }
}
