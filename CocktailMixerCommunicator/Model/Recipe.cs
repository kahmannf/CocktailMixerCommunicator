using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Model
{
    [Serializable]
    public class Recipe
    {
        public List<Beverage> Ingredients { get; set; }

        public string Name { get; set; }

        public string ToParameters()
        {
            return $" order cname \"{Name}\" ";
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name) || Ingredients == null)
                return base.ToString();

            return $"Recipe: {Name}, Ingredients: {Ingredients.Count}";
        }
    }
}
