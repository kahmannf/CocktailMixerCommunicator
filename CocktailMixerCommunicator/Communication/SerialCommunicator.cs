using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
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

        public Task<int> SendRequestAsync(Beverage b, int amountInMl, CMGlobalState state)
        {
            return Task.Run(() => SendRequest(b, amountInMl, state));
        }

        public int SendRequest(Beverage b, int amountInMl, CMGlobalState state)
        {
            try
            {
                _serialPort.Open();
                
                MixerSupplyItem supplyItem = state.Supply.FirstOrDefault(x => x.GUID_Beverage == b.GUID);

                if (supplyItem == null)
                {
                    throw new ArgumentException("Can only use beverages that are in the supply");
                }

                StatusMessageChanged?.Invoke(this, $"Dispnsing Beverage: {b.Name}, {amountInMl} ml");

                byte portId = (byte)state.MapSlotToPort(supplyItem.SupplySlotID);

                _serialPort.Write(new byte[] { state.CompressorPortId }, 0, 1); //enable compressor
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 1 }, 0, 1);
                _serialPort.BaseStream.Flush();


                _serialPort.Write(new byte[] { portId }, 0, 1); //open slot valve
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 1 }, 0, 1);
                _serialPort.BaseStream.Flush();

                int remainingTime = Convert.ToInt32(b.AmountTimeCoefficient * amountInMl);

                remainingTime -= remainingTime % 10;


                for (; remainingTime > 0; remainingTime -= 10)
                {

                    StatusMessageChanged?.Invoke(this, remainingTime / 10 + " seconds remaining");

                    System.Threading.Thread.Sleep(100);
                }


                _serialPort.Write(new byte[] { portId }, 0, 1);//clsoe slot valve
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 0 }, 0, 1);
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { state.CompressorPortId }, 0, 1); //disable compressor
                _serialPort.BaseStream.Flush();

                _serialPort.Write(new byte[] { 0 }, 0, 1);
                _serialPort.BaseStream.Flush();

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
