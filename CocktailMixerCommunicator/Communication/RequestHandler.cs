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

        public void Start(string cmcliLocation)
        {
            if (_run)
                throw new InvalidOperationException("RequestHandler already running");

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
        public void POST(Request request)
        {
            _queue.Push(request);
        }

        private bool _run = false;
        /// <summary>
        /// path to the cmcli.exe
        /// </summary>
        private string _cmcliLocation;

        private void RunInternal()
        {
            Task.Run(() =>
            {
                while (_run)
                {
                    if (_queue.HasItems)
                    {
                        Request request = _queue.Pull();

                        Process cmcli = new Process();

                        ProcessStartInfo startinfo = new ProcessStartInfo(_cmcliLocation, request.ToParameter());

                        cmcli.StartInfo = startinfo;

                        cmcli.Start();

                        cmcli.WaitForExit();

                        int resetcount = 0;

                        while (cmcli.ExitCode != 0 && resetcount < 3)
                        {
                            cmcli.StartInfo.Arguments = " reset";

                            resetcount++;

                            cmcli.Start();

                            cmcli.WaitForExit();
                        }

                        if (resetcount == 3 && cmcli.ExitCode != 0)
                        {
                            throw new Exception("Failed to reset arduino after failed execution");
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
