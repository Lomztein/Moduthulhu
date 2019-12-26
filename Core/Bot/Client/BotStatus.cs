using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotStatus
    {
        private int _currentTimePassed;
        private int _timeTreshold;
        private int _currentIndex;

        private Action<IActivity> _onChange;
        private StatusMessage[] _messages;

        public BotStatus (Action<IActivity> onChange, int treshold, params StatusMessage[] messages)
        {
            _onChange = onChange;
            _timeTreshold = treshold;
            _messages = messages;

            Change();
        }

        public Task Cycle (DateTime before, DateTime after)
        {
            _currentTimePassed++;
            if (_currentTimePassed >= _timeTreshold)
            {
                _currentIndex = (_currentIndex + 1) % _messages.Length;
                _currentTimePassed = 0;
                Change();
            }
            return Task.CompletedTask;
        }

        private void Change ()
        {
            StatusMessage msg = _messages[_currentIndex];
            _onChange(msg);
        }
    }
}
