using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Communication
{
    /// <summary>
    /// Respresents a REquest that can be interpreted by cmcli
    /// contains either a list of Beverages or a Recipe
    /// </summary>
    public class Request
    {
        public List<Beverage> Beverages { get; private set; }

        public int Amount { get; set; }

        public Request(Recipe r, int amountInML)
            : this(r.Ingredients, amountInML)
        {
        }

        public Request(List<Beverage> beverages, int amountInML)
        {
            Beverages = beverages;
        }

        
    }
}
