using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCLI
{
    public class Cliconfig
    {
        public string CMStateDirectory { get; set; }
        public string SerialPortName { get; set; }
        public int BaudRate { get; set; }
        public int DebugMode { get; set; }

        internal void Print(TextWriter output)
        {
            output.WriteLine("Current configuration setup:\n");
            output.WriteLine($"Cocktailmixer State File Directory:   {CMStateDirectory}");
            output.WriteLine($"Serial Port Name:                     {SerialPortName}");
            output.WriteLine($"Serial Port Baud-Rate:                {BaudRate}");
        }
    }
}
