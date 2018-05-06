using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public static class Log
    {
        public enum Type { SYSTEM, BOT, MODULE, CONFIG, CHAT, CHANNEL, SERVER, USER, WARNING, EXCEPTION, CRITICAL }
        private static ConsoleColor [ ] typeColor = new ConsoleColor[] {

            ConsoleColor.Blue, // SYSTEM
            ConsoleColor.Cyan, // BOT
            ConsoleColor.Green, // MODULE
            ConsoleColor.Magenta, // CONFIG

            ConsoleColor.White, // CHAT
            ConsoleColor.White, // CHANNEL
            ConsoleColor.White, // SERVER
            ConsoleColor.White, // USER

            ConsoleColor.Yellow, // WARNING
            ConsoleColor.DarkRed, // EXCEPTION
            ConsoleColor.Red // CRITICAL
        };
        
        public static void Write (Type type, string text) {
            Console.ForegroundColor = typeColor [ (int)type ];
            Console.WriteLine ($"[{type.ToString ()}] - [{DateTime.Now.ToString ()}] {text}");
            Console.ResetColor ();
        }

        public static void Write (Exception exception) {
            Write (Type.EXCEPTION, exception.Message + " - " + exception.StackTrace);
        }

        public static void Write(string text) {
            Write (Type.BOT, text);
        }
    }
}
