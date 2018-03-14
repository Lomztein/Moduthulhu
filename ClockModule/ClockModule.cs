using System;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Configuration;
using System.Threading;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Modules.Clock
{
    public class ClockModule : ModuleBase, IConfigurable<SingleConfig>
    {
        public override string Name => "Clock Module";
        public override string Description => "Base module for handling objects that need to update automatically.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private List<ITickable> tickables = new List<ITickable> ();
        private int tickFrequency = 1;
        private bool running;

        public SingleConfig Configuration { get; set; } = new SingleConfig ();
        private Thread clockThread;

        private DateTime lastTick;

        public override void Initialize() {
            ThreadStart start = new ThreadStart (Run);
            clockThread = new Thread (start);
        }

        public void Configure() {
            tickFrequency = Configuration.GetEntry ("TickFrequency", tickFrequency);
        }

        public override void Shutdown() {
            Stop ();
            foreach (ITickable tickable in tickables) {
                IModule module = tickable as IModule;
                if (module != null)
                    ParentModuleHandler.ShutdownModule (module);
            }
        }

        public void AddTickable (ITickable tickable) {
            tickables.Add (tickable);
        }

        public void Stop() => running = false;

        private void Run () {
            int milliseconds = (int)Math.Round (1f / tickFrequency) * 1000;
            running = true;

            while (running) {
                Thread.Sleep (milliseconds);
                tickables.ForEach (x => x.Tick (lastTick, DateTime.Now));
                lastTick = DateTime.Now;
            }

            clockThread.Abort ();
        }
    }
}
