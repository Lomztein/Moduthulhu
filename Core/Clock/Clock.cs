using System;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Configuration;
using System.Threading;
using Lomztein.Moduthulhu.Core.Extensions;
using Discord.WebSocket;

namespace Lomztein.Moduthulhu.Core.Clock
{
    public class Clock
    {
        private List<ITickable> Tickables { get; set; } = new List<ITickable> ();
        private Thread ClockThread { get; set; }

        private DateTime LastTick { get; set; }
        private bool IsRunning { get; set; }
        private int TickFrequency { get; set; }

        public delegate void TickEvent(DateTime currentTick, DateTime lastTick);

        public event TickEvent OnMinutePassed;
        public event TickEvent OnHourPassed;
        public event TickEvent OnDayPassed;
        public event TickEvent OnMonthPassed;
        public event TickEvent OnYearPassed;

        public Clock (int tickFrequency) {
            TickFrequency = tickFrequency;
            Start ();
        }

        public void AddTickable (ITickable tickable) {
            Tickables.Add (tickable);
        }

        public void Stop() => IsRunning = false;

        public void Start () {
            ClockThread = new Thread (new ThreadStart (Run));
            ClockThread.Start ();
        }

        private void Run () {
            int milliseconds = (int)Math.Round (1d / TickFrequency * 1000d);
            IsRunning = true;

            while (IsRunning) {
                Thread.Sleep (milliseconds);
                Tick (DateTime.Now, LastTick);
                LastTick = DateTime.Now;
            }
        }

        private void Tick (DateTime curTick, DateTime lTick) {
            Tickables.ForEach (x => x.Tick (LastTick, DateTime.Now));

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
