using CocktailMixerCommunicator.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator
{
    public static class DataExport
    {
        public static void ExportCmGlobalState(string filepath, CMGlobalState state)
        {
            try
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    StreamWriter writer = new StreamWriter(stream);

                    string[] jsonlines = JsonConvert.SerializeObject(state).Split('\n');

                    foreach(string line in jsonlines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
