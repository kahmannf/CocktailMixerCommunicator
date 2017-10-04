﻿using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Communication
{
    public class SerialCommunicator
    {
        /// <summary>
        /// The predifined Header fot the serial communication
        /// 
        /// KAFACMH => Kahmann Felix Arduino Cocktail Mixer Header;
        /// </summary>
        private readonly byte[] HEADER_TEMPLATE = "KAFACMH".Select(x => (byte)x).ToArray();

        private bool _isQueueActive = false;
        public bool IsQueueActive => _isQueueActive;

        private SerialPort _serialPort;
        public string PortName => _serialPort.PortName;
        public int BaudRate => _serialPort.BaudRate;

        public SerialCommunicator(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
        }

        public Task<int> SendRequestAsync(IEnumerable<Beverage> beverages, CMGlobalState state)
        {
            return Task.Run(() => SendRequest(beverages, state));
        }

        public int SendRequest(IEnumerable<Beverage> beverages, CMGlobalState state)
        {
            try
            {
                _serialPort.Open();

                foreach (Beverage b in beverages)
                {
                    MixerSupplyItem supplyItem = state.Supply.FirstOrDefault(x => x.GUID_Beverage == b.GUID);

                    if (supplyItem == null)
                    {
                        throw new ArgumentException("Can only use beverages that are in the supply");
                    }

                    Console.WriteLine($"Dispnsing Beverage: {b.Name}, {b.Amount} ml");

                    _serialPort.Write(new byte[] { (byte)supplyItem.SupplySlotID }, 0, 1);
                    _serialPort.BaseStream.Flush();

                    _serialPort.Write(new byte[] { 1 }, 0, 1);
                    _serialPort.BaseStream.Flush();

                    int remainingTime = Convert.ToInt32(b.AmountTimeCoefficient * b.Amount);

                    remainingTime -= remainingTime % 10;


                    for (; remainingTime > 0; remainingTime--)
                    {
                        if (remainingTime % 10 == 0)
                            Console.WriteLine(remainingTime / 10 + " seconds remaining");

                        System.Threading.Thread.Sleep(100);
                    }


                    _serialPort.Write(new byte[] { (byte)supplyItem.SupplySlotID }, 0, 1);
                    _serialPort.BaseStream.Flush();

                    _serialPort.Write(new byte[] { 0 }, 0, 1);
                    _serialPort.BaseStream.Flush();
                }

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