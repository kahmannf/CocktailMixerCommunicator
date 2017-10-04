using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator
{
    public static class Extensions
    {
        public static string ToParameters(this List<Beverage> list)
        {

            string result = " order ";

            for (int i = 0; i < list.Count; i++)
            {
                result += " b" + i;

                result += " " + list[i].GUID;

                result += " " + list[i].Amount;
            }

            return result;
        }
    }
}
