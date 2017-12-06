using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CocktailMixerCommunicator.Model
{
    /// <summary>
    /// This item contains information about the current configuration of the CocktailMixer
    /// as well as all the data about beverages and recipes
    /// </summary>
    [Serializable]
    public class CMGlobalState
    {
        public const string CMSTATE_FILENAME = "cocktail_mixer_state.cms.kaf";

        /// <summary>
        /// Creates a new instance of CMGlobalState and a file inside the directory
        /// </summary>
        /// <param name="directorypath"></param>
        /// <returns></returns>
        public static CMGlobalState CreateNew(string directorypath)
        {
            CMGlobalState result = new CMGlobalState()
            {
                Supply = new List<MixerSupplyItem>(),
                BeverageDataBase = new List<Beverage>(),
                Recipes = new List<Recipe>(),
            };

            result.ApplyChanges(directorypath);

            return result;
        }

        public static CMGlobalState LoadStateFromFile(string directorypath)
        {
            try
            {
                string filepath = Path.Combine(directorypath, CMSTATE_FILENAME);

                #region parameter validation

                if (string.IsNullOrEmpty(directorypath))
                {
                    throw new ArgumentNullException("directorypath");
                }

                if (!Directory.Exists(directorypath))
                {
                    throw new DirectoryNotFoundException($"The directory \"{directorypath}\" was not found");
                }

                if (!File.Exists(filepath))
                {
                    throw new FileNotFoundException($"The no CMState file was found insode the directory \"{directorypath}\"", filepath);
                }

                #endregion

                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    byte[] data = new byte[fs.Length];

                    fs.Read(data, 0, (int)fs.Length);

                    CMGlobalState state = (CMGlobalState)data;

                    state.SortLists();

                    return state;
                }
            }
            catch (InvalidCastException icex)
            {
                throw new Exception("File to large to read", icex);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load a CMState", ex);
            }
        }

        public List<MixerSupplyItem> Supply { get; set; }
        public List<Beverage> BeverageDataBase { get; set; }
        public List<Recipe> Recipes { get; set; }

        public byte CompressorPortId { get; set; } = 0;

        /// <summary>
        /// Supposed to be called when the state changes. 
        /// Will save the current state in the associated CMState-file
        /// </summary>
        /// <param name="directorypath">Absolute path of the directory containing the CMState-file</param>
        public void ApplyChanges(string directorypath)
        {
            try
            {
                #region paramter validation

                if (string.IsNullOrEmpty(directorypath))
                {
                    throw new ArgumentNullException("directorypath");
                }

                if (!Directory.Exists(directorypath))
                    Directory.CreateDirectory(directorypath);

                //if directory still doenst exist throw an exception (invalid path or something...)
                if (!Directory.Exists(directorypath))
                {
                    throw new Exception($"Wasnt able to create the directory specified: {directorypath}");
                }

                #endregion

                //update the recipes (in case of changes to the beverages)
                UpdateRecipeIngredients();

                using (FileStream fs = new FileStream(Path.Combine(directorypath, CMSTATE_FILENAME), FileMode.OpenOrCreate))
                {
                    byte[] data = (byte[])this;
                    fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to apply the changes of the CMState to the CMState-file", ex);
            }
        }

        private void UpdateRecipeIngredients()
        {
            foreach (Recipe recipe in Recipes)
            {
                IEnumerable<Beverage> updatedIngredients = from ingredient in recipe.Ingredients
                                                           join beverage in BeverageDataBase
                                                           on ingredient.GUID equals beverage.GUID
                                                           select new Beverage()
                                                           {
                                                               GUID = ingredient.GUID,
                                                               AlcoholVolPercentage = beverage.AlcoholVolPercentage,
                                                               RatioAmount = ingredient.RatioAmount,
                                                               Name = beverage.Name,
                                                               AmountTimeCoefficient = beverage.AmountTimeCoefficient
                                                           };

                recipe.Ingredients = new List<Beverage>(updatedIngredients);
            }
        }


        /// <summary>
        /// Serializes a CMGlobalState object using a BinaryFormatter
        /// </summary>
        /// <param name="state"></param>
        public static explicit operator byte[] (CMGlobalState state)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, state);
                return stream.GetBuffer();
            }
        }

        /// <summary>
        /// Deserializes a CMGlobalState object using a BinaryFormatter
        /// </summary>
        /// <param name="data"></param>
        public static explicit operator CMGlobalState(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream(data))
            {
                return (CMGlobalState)formatter.Deserialize(stream);
            }
        }

        public void AddBeverageToDatabase(Beverage b, string directorypath)
        {
            if (string.IsNullOrEmpty(b.GUID))
                b.GUID = Guid.NewGuid().ToString();

            BeverageDataBase.Add(b);

            ApplyChanges(directorypath);
        }

        public void SetSupplySlot(string guid_beverage, int supplySlotID, string directorypath)
        {
            if (BeverageDataBase.FirstOrDefault(x => x.GUID == guid_beverage) == null)
            {
                throw new ArgumentException($"No beverage with GUID \"{guid_beverage}\" inside the database");
            }

            if (Supply.FirstOrDefault(x => x.SupplySlotID == supplySlotID) is MixerSupplyItem item)
            {
                item.GUID_Beverage = guid_beverage;
            }
            else
            {
                Supply.Add(new MixerSupplyItem() { GUID_Beverage = guid_beverage, SupplySlotID = supplySlotID });
            }

            ApplyChanges(directorypath);
        }

        public void ClearSupplySlot(int supplySlotID, string directorypath)
        {
            if (Supply.FirstOrDefault(x => x.SupplySlotID == supplySlotID) is MixerSupplyItem item)
            {
                Supply.Remove(item);
            }

            ApplyChanges(directorypath);
        }

        public void AddRecipe(Recipe r, string directoryName)
        {
            try
            {
                #region parameter validation

                if (Recipes.FirstOrDefault(x => x.Name.ToLower() == r.Name.ToLower()) is Recipe)
                {
                    throw new InvalidOperationException($"Cannot add the Recipe for \"{r.Name}\" because a recipe with the name already exists");
                }

                //check whether all ingredients are known inside the database
                if (r.Ingredients.Any(x => BeverageDataBase.FirstOrDefault(b => b.GUID == x.GUID) == null))
                {
                    throw new InvalidOperationException("Tried to add a recipe with a unknown beverage as ingredient.");
                }

                #endregion

                Recipes.Add(r);

                ApplyChanges(directoryName);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add the Recipe.", ex);
            }
        }

        public void DeleteBeverage(string guid_beverage, string cmstateDirectory)
        {
            if (BeverageDataBase.FirstOrDefault(x => x.GUID == guid_beverage) is Beverage b)
            {
                BeverageDataBase.Remove(b);

                if (Supply.FirstOrDefault(x => x.GUID_Beverage == guid_beverage) is MixerSupplyItem supplyItem)
                {
                    Supply.Remove(supplyItem);
                }

                List<Recipe> removeRecipies = Recipes.Where(x => x.Ingredients.Any(y => y.GUID == guid_beverage)).ToList();

                foreach (Recipe recipe in removeRecipies)
                {
                    Recipes.Remove(recipe);
                }

                ApplyChanges(cmstateDirectory);
            }
            else
            {
                throw new InvalidOperationException($"No beverage with guid \"{guid_beverage}\"");
            }
        }

        public void DeleteBeverage(Beverage b, string cmstateDirectory)
        {
            DeleteBeverage(b.GUID, cmstateDirectory);
        }

        public void DeleteRecipe(string name, string cmstateDirectory)
        {
            if (Recipes.FirstOrDefault(x => x.Name == name) is Recipe r)
            {
                Recipes.Remove(r);
                ApplyChanges(cmstateDirectory);
            }
            else
            {
                throw new InvalidOperationException($"No recipe with name {name}");
            }
        }

        public void DeleteRecipe(Recipe r, string cmStateDirectory)
        {
            DeleteRecipe(r.Name, cmStateDirectory);
        }

        private void SortLists()
        {
            BeverageDataBase = new List<Beverage>(BeverageDataBase.OrderBy(x => x.Name));
            Recipes = new List<Recipe>(Recipes.OrderBy(x => x.Name));

            Supply = new List<MixerSupplyItem>(Supply.OrderBy(x => x.SupplySlotID));
        }

        public override string ToString()
        {
            if (this.Recipes == null || this.Supply == null || this.BeverageDataBase == null)
                return "Uninitialized CMState:" + base.ToString();

            return $"CMState Loaded Items: Recipies:{Recipes.Count}; Beverages: {BeverageDataBase.Count}; SupplySlots: {Supply.Count}";
        }

        public int MapSlotToPort(int slotId)
        {
            if (slotId <= 5)//slot B0 - B5
            {
                return slotId;
            }
            else
            {
                return slotId + 4; //slot d2 - d8
            }
        }
    }
}
