using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public class TimedAction
    {
        private readonly Action _action;
        private readonly Clock _clock;
        private int _timeInSeconds;

        public event Action<Exception> OnExceptionOccured;

        public TimedAction (Action action, int timeInSeconds, Clock clock)
        {
            _action = action;
            _clock = clock;
            _timeInSeconds = timeInSeconds;
        }

        public void Start()
        {
            _clock.OnSecondPassed += OnSecondPassed;
        }

        public void Cancel ()
        {
            _clock.OnSecondPassed -= OnSecondPassed;
        }

        private Task OnSecondPassed(DateTime currentTick, DateTime lastTick)
        {
            _timeInSeconds--;
            if (_timeInSeconds <= 0)
            {
                Stop();
            }
            return Task.CompletedTask;
        }

        private void Stop()
        {
            _clock.OnSecondPassed -= OnSecondPassed;
            try
            {
                _action();
            } catch (Exception e)
            {
                OnExceptionOccured?.Invoke(e);
            }
        }
    }
}
