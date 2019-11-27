using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lomztein.Moduthulhu.Core
{
    public static class Log
    {
        public enum Type { SYSTEM, BOT, PLUGIN, DATA, CHAT, CONFIRM, CHANNEL, SERVER, USER, WARNING, EXCEPTION, CRITICAL }
        private static ConsoleColor[] _typeColor = new [] {

            ConsoleColor.Blue, // SYSTEM
            ConsoleColor.Cyan, // BOT
            ConsoleColor.Green, // PLUGIN
            ConsoleColor.Magenta, // DATA
            ConsoleColor.Green, // CONFIRM

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
            Console.WriteLine ($"[{prefix}] - [{DateTime.Now.ToString ("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}] {text}");
        }

        public static void Write (Type type, string text) {
            Write (_typeColor[(int)type], type.ToString (), text);
        }

        public static void Exception (Exception exception) {
            Write (Type.EXCEPTION, exception.Message + " - " + exception.StackTrace);
        }

        public static void Write(string text) {
            Write (Type.BOT, text);
        }

        public static void System(string text) => Write(Log.Type.SYSTEM, text);
        public static void Bot(string text) => Write(Log.Type.BOT, text);
        public static void Plugin(string text) => Write(Log.Type.PLUGIN, text);
        public static void Data(string text) => Write(Log.Type.DATA, text);
        public static void Confirm(string text) => Write(Log.Type.CONFIRM, text);

        public static void Chat(string text) => Write(Log.Type.CHAT, text);
        public static void Channel(string text) => Write(Log.Type.CHANNEL, text);
        public static void Server(string text) => Write(Log.Type.SERVER, text);
        public static void User(string text) => Write(Log.Type.USER, text);

        public static void Warning(string text) => Write(Log.Type.WARNING, text);
        public static void Critical(string text) => Write(Log.Type.CRITICAL, text);


        public static ConsoleColor GetColor (Type type) {
            return _typeColor[(int)type];
        }
    }
}
