using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Bot
{
    public static class Log
    {
        public enum Type { SYSTEM, BOT, CHAT, CONFIG, WARNING, EXCEPTION, CRITICAL, MODULE }
        private static ConsoleColor [ ] typeColor = new ConsoleColor[] { ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.White, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.DarkRed, ConsoleColor.Red, ConsoleColor.Magenta };
        
        public static void Write (Type type, string text) {
            Console.ForegroundColor = typeColor [ (int)type ];
            Console.WriteLine ($"[{type.ToString ()}] - [{DateTime.Now.ToString ()}] {text}");
            Console.ResetColor ();
        }

        public static void Write (Exception exception) {
            Write (Type.EXCEPTION, exception.Message + " - " + exception.StackTrace);
        }
    }
}
