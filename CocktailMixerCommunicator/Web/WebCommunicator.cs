using CocktailMixerCommunicator.Communication;
using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CocktailMixerCommunicator.Web
{
    public class WebCommunicator
    {
        public event EventHandler<Recipe> MixCocktailRequested;

        public WebCommunicator(Configuration config)
        {
            _config = config;
        }

        private Configuration _config; 

        public void StartListening()
        {
            if (_running)
                throw new InvalidOperationException("Already running");

            _running = true;


            ThreadStart start = new ThreadStart(RunInternal);

            _runningThread = new Thread(start);

            _runningThread.Start();
        }

        public void StopListening()
        {
            _runningThread.Abort();
            _runningThread = null;
            _running = false;
        }

        private bool _running = false;

        public bool Running => _running;

        private Thread _runningThread;

        private void RunInternal()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");

            listener.Start();

            while (true)
            {
                HandleRequest(listener.GetContext());
            }
        }

        private volatile bool _mixing = false;

        private void EndRequest(HttpListenerContext listenerContext, int status, string message)
        {
            listenerContext.Response.StatusCode = status;
            listenerContext.Response.OutputStream.Write(message.Select(x => (byte)x).ToArray(), 0, message.Length);
            listenerContext.Response.OutputStream.Close();
        }
        
        
        private async void HandleRequest(HttpListenerContext listenerContext)
        {
            await Task.Run(() =>
            {
                System.Collections.Specialized.NameValueCollection queryParameter = listenerContext.Request.QueryString;
                
                if(!string.IsNullOrEmpty(queryParameter["type"]))
                {
                    switch(queryParameter["type"])
                    {
                        case "mix":
                            MixCocktail(listenerContext);
                            break;
                        case "menu":
                            GetMenu(listenerContext);
                            break;
                        default:
                            EndRequest(listenerContext, 400, "Invalid type provided");
                            break;
                    }
                }
                else
                {
                    listenerContext.Response.StatusCode = 400; //HttpStatus 400 = BadRequest
                    listenerContext.Response.OutputStream.Write("No type provided".Select(x => (byte)x).ToArray(), 0, 16);
                    listenerContext.Response.OutputStream.Close();
                }
                
            });
        }
        
        private void MixCocktail(HttpListenerContext listenerContext)
        {
            if (_mixing)
                {
                    EndRequest(listenerContext, 423, "Already mixing!");
                }
                else
                {
                    _mixing = true;

                    System.Collections.Specialized.NameValueCollection queryParameter = listenerContext.Request.QueryString;

                    CMGlobalState state = CMGlobalState.LoadStateFromFile(_config.CMStateDirectory);

                    if ((!string.IsNullOrEmpty(queryParameter["cocktail"]))
                        && (state.Recipes.FirstOrDefault(x => x.Name.ToUpper(CultureInfo.InvariantCulture) == queryParameter[0].ToUpper(CultureInfo.InvariantCulture)) is Recipe cocktail))
                    {
                        if (state.GetMissingIngredients(cocktail).Count() == 0)
                        {
                            EndRequest(listenerContext, 202, "Created");

                            try
                            {
                                MixCocktailRequested?.Invoke(this, cocktail);
                            }
                            catch (Exception ex)
                            {
                                
                            }

                            //List<Beverage> remainingIngredients = new List<Beverage>(cocktail.Ingredients);
                            //List<Beverage> pouringIngredients = new List<Beverage>();
                            //List<Beverage> servedIngredients = new List<Beverage>();

                            //SerialCommunicator com = new SerialCommunicator(_config.COMPort, _config.BaudRate);

                            //int totalParts = remainingIngredients.Select(x => x.RatioAmount).Aggregate((x, y) => x + y);

                            //bool first = true;

                            //while (remainingIngredients.Count > 0)
                            //{


                            //    Beverage b = remainingIngredients.First();
                            //    remainingIngredients.Remove(b);
                            //    pouringIngredients.Add(b);

                            //    int amount = (int)((double)cocktail.DefaultAmountML * b.RatioAmount / (double)totalParts);

                            //    com.SendRequest(b, amount, state, _config.CMStateDirectory, first, remainingIngredients.Count == 0);

                            //    if (first)
                            //        first = false;

                            //    pouringIngredients.Remove(b);

                            //    servedIngredients.Add(b);
                            //}
                        }
                        else
                        {
                            EndRequest(listenerContext, 410, "Gone");
                        }
                    }
                    else
                    {
                        EndRequest(listenerContext, 400, "No cocktail found");
                    }

                    _mixing = false;
                }
        }
        
        private void GetMenu(HttpListenerContext listenerContext)
        {
            CMGlobalState state = CMGlobalState.LoadStateFromFile(_config.CMStateDirectory);
            Recipe[] cocktails = state.Recipes.ToArray();
            
            string message = JsonConvert.SerializeObject(cocktails);
            
            EndRequest(listenerContext, 200, message);
        }
    }
}
