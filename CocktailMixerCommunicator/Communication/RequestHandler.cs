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

        public void Stop()
        {
            _run = false;
        }

        public void POST(Request request)
        {
            _queue.Push(request);
        }

        private bool _run = false;
        private string _cmcliLocation;
        private bool blocked;

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
