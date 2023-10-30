using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Miscellaneous
{
    [Descriptor("Lomztein", "Nut", "Plugin that allows the bot to nut, but only a certain month a year.")]
    [Dependency("Lomztein-OpenAI")]
    public class NutPlugin : PluginBase
    {

        private CachedValue<int> _chanceStartDay;
        private CachedValue<float> _chanceStartPercent;
        private CachedValue<float> _chancePerDayPercent;
        private CachedValue<int> _refractoryPeriod;
        private CachedValue<int> _attemptCooldown;
        private CachedValue<List<ulong>> _nutters;

        private bool _onCooldown = false;
        private bool _recentNut = false;

        private static ulong[] _preciousIds = new ulong[] { 249307541648048138 };
        private static ulong[] _noHurtIds = new ulong[] { 249307541648048138 };

        private NutCommand _nutCommand;

        public override void Initialize()
        {
            if (!IsInNovember(DateTime.Now))
            {
                throw new NotNovemberException("I can only nut during november, the one month where one must not nut.");
            }

            _chanceStartDay = GetConfigCache("ChanceStartDay", x => 10);
            _chanceStartPercent = GetConfigCache("ChanceStartPercent", x => 10f);
            _chancePerDayPercent = GetConfigCache("ChancePerDayPercent", x => 5f);
            _refractoryPeriod = GetConfigCache("RefractoryPeriod", x => 60 * 60 * 2); // two hours.
            _attemptCooldown = GetConfigCache("AttemptCooldown", x => 60 * 60 * 2); // two hours.
            _nutters = GetDataCache("Nutters", x => new List<ulong>());

            AddConfigInfo<int>("setchancestartday", "", (x) => _chanceStartDay.SetValue(x), (success, x) => $"Start day set to day {x}.", "day");
            AddConfigInfo<float>("setchancestartpercent", "", (x) => _chanceStartPercent.SetValue(x), (success, x) => $"Starting chance after starting day set to {x}%", "chance");
            AddConfigInfo<float>("setchanceperday", "", (x) => _chancePerDayPercent.SetValue(x), (success, x) => $"Chance per day after starting day set to {x}%", "chance per day");
            AddConfigInfo<int>("setrefractoryperiod", "", (x) => _refractoryPeriod.SetValue(x), (success, x) => $"Refractory period set to {x} seconds.", "seconds");
            AddConfigInfo<int>("setattemptcooldown", "", (x) => _attemptCooldown.SetValue(x), (success, x) => $"Cooldown set to {x} seconds.", "seconds");

            GuildHandler.Clock.OnDayPassed += CheckDate;

            _nutCommand = new NutCommand()
            {
                ParentPlugin = this
            };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _nutCommand);
        }

        private Task CheckDate(DateTime currentTick, DateTime lastTick)
        {
            if (!IsInNovember(currentTick))
            {
                DisablePlugin("Nut plugin has been disabled, as it is no longer november.");
            }
            return Task.CompletedTask;
        }

        private bool IsPrecious(ulong id) => _preciousIds.Contains(id);

        public override void Shutdown()
        {
            GuildHandler.Clock.OnDayPassed -= CheckDate;
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _nutCommand);
        }

        private bool IsInNovember (DateTime date)
        {
            return date.Month == 11; // November = 11;
        }

        public class NotNovemberException : Exception
        {
            public NotNovemberException(string message) : base(message)
            {
            }
        }

        public async Task<Tuple<string, bool>> TryNut()
        {
            DateTime now = DateTime.Now;

            int startDay = _chanceStartDay.GetValue();
            float startChance = _chanceStartPercent.GetValue();
            float chancePerDay = _chancePerDayPercent.GetValue();


            if (now.Day < startDay)
            {
                return new Tuple<string, bool>("Could you stop that, it's not even november yet.", false);
            }
            else if (!_onCooldown)
            {
                int daysInto = now.Day - startDay;
                float chance = startChance + chancePerDay * daysInto;

                Random random = new Random();
                if (random.Next(0, 100) < chance)
                {
                    return new Tuple<string, bool>(await GetSuccessMessage(), true);
                }
                else
                {
                    return new Tuple<string, bool>(await GetChanceFailMessage(), false);
                }
            }
            else
            {
                if (_recentNut)
                    return new Tuple<string, bool>(await GetRefractoryPeriodFailMessage(), false);
                return new Tuple<string, bool>(await GetCooldownFailMessage(), false);
            }
        }

        private void InvokeCooldownPeriod (int time)
        {
            _onCooldown = true;
            InvokeTimedAction(ResetNut, time);
        }

        private void ResetNut()
        {
            _onCooldown = false;
            _recentNut = false;
        }

        private async Task<string> GetCooldownFailMessage() =>
            await SendMessage<Task<string>>("Lomztein-OpenAI", "GetChatResponseAsync_DefaultProfileBase", "You have an assertive, degrading tone. They are attempting to get you to orgasm, but failed because they tried too recently.", "cum.");

        private async Task<string> GetRefractoryPeriodFailMessage() =>
            await SendMessage<Task<string>>("Lomztein-OpenAI", "GetChatResponseAsync_DefaultProfileBase", "You have an assertive, degrading tone. They are attempting to get you to orgasm, but failed because you have achieved orgasm too recently.", "cum.");

        private async Task<string> GetChanceFailMessage() =>
            await SendMessage<Task<string>>("Lomztein-OpenAI", "GetChatResponseAsync_DefaultProfileBase", "You have an assertive, degrading tone. They are attempting to get you to orgasm, but failed.", "cum.");

        private async Task<string> GetSuccessMessage() =>
            await SendMessage<Task<string>>("Lomztein-OpenAI", "GetChatResponseAsync_DefaultProfileBase", "You have an assertive, degrading tone. They are attempting to get you to orgasm, and succeeded, which you are very embarrased about.", "cum.");

        public int GetNutCount (ulong user)
        {
            var nutters = _nutters.GetValue();
            return nutters.Count(x => x == user);
        }

        public int AddNutter (ulong user)
        {
            _nutters.MutateValue(x => x.Add(user));
            return GetNutCount(user);
        }

        private string RandomMessage (params string[] messages)
        {
            Random random = new Random();
            return messages[random.Next(0, messages.Length)];
        }


        public class NutCommand : PluginCommand<NutPlugin>
        {
            public NutCommand ()
            {
                Name = "nut";
                Description = "Attempt the nut.";
                Category = StandardCategories.Utility;
                Aliases = new [] { "cum" };
            }

            [Overload(typeof (string), "The command which nuts.")]
            public async Task<Result> Execute (ICommandMetadata data)
            {
                string suffix;
                var result = await ParentPlugin.TryNut();
                string message = result.Item1;
                if (result.Item2) {
                    suffix = $"You have made me nut {ParentPlugin.AddNutter(data.AuthorId)} time(s) now, be proud mortal.";
                }
                else
                {
                    suffix = "You have failed to make me nut. " + (ParentPlugin.IsPrecious(data.AuthorId) ? "But that's okay I still love you." : "You are worthless.");
                }
                await data.Channel.SendMessageAsync(message + " " + suffix);
                return new Result(null, null);
            }
        }
    }
}
