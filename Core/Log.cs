using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core
{
    public static class Log
    {
        public enum Type { SYSTEM, BOT, PLUGIN, CONFIG, CHAT, CHANNEL, SERVER, USER, WARNING, EXCEPTION, CRITICAL }
        public static ConsoleColor[] TypeColor { get; private set; } = new ConsoleColor[] {

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

        public static void Write(ConsoleColor color, string prefix, string text) {
            Console.ForegroundColor = color;
            Console.WriteLine ($"[{prefix}] - [{DateTime.Now.ToString ()}] {text}");
        }

        public static void Write (Type type, string text) {
            Write (TypeColor[(int)type], type.ToString (), text);
        }

        public static void Write (Exception exception) {
            Write (Type.EXCEPTION, exception.Message + " - " + exception.StackTrace);
        }

        public static void Write(string text) {
            Write (Type.BOT, text);
        }

        public static ConsoleColor GetColor (Type type) {
            return TypeColor[(int)type];
        }
    }
}
