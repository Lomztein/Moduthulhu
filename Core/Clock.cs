using System;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System.Collections.Generic;
using System.Threading;
using Lomztein.Moduthulhu.Core.Extensions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core
{
    public class Clock
    {
        private readonly string _name;
        private readonly int _tickFrequency;
        private Thread _clockThread;

        private DateTime _lastTick;
        private bool _isRunning;

        public delegate Task TickEvent(DateTime currentTick, DateTime lastTick);

        public event TickEvent OnSecondPassed;
        public event TickEvent OnMinutePassed;
        public event TickEvent OnHourPassed;
        public event TickEvent OnDayPassed;
        public event TickEvent OnMonthPassed;
        public event TickEvent OnYearPassed;

        public event Func<Exception, Task> ExceptionCaught;

        public Clock (int tickFrequency, string name) {
            _tickFrequency = tickFrequency;
            _name = name;
        }

        public void Stop() => _isRunning = false;

        public void Start () {
            _clockThread = new Thread(new ThreadStart(Run)) {
                Name = _name
            };
            _clockThread.Start ();
        }

        private void Run () {
            int milliseconds = (int)Math.Round (1d / _tickFrequency * 1000d);
            _lastTick = DateTime.Now;
            _isRunning = true;

            while (_isRunning) {
                Thread.Sleep (milliseconds);
                Tick (DateTime.Now, _lastTick);
                _lastTick = DateTime.Now;
            }

            _clockThread.Abort();
        }

        private void Tick (DateTime curTick, DateTime lTick) {
            try
            {
                OnSecondPassed?.Invoke(curTick, lTick);
                if (MinutePassed(curTick, lTick))
                {
                    OnMinutePassed?.Invoke(curTick, lTick);
                }
                if (HourPassed(curTick, lTick))
                {
                    OnHourPassed?.Invoke(curTick, lTick);
                }
                if (DayPassed(curTick, lTick))
                {
                    OnDayPassed?.Invoke(curTick, lTick);
                }
                if (MonthPassed(curTick, lTick))
                {
                    OnMonthPassed?.Invoke(curTick, lTick);
                }
                if (YearPassed(curTick, lTick))
                {
                    OnYearPassed?.Invoke(curTick, lTick);
                }
            }
            catch (Exception exc)
            {
                ExceptionCaught?.Invoke(exc);
            }
        }

        // Just for good measure.
        public static bool YearPassed(DateTime lastTick, DateTime currentTick) => lastTick.Year != currentTick.Year;
        public static bool MonthPassed(DateTime lastTick, DateTime currentTick) => lastTick.Month != currentTick.Month;
        public static bool DayPassed(DateTime lastTick, DateTime currentTick) => lastTick.Day != currentTick.Day;
        public static bool HourPassed(DateTime lastTick, DateTime currentTick) => lastTick.Hour != currentTick.Hour;
        public static bool MinutePassed(DateTime lastTick, DateTime currentTick) => lastTick.Minute != currentTick.Minute;
    }
}
