using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Model
{
    [Serializable]
    public class Beverage
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public double AlcoholVolPercentage { get; set; }

        /// <summary>
        /// Defines how long a valve has to be open for a certain amount of fluid to pass
        /// Calculation: Amount (in milliliters) * AmountTimeCoefficient (in 1/10 seconds per milliliter) = ValveOpenTime (in 1/10 sec)
        /// </summary>
        public double AmountTimeCoefficient { get; set; }

        public double Amount { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return base.ToString();

            return $"Beverage: {Name}; Amount={Amount}";
        }
    }
}
