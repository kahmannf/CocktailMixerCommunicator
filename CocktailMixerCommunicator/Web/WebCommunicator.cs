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

        private async void HandleRequest(HttpListenerContext listenerContext)
        {
            await Task.Run(() =>
            {
                if (_mixing)
                {
                    listenerContext.Response.StatusCode = 423; //HttpStatus 423 = LOCKED
                    listenerContext.Response.OutputStream.Write("Already mixing!".Select(x => (byte)x).ToArray(), 0, 15);
                    listenerContext.Response.OutputStream.Close();
                }
                else
                {
                    _mixing = true;

                    System.Collections.Specialized.NameValueCollection queryParameter = listenerContext.Request.QueryString;

                    CMGlobalState state = CMGlobalState.LoadStateFromFile(_config.CMStateDirectory);

                    if ((queryParameter.GetKey(0) == "cocktail" && !string.IsNullOrEmpty(queryParameter[0]))
                        && (state.Recipes.FirstOrDefault(x => x.Name.ToUpper(CultureInfo.InvariantCulture) == queryParameter[0].ToUpper(CultureInfo.InvariantCulture)) is Recipe cocktail))
                    {
                        if (state.GetMissingIngredients(cocktail).Count() == 0)
                        {
                            listenerContext.Response.StatusCode = 202; //HttpStatus 202 = Created
                            listenerContext.Response.OutputStream.Write("Created".Select(x => (byte)x).ToArray(), 0, 7);
                            listenerContext.Response.OutputStream.Close();

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
                            listenerContext.Response.StatusCode = 410; //HttpStatus 410 = Gone
                            listenerContext.Response.OutputStream.Write("Gone".Select(x => (byte)x).ToArray(), 0, 4);
                            listenerContext.Response.OutputStream.Close();
                        }
                    }
                    else
                    {
                        listenerContext.Response.StatusCode = 400; //HttpStatus 400 = BadRequest
                        listenerContext.Response.OutputStream.Write("Bad Request".Select(x => (byte)x).ToArray(), 0, 11);
                        listenerContext.Response.OutputStream.Close();
                    }

                    _mixing = false;
                }
            });
        }
    }
}
