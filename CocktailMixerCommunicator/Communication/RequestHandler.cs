using CocktailMixerCommunicator.Model;
using RandomUtilities.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocktailMixerCommunicator.Communication
{
    /// <summary>
    /// Provides a queue to queue up request. 
    /// These will be send to cmcli
    /// </summary>
    public class RequestHandler
    {
        FILO<Request> _queue = new FILO<Request>();

        SerialCommunicator serialComm;

        public void Start(string portname, int baudRate)
        {
            if (_run)
                throw new InvalidOperationException("RequestHandler already running");

            serialComm = new SerialCommunicator(portname, baudRate);
            _run = true;
            RunInternal();
        }

        /// <summary>
        /// Stops the RequestHandler after the current execution.
        /// No further items will be pulled from the queue, but the current request will be complete
        /// </summary>
        public void Stop()
        {
            _run = false;
        }

        /// <summary>
        /// Add a REquest to the queue
        /// </summary>
        /// <param name="request"></param>
        public void POST(Request request, CMGlobalState currentState)
        {
            State = currentState;
            _queue.Push(request);
        }

        private bool _run = false;

        public CMGlobalState State { get; set; }

        private void RunInternal()
        {
            Task.Run(() =>
            {

                while (_run)
                {
                    if (_queue.HasItems)
                    {
                        Request request = _queue.Pull();

                        int totalParts = request.Beverages.Select(z => z.RatioAmount).Aggregate((x, y) => x+ y);

                        foreach (Beverage b in request.Beverages)
                        {
                            serialComm.SendRequest(b, (int)(((double)request.Amount) * (((double)b.RatioAmount) / ((double)totalParts))), State);
                        }

                        //Wait 5 seconds befor serving another drink
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
            });
        }
    }
}
