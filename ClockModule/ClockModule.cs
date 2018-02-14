using System;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System.Collections.Generic;
using Lomztein.ModularDiscordBot.Core.Configuration;
using System.Threading;
using Lomztein.ModularDiscordBot.Core.Extensions;

namespace Lomztein.ModularDiscordBot.Modules.Clock
{
    public class ClockModule : ModuleBase, IConfigurable
    {
        public override string Name => "Clock Module";
        public override string Description => "Base module for handling objects that need to update automatically.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private List<ITickable> tickables = new List<ITickable> ();
        private int tickFrequency = 1;
        private bool running;

        private SingleConfig config;
        private Thread clockThread;

        private DateTime lastTick;

        public override void Initialize() {
            ThreadStart start = new ThreadStart (Run);
            clockThread = new Thread (start);
        }

        public void Configure() {
            config = new SingleConfig (this.CompactizeName ());
            tickFrequency = config.GetEntry ("TickFrequency", tickFrequency);
            config.Save ();
        }

        public Config GetConfiguration() => config;

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
        }
    }
}
