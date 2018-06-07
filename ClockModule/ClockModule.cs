using System;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Configuration;
using System.Threading;
using Lomztein.Moduthulhu.Core.Extensions;
using Discord.WebSocket;

namespace Lomztein.Moduthulhu.Modules.Clock
{
    public class ClockModule : ModuleBase, IConfigurable<SingleConfig>
    {
        public override string Name => "Clock Module";
        public override string Description => "Base module for handling objects that need to update automatically.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private List<ITickable> tickables = new List<ITickable> ();
        [AutoConfig] private SingleEntry<int, SocketGuild> tickFrequency = new SingleEntry<int, SocketGuild> (x => 1, "TickFrequency", false);
        private bool running;

        public SingleConfig Configuration { get; set; } = new SingleConfig ();
        private Thread clockThread;

        private DateTime lastTick;

        public delegate void TickEvent(DateTime currentTick, DateTime lastTick);

        public event TickEvent OnMinutePassed;
        public event TickEvent OnHourPassed;
        public event TickEvent OnDayPassed;
        public event TickEvent OnMonthPassed;
        public event TickEvent OnYearPassed;

        public override void Initialize() {
            clockThread = new Thread (Run);
            clockThread.Start ();
        }

        public override void Shutdown() {
            Stop ();
            foreach (ITickable tickable in tickables) {
                if (tickable is IModule module)
                    ParentModuleHandler.ShutdownModule (module);
            }
        }

        public void AddTickable (ITickable tickable) {
            tickables.Add (tickable);
        }

        public void Stop() => running = false;

        private void Run () {
            int milliseconds = (int)Math.Round (1f / tickFrequency.GetValue ()) * 1000;
            running = true;

            while (running) {
                Thread.Sleep (milliseconds);
                Tick (DateTime.Now, lastTick);
                lastTick = DateTime.Now;
            }
        }

        private void Tick (DateTime curTick, DateTime lTick) {
            tickables.ForEach (x => x.Tick (lastTick, DateTime.Now));

            if (MinutePassed (curTick, lTick))
                OnMinutePassed?.Invoke (curTick, lTick);
            if (HourPassed (curTick, lTick))
                OnHourPassed?.Invoke (curTick, lTick);
            if (DayPassed (curTick, lTick))
                OnDayPassed?.Invoke (curTick, lTick);
            if (MonthPassed (curTick, lTick))
                OnMonthPassed?.Invoke (curTick, lTick);
            if (YearPassed (curTick, lTick))
                OnYearPassed?.Invoke (curTick, lTick);
        }

        // Just for good measure.
        public static bool YearPassed(DateTime lastTick, DateTime currentTick) => lastTick.Year != currentTick.Year;
        public static bool MonthPassed(DateTime lastTick, DateTime currentTick) => lastTick.Month != currentTick.Month;
        public static bool DayPassed(DateTime lastTick, DateTime currentTick) => lastTick.Day != currentTick.Day;
        public static bool HourPassed(DateTime lastTick, DateTime currentTick) => lastTick.Hour != currentTick.Hour;
        public static bool MinutePassed(DateTime lastTick, DateTime currentTick) => lastTick.Minute != currentTick.Minute;
    }
}
