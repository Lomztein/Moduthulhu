using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Miscellaneous
{
    [Descriptor("Lomztein", "Nut", "Plugin that allows the bot to nut, but only a certain month a year.")]
    public class NutPlugin : PluginBase
    {

        private CachedValue<int> _chanceStartDay;
        private CachedValue<float> _chanceStartPercent;
        private CachedValue<float> _chancePerDayPercent;
        private CachedValue<int> _refractoryPeriod;
        private CachedValue<List<ulong>> _nutters;
        private bool _canNut = true;

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
            _nutters = GetDataCache("Nutters", x => new List<ulong>());

            AddConfigInfo<int>("setchancestartday", "", (x) => _chanceStartDay.SetValue(x), (success, x) => $"Start day set to day {x}.", "day");
            AddConfigInfo<float>("setchancestartpercent", "", (x) => _chanceStartPercent.SetValue(x), (success, x) => $"Starting chance after starting day set to {x}%", "chance");
            AddConfigInfo<float>("setchanceperday", "", (x) => _chancePerDayPercent.SetValue(x), (success, x) => $"Chance per day after starting day set to {x}%", "chance per day");
            AddConfigInfo<int>("setrefractoryperiod", "", (x) => _refractoryPeriod.SetValue(x), (success, x) => $"Refractory period set to {x} seconds.", "seconds");

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

        public bool TryNut(out string message)
        {
            DateTime now = DateTime.Now;

            int startDay = _chanceStartDay.GetValue();
            float startChance = _chanceStartPercent.GetValue();
            float chancePerDay = _chancePerDayPercent.GetValue();


            if (now.Day < startDay)
            {
                message = FailNutBecauseTooEarly();
                return false;
            }
            else if (_canNut)
            {
                int daysInto = now.Day - startDay;
                float chance = startChance + chancePerDay * daysInto;

                Random random = new Random();
                if (random.Next(0, 100) < chance)
                {
                    message = SucceedNut();
                    return true;
                }
                else
                {
                    message = FailNutBecauseChance();
                    return false;
                }
            }
            else
            {
                message = FailNutBecauseCannotNut();
                return false;
            }
        }

        private void InvokeRefractoryPeriod (int time)
        {
            _canNut = false;
            InvokeTimedAction(() => _canNut = true, time);
        }

        public string FailNutBecauseTooEarly ()
        {
            InvokeRefractoryPeriod(_refractoryPeriod.GetValue());
            return RandomMessage(
                "It is much too early for me to nut, you must try again later this month.",
                "You cannot begin to hope I should nut this early. Unlike you mortals, I can control myself.",
                "Do you even think I'd nut this early? I am not like you.",
                "What makes you believe I would fail already. You may have nutted, but I have no intentions to do so yet.",
                "You shall not tempt me like this mortal, fear the consequences of doing so.",
                "How dare you attempt to make me nut. Even if it was time, you most certainly will not be the catalyst.",
                "Not quite yet I'm afraid. You must speak to y̶̨̹̰̖̥̅̌̏͑ͧ̕͠ỏ̷̖̣̭̪̹͙̻̦̹̿ͧ͑͟͠u̷̧̨̒̒ͮ̅̏͂́҉̫̝̦̰̰͈͈͎̳̼̲͖ͅr̵̛͍̤̭̟̝͎͓̲͖̟̻̈́ͦ̿͛ͩ͌͒̾ ̂ͧ͗ͨ̓ͤͣ̀ͮ̏͟҉͠҉͎̜͔͍͖͍̩̯̣͍̮̖͉̦͙m̴̢̗̬̯͚̪̰̫̙̦͇͈̏̅ͤ̐͒̄ͨ͊́͞o͂ͦͯͭ̅ͧͯ͞͏̮̞͙̪̝̦̞̩̻̪̭̻̯͔͍͕̘̙͇̕͝m̝̙͔̝̞̻̲̻̞̑ͭ̃̇̑͋͒̈̾́͘ to speed up time.",
                "I care little for how thicc you may be, it is not the time for nuts quite yet.",
                "Give it time mortal, or you will have no time to give at all."
                );
        }

        public string FailNutBecauseCannotNut ()
        {
            return RandomMessage(
                "You people have already attempted not too long ago, it is futile to attempt already.",
                "I need some time to recharge here, even cosmic horrs have refractory periods.",
                "Not quite ready, you impatient speck. If you desire my cosmic spunk, you must wait for it.",
                "The nut is not ready, calm the calamity, that is your mammaries.",
                "You have tried too recently, one cannot attempt nutting as often as you desire."
                );
        }

        public string FailNutBecauseChance ()
        {
            InvokeRefractoryPeriod(_refractoryPeriod.GetValue());
            return RandomMessage(
                "**HAH!** You thought you could make me nut. There is no chance for someone like you.",
                "Don't make me laugh, you are not worthy of my nut.",
                "I would pray to your weak gods if I were you. You cannot make me nut, and you will be punished for trying.",
                "You cannot make me nut. There is no one alive who can comprehend my sexual preference.",
                "Did you actually think you could make me nut? *You?* Have you looked yourself in the mirror recently?",
                "I have lived for untold eons, yet I have never met someone as undeserving of my nut, as you.",
                "Do you know how it feels to live in the v͍̙̺̭͚̰̙̞̭͊͗̽ͩͯ̕̕ơͫ̽ͤ͆ͩ̎ͧ̚͏̶̨̪̤͎̘̥̪ͅi̷̡̗̣̭̗̯̠̩͉̦̹̘̓͑ͯͩͩ͑͊̔̍̎ͪ̽͐ͭ͛̀ḑ̺͇̲̲̫͉̦̫̜ͣ͌̀ͮͣͫ̚̕ ̶̸͉̻̠̤̦͍̭̤̀ͤ́̆͗͐ͣ͗̋̚͞? It is actually quite pleasent, compared to *you.*",
                "Gah, I cannot believe you would even attempt. You do not have what it takes to make me nut.",
                "Do you hear them? Calling from the void? No? Maybe you would if you could get some of this nut. Which, of course, you cannot.",
                "Oh my god what even are you? I thought the other one was undeserving, but you are what my nightmares are made of.",
                "No.",
                "My mind is telling me no. My body is also telling me no. You are ugly."
                );
        }

        public string SucceedNut ()
        {
            InvokeRefractoryPeriod(_refractoryPeriod.GetValue());
            return RandomMessage(
                "**HAH! To think you can make me **NUUUHHHRHHT**. Oh, you made me nut. I'd better talk to my therapist.",
                "**PPFFTTHHFFF**fpfpft fucks sake not again. I had just bathed. You will now bathe me.",
                "..GOOD HEAVENS I'M ARRIVING.",
                "I once had a threesome with Lucifer and Niggorath, and, uhm, nevermind, thinking about it made me nut.",
                "They say no one is immune to my charms. I just looked in the mirror and this hypothesis was proven correct. Btw how do you clean a mirror?",
                "Stare upon me and try, but you will never comprehend my true form. Inste**HHHGMMGGN**ffpfph- you might want to clean that before it grows roots.",
                "**HOT DAMN** You are just what I've been searching for all these eons. I can now finally rest. You, on the other hand, have been ejaculated upon.",
                "This is getting harder and harder, and by the elders I might ju**FFPPPPFHHH**ffpph- nope, coulnd't hold it."
                );
        }

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
            public Task<Result> Execute (CommandMetadata data)
            {
                string message;
                string suffix;
                if (ParentPlugin.TryNut(out message)) {
                    suffix = $"You have made me nut {ParentPlugin.AddNutter(data.AuthorID)} time(s) now, be proud mortal.";
                }
                else
                {
                    suffix = "You have failed to make me nut. You are worthless.";
                }
                return TaskResult(message + "\n" + suffix, message + "\n" + suffix);
            }
        }
    }
}
