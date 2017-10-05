using CocktailMixerCommunicator.Communication;
using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CocktailMixerCLI
{
    /********************************************************************  DISCLAIMER  ****************************************************************************/
    /* 
            I know this file/cli is a lot of mess, but it was a quick implementaion to test the basic communication
            
            i probably will forget about cleaning this up and find this fragment in a few years..

            but hey it work :)
    */
    public class CMCLI
    {
        public const int SLOT_COUNT = 11;
        public const string CONFIG_FILE_NAME = "cliconfig.xml";

        /// <summary>
        /// Possible arguments:
        /// 
        /// /order : place a order
        /// 
        /// /config : change or output config.
        /// 
        /// /cmstate : get or modify information about the CocktailMixer
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Result code: -1 exception occured, anything else is the returncode from the serialport</returns>
        public static int Main(string[] args)
        {
            int rv = 0;
            try
            {
                if (args.Length < 1)
                {
                    throw new ArgumentException("No arguments provided");
                }

                LoadOrCreateConfig();

                rv = ProcessArguments(args);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    if (Configuration?.DebugMode == 1)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    else
                    {
                        Console.WriteLine(ex.GetType().Name);
                        Console.WriteLine(ex.Message);
                    }

                    ex = ex.InnerException;
                }
                rv = -1;
            }
            Console.WriteLine("Exiting with code " + rv);
            return rv;
        }

        public static Cliconfig Configuration;
        public static string ConfigFileName;

        public static void LoadOrCreateConfig()
        {
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ConfigFileName = Path.Combine(currentDir, CONFIG_FILE_NAME);

            if (!File.Exists(ConfigFileName))
            {
                Configuration = new Cliconfig();
                SaveConfig();
            }
            else
            {
                using (FileStream fs = new FileStream(ConfigFileName, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Cliconfig));
                    Configuration = (Cliconfig)serializer.Deserialize(fs);
                }
            }
        }

        public static int ProcessArguments(string[] args)
        {
            int rv = 0;
            switch (args[0].ToLower().TrimStart('/', '-'))
            {
                case "config":
                    rv = ChangeConfig(args);
                    break;
                case "order":
                    rv = TakeOrder(args);
                    break;
                case "cmstate":
                    rv = HandleCMState(args);
                    break;
                case "reset":
                    rv = Reset();
                    break;
                default:
                    throw new ArgumentException($"Unknown parameter: \"{args[0]}\"");
            }
            return rv;
        }

        public static int Reset()
        {
            try
            {
                SerialCommunicator com = new SerialCommunicator(Configuration.SerialPortName, Configuration.BaudRate);

                return com.ResetSlots(SLOT_COUNT);                
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public static void SaveConfig()
        {
            using (FileStream fs = new FileStream(ConfigFileName, FileMode.OpenOrCreate))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Cliconfig));
                serializer.Serialize(fs, Configuration);
            }
        }

        public static string GetArgument(string[] args, string argument, bool required)
        {
            int index = Array.IndexOf(args, argument);

            if (index == -1)
                index = Array.IndexOf(args, "/" + argument);

            if (index == -1)
                index = Array.IndexOf(args, "-" + argument);

            if (index == -1)
            {
                if (required)
                {
                    throw new ArgumentException($"Argument \"{argument} *value*\" missing");
                }
                else
                {
                    return string.Empty;
                }
            }

            index = index + 1;

            if (args.Length <= (index))
            {
                throw new ArgumentException($"Missing value for parameter {argument}");
            }

            return args[index];
        }
        
        public static int TakeOrder(string[] args)
        {
            string cocktail = GetArgument(args, "cname", false);

            string firstBeverage = GetArgument(args, "b0", false);

            CMState = CMGlobalState.LoadStateFromFile(Configuration.CMStateDirectory);

            List<Beverage> order = new List<Beverage>();

            if (!string.IsNullOrEmpty(cocktail))
            {
                if (CMState.Recipes.FirstOrDefault(x => x.Name.ToLower() == cocktail.ToLower()) is Recipe r)
                {
                    order.AddRange(r.Ingredients);
                }
                else
                {
                    throw new ArgumentException($"Unknown cocktail-name: \"{cocktail}\"");
                }
            }
            else if (!string.IsNullOrEmpty(firstBeverage))
            {
                for (int i = 0; ; i++)
                {
                    string guid_beverage = GetArgument(args, "b" + i, false);

                    if (string.IsNullOrEmpty(guid_beverage))
                    {
                        break;
                    }

                    if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == guid_beverage) is Beverage b)
                    {
                        int index_amount = Array.IndexOf(args, guid_beverage) + 1;

                        b.Amount = Double.Parse(args[index_amount]);

                        order.Add(b);
                    }
                    else
                    {
                        throw new ArgumentException("Unknown Beverage GUID provided: \"{guid_beverage}\"");
                    }
                }
            }
            else
            {
                throw new ArgumentException("Missing Argument: either \"/cname *value*\" or at leaat one beverage \"/b0 *value*\" msut be provided.");
            }

            Console.WriteLine("Creating a drink with the following ingredients:");

            foreach (Beverage b in order)
            {
                Console.WriteLine($"{b.Name.PadRight(40)}  {b.Amount} ml");
            }

            SerialCommunicator communicator = new SerialCommunicator(Configuration.SerialPortName, Configuration.BaudRate);

            int result = communicator.SendRequest(order, CMState);

            Console.WriteLine("Transmisson successful");

            return result;
        }

        public static int ChangeConfig(string[] args)
        {
            if (args.Length < 2)
            {
                args = new string[] { "/config", "/show" };
            }

            switch (args[1].ToLower().TrimStart('/', '-'))
            {
                case "set":
                    return SetConfigProperty(args);
                default:
                    Configuration.Print(Console.Out);
                    return 0;
            }
        }

        public static int SetConfigProperty(string[] args)
        {
            if (args.Length < 4)
            {
                throw new ArgumentException("Not enough Arguments provided for /config /set");
            }
            else
            {
                switch (args[2].ToLower().TrimStart('/', '-'))
                {
                    case "cmstatedir":
                    case "cmsd":
                    case "statedir":
                        Configuration.CMStateDirectory = args[3];
                        break;
                    case "comport":
                    case "port":
                    case "serialport":
                        Configuration.SerialPortName = args[3];
                        break;
                    case "baud":
                    case "br":
                    case "baudrate":
                        Configuration.BaudRate = Int32.Parse(args[3]);
                        break;
                    case "debug":
                    case "debugmode":
                        Configuration.DebugMode = Int32.Parse(args[3]);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown config-property: \"{args[2]}\"");
                }
                SaveConfig();
                return 0;
            }
        }

        /// <summary>
        /// Manipulate CMStatefile
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int HandleCMState(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Not enough arguments provided for \"/cmstate\"");
            
            CMState = CMGlobalState.LoadStateFromFile(Configuration.CMStateDirectory);

            switch (args[1].ToLower().TrimStart('/', '-'))
            {
                case "beverage":
                    return HandleBeverage(args);
                case "create":
                    if (string.IsNullOrEmpty(Configuration.CMStateDirectory))
                        throw new InvalidOperationException("Must add a CMstatedirectory to the config befor creating a new CMState file");

                    CMGlobalState.CreateNew(Configuration.CMStateDirectory);
                    return 0;
                case "recipe":
                    return HandleRecipe(args);
                case "supply":
                    return HandleSupply(args);
                case "print":
                    return HandleCMStatePrint(args);
                default:
                    throw new InvalidOperationException($"Unknown Argument for /cmstate: \"{args[1]}\"");
            }
            
        }

        public static CMGlobalState CMState;

        public static int HandleBeverage(string[] args)
        {
            switch (args[2].TrimStart('/', '-'))
            {
                case "add": //required args: /name (string), /amounttime (double)
                            //optional args: /alcvol (double)
                    return AddBeverage(args);
                case "remove":
                case "delete":
                    string guid = GetArgument(args, "guid", true);

                    return DeleteBeverage(guid);
                default:
                    throw new ArgumentException($"Unknown argument for /cmstate /beverage : \"{args[2]}\"");
            }
        }

        public static int AddBeverage(string[] args)
        {
            Beverage b = new Beverage()
            {
                GUID = Guid.NewGuid().ToString(),
                Name = GetArgument(args, "name", true),
                AmountTimeCoefficient = Double.Parse(GetArgument(args, "amounttime", true)),
                Amount = 0.0
            };

            string alcvol = GetArgument(args, "alcvol", false);

            if (Double.TryParse(alcvol, out double d))
            {
                b.AlcoholVolPercentage = d;
            }

            CMState.AddBeverageToDatabase(b, Configuration.CMStateDirectory);

            PrintBeverage(b);

            return 0;
        }

        public static int DeleteBeverage(string guid)
        {
            if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == guid) is Beverage b)
            {
                CMState.DeleteBeverage(b, Configuration.CMStateDirectory);
                return 0;
            }
            else
            {
                throw new ArgumentException($"No beverage found for guid \"{guid}\"");
            }
        }

        public static int HandleRecipe(string[] args)
        {
            if (args.Length < 3)
            {
                throw new ArgumentException("Not enough arguments provided for /cmstate /recipe");
            }

            switch (args[2].ToLower().TrimStart('/', '-'))
            {
                case "add":
                    return AddRecipe(args);
                case "remove":
                case "delete":
                    return DeleteRecipe(args);
                case "modify":
                case "change":
                    return ModifyRecipe(args);
                default:
                    throw new ArgumentException($"Unknown paramter \"{args[2]}\" for /cmstate / recipe");
            }
        }

        public static int AddRecipe(string[] args)
        {
            Recipe result = new Recipe()
            {
                Name = GetArgument(args, "name", true),

                Ingredients = new List<Beverage>()
            };

            for (int i = 0; ; i++)
            {
                string guid_beverage = GetArgument(args, "b" + i, false);

                if (string.IsNullOrEmpty(guid_beverage))
                {
                    break;
                }

                if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == guid_beverage) is Beverage b)
                {
                    int index_amount = Array.IndexOf(args, guid_beverage) + 1;

                    b.Amount = Double.Parse(args[index_amount]);

                    result.Ingredients.Add(b);
                }
                else
                {
                    throw new ArgumentException("Unknown Beverage GUID provided: \"{guid_beverage}\"");
                }
            }

            if (result.Ingredients.Count == 0)
            {
                throw new ArgumentException("There has to be aat least one beverage in a Recipe");
            }

            CMState.AddRecipe(result, Configuration.CMStateDirectory);

            PrintRecipe(result);

            return 0;
        }

        public static int DeleteRecipe(string[] args)
        {
            if (args.Length < 4)
            {
                throw new ArgumentException("Not enough arguments provided for /cmstate /recipe /delete");
            }

            if (CMState.Recipes.FirstOrDefault(x => x.Name.ToLower() == args[3].ToLower()) is Recipe r)
            {
                CMState.DeleteRecipe(r, Configuration.CMStateDirectory);
                return 0;
            }
            else
            {
                throw new ArgumentException($"No recipe with name {args[3]}");
            }
        }

        public static int ModifyRecipe(string[] args)
        {
            if (args.Length < 4)
            {
                throw new ArgumentException("Not enough Arguments provided for /cmstate /recipe /modify");
            }

            switch (args[3].ToLower().TrimStart('/', '-'))
            {
                case "name":
                    string oldName = GetArgument(args, "old", true);
                    if (CMState.Recipes.FirstOrDefault(x => x.Name.ToLower() == oldName) is Recipe r)
                    {
                        r.Name = GetArgument(args, "new", true);
                        CMState.ApplyChanges(Configuration.CMStateDirectory);
                    }
                    else
                    {
                        throw new ArgumentException($"No Beverage with name \"{oldName}\"");
                    }
                    return 0;
                case "add":
                    string recipeName = GetArgument(args, "recipename", true);
                    string guid_newBeverage = GetArgument(args, "guid", true);
                    double amount = Double.Parse(GetArgument(args, "amount", true));

                    if (CMState.Recipes.FirstOrDefault(x => x.Name.ToLower() == recipeName.ToLower()) is Recipe modifyRecipe)
                    {
                        if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == guid_newBeverage) is Beverage b)
                        {
                            b.Amount = amount;
                            modifyRecipe.Ingredients.Add(b);
                            CMState.ApplyChanges(Configuration.CMStateDirectory);
                            return 0;
                        }
                        else
                        {
                            throw new ArgumentException($"No beverage with guid {guid_newBeverage}");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"No Recipe with name {recipeName}");
                    }
                case "remove":
                    string removeBevRecipeName = GetArgument(args, "recipename", true);
                    string guid_removeBeverage = GetArgument(args, "guid", true);

                    if (CMState.Recipes.FirstOrDefault(x => x.Name.ToLower() == removeBevRecipeName.ToLower()) is Recipe modifyRemoveRecipe)
                    {
                        if (modifyRemoveRecipe.Ingredients.FirstOrDefault(x => x.GUID == guid_removeBeverage) is Beverage removeBeverage)
                        {
                            modifyRemoveRecipe.Ingredients.Remove(removeBeverage);
                            CMState.ApplyChanges(Configuration.CMStateDirectory);
                            return 0;
                        }
                        else
                        {
                            throw new ArgumentException($"No beverage with guid {guid_removeBeverage} in the recipe");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"No Recipe with name {removeBevRecipeName}");
                    }
                default:
                    throw new ArgumentException($"Unknown parameter for /cmstate /recipe /modify: {args[3]}");
            }
        }

        public static int HandleSupply(string[] args)
        {
            if (args.Length < 3)
            {
                throw new ArgumentException("Not enough arguments provided for /cmstate /supply");
            }

            switch (args[2])
            {
                case "setslot":
                    int setslot = Int32.Parse(GetArgument(args, "slot", true));
                    string guidSetSlot = GetArgument(args, "guid", true);

                    if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == guidSetSlot) is Beverage b)
                    {
                        CMState.SetSupplySlot(guidSetSlot, setslot, Configuration.CMStateDirectory);
                        Console.WriteLine($"Slot {setslot} contains now:");

                        PrintBeverage(b);
                        return 0;
                    }
                    else
                    {
                        throw new ArgumentException($"No beverage with guid {guidSetSlot}");
                    }

                case "clearslot":
                    int clearslot = Int32.Parse(GetArgument(args, "slot", true));

                    CMState.ClearSupplySlot(clearslot, Configuration.CMStateDirectory);

                    return 0;
                case "clearall":
                    foreach (MixerSupplyItem item in new List<MixerSupplyItem>(CMState.Supply))
                    {
                        CMState.Supply.Remove(item);
                    }

                    CMState.ApplyChanges(Configuration.CMStateDirectory);
                    return 0;
                default:
                    throw new ArgumentException($"Unknown argument for /cmstate /supply : {args[2]}");
            }
        }

        public static int HandleCMStatePrint(string[] args)
        {
            if (args.Length == 2)
                args = new string[] { "", "", "all" };

            switch (args[2].ToLower())
            {
                case "beverage":
                    string beverageName = GetArgument(args, "name", false);
                    string beverageGuid = GetArgument(args, "guid", false);

                    if (string.IsNullOrEmpty(beverageGuid) && string.IsNullOrEmpty(beverageName))
                    {
                        throw new ArgumentException("Parameter \"name\" or \"guid\" required for /cmstate /print /beverage");
                    }
                    else if (!string.IsNullOrEmpty(beverageGuid) && !string.IsNullOrEmpty(beverageName))
                    {
                        throw new ArgumentException("Use either \"name\" or \"guid\" for /cmstate /print /beverage, not both");
                    }
                    else if (!string.IsNullOrEmpty(beverageName))
                    {
                        if (CMState.BeverageDataBase.FirstOrDefault(x => x.Name.ToLower() == beverageName.ToLower()) is Beverage b)
                        {
                            PrintBeverage(b);
                        }
                        else
                        {
                            throw new ArgumentException($"No beverage with name \"{beverageName}\"");
                        }
                    }
                    else //guid is not null/empty
                    {
                        if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == beverageGuid) is Beverage b)
                        {
                            PrintBeverage(b);
                        }
                        else
                        {
                            throw new ArgumentException($"No beverage with GUID \"{beverageGuid}\"");
                        }
                    }

                    return 0;
                case "beverages":
                    if (CMState.BeverageDataBase.Count == 0)
                    {
                        Console.WriteLine("No Beverages in Database");
                        return 0;
                    }

                    foreach (Beverage b in CMState.BeverageDataBase)
                    {
                        PrintBeverage(b);
                    }
                    return 0;
                case "recipe":
                    string recipeName = GetArgument(args, "name", false);

                    if (string.IsNullOrEmpty(recipeName))
                    {
                        throw new ArgumentException("Parameter \"name\" required for /cmstate /print /recipe");
                    }
                    else
                    {
                        if (CMState.Recipes.FirstOrDefault(x => x.Name.ToLower() == recipeName.ToLower()) is Recipe r)
                        {
                            PrintRecipe(r);
                        }
                        else
                        {
                            throw new ArgumentException($"No recipe with name \"{recipeName}\"");
                        }
                    }

                    return 0;

                case "recipies":
                    if (CMState.Recipes.Count == 0)
                    {
                        Console.WriteLine("No Recipes in Database");
                        return 0;
                    }

                    foreach (Recipe r in CMState.Recipes)
                    {
                        PrintRecipe(r);
                    }
                    return 0;
                case "supply":
                    int slot = Int32.Parse(GetArgument(args, "slot", true));
                    if (CMState.Supply.FirstOrDefault(x => x.SupplySlotID == slot) is MixerSupplyItem item)
                    {
                        PrintSupply(item);
                    }
                    else
                    {
                        Console.WriteLine($"No Configuration for supply-slot {slot}");
                    }
                    return 0;
                case "supplies":
                    if(CMState.Supply.Count == 0)
                    {
                        Console.WriteLine("No Supply-Slots configured");
                        return 0;
                    }

                    foreach (MixerSupplyItem supplyItem in CMState.Supply)
                    {
                        PrintSupply(supplyItem);
                    }
                    return 0;
                case "all":
                default:
                    HandleCMStatePrint(new string[] { "", "", "beverages" });
                    HandleCMStatePrint(new string[] { "", "", "recipies" });
                    HandleCMStatePrint(new string[] { "", "", "supplies" });
                    return 0;
            }
        }

        public static void PrintBeverage(Beverage b)
        {
            Console.WriteLine($"Beverage {b.Name}:");

            Console.WriteLine($"GUID                        {b.GUID}");
            Console.WriteLine($"Alcohol Volume Percentage   {b.AlcoholVolPercentage}");
            Console.WriteLine($"Amout Time Coefficient      {b.AmountTimeCoefficient}");

            Console.WriteLine();
        }

        public static void PrintRecipe(Recipe r)
        {
            Console.WriteLine($"Recipe {r.Name}:\n");

            Console.WriteLine("Ingredients: ");

            foreach (Beverage b in r.Ingredients)
            {
                Console.WriteLine(b.Name.PadRight(40) + b.Amount + " ml");
            }

            Console.WriteLine();
        }

        public static void PrintSupply(MixerSupplyItem si)
        {
            if (CMState.BeverageDataBase.FirstOrDefault(x => x.GUID == si.GUID_Beverage) is Beverage b)
            {
                Console.WriteLine($"SupplyItem in Slot {si.SupplySlotID}");
                Console.WriteLine($"Name {b.Name.PadRight(40)} GUID:{b.GUID}");
            }
            else
            {
                Console.WriteLine($"WARNIG: Slot {si.SupplySlotID} contains a unknown guid: \"{si.GUID_Beverage}\"");
            }
        }
    }
}
