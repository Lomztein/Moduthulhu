using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    // TODO: Extend this system to have a number of static rate limiters.
    /// <summary>
    /// This class is currently useless, don't use it. Kept in case it can prove useful someday in the future.
    /// </summary>
    internal class RateLimiter
    {
        private readonly Clock _clock;
        private int _maxRequests;
        private int _currentRequests;
        private int _timeInSeconds;

        public event Action<Exception> OnExceptionOccured;

        public RateLimiter (int maxRequests, int timeInSeconds, Clock clock)
        {
            _clock = clock;
            _maxRequests = maxRequests;
            _timeInSeconds = timeInSeconds;
        }

        public bool TryRequest (Action request)
        {
            if (!IsMaxed ())
            {
                try
                {
                    request();
                } catch (Exception e)
                {
                    OnExceptionOccured?.Invoke(e);
                }

                Enqueue();
                return true;
            }
            return false;
        }

        private void Enqueue ()
        {
            _currentRequests++;
            TimedAction timedAction = new TimedAction(() => _currentRequests--, _timeInSeconds, _clock);
            timedAction.Start();
        }

        public bool IsMaxed() => _currentRequests >= _maxRequests;
    }
}
