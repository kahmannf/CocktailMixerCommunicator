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

        public Recipe Recipe { get; private set; }

        public Request(Recipe r)
        {
            Recipe = r;
        }

        public Request(List<Beverage> beverages)
        {
            Beverages = beverages;
        }

        /// <summary>
        /// Converts the Request into cmcli commandline parameters as string
        /// </summary>
        /// <returns></returns>
        public string ToParameter()
        {
            if (Beverages == null || Beverages.Count == 0)
            {
                return Recipe.ToParameters();
            }
            else
            {
                return Beverages.ToParameters();
            }
        }
    }
}
