using Lomztein.Moduthulhu.Core.Bot;
using System;

namespace Lomztein.Moduthulhu.Core
{
    class Program
    {
        static void Main(string [ ] args) => new Bot.Core ().InitializeCore ().GetAwaiter ().GetResult ();
    }
}
