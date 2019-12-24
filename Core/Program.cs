using Lomztein.Moduthulhu.Core.Bot;
using System;

namespace Lomztein.Moduthulhu.Core
{
    public static class Program
    {
        static void Main(string [ ] args) => new BotCore ().InitializeCore (args).GetAwaiter ().GetResult ();
    }
}
