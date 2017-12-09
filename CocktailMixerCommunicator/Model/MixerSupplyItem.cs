using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Model
{
    [Serializable]
    public class MixerSupplyItem
    {
        public string GUID_Beverage { get; set; }

        /// <summary>
        /// ID of slot which contains the bottle
        /// </summary>
        public int SupplySlotID { get; set; }

        public int AmountMLLeft { get; set; }

        public override string ToString()
        {
            return $"SlotID: {SupplySlotID}";
        }
    }
}
