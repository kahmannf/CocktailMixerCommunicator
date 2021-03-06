﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Model
{
    [Serializable]
    public class Recipe
    {
        public Recipe()
        {
            Ingredients = new List<Beverage>();
        }

        public List<Beverage> Ingredients { get; set; }

        public string Name { get; set; }

        public int DefaultAmountML { get; set; }

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
