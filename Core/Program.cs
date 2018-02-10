using Lomztein.ModularDiscordBot.Core.Bot;
using System;

namespace Lomztein.ModularDiscordBot.Core
{
    class Program
    {
        static void Main(string [ ] args) => new BotClient ().Initialize ().GetAwaiter ().GetResult ();
    }
}
