using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Communication
{
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
