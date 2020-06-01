using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lomztein.Moduthulhu.Core
{
    public static class Log
    {
        public static int LogLevel { get; set; } = int.TryParse (Environment.GetEnvironmentVariable ("MODUTHULHU_LOGLEVEL"), out int level) ? level : int.MaxValue;
        public enum Type { CRITICAL, EXCEPTION, WARNING, SYSTEM, CONFIRM, CLIENT, BOT, PLUGIN, DATA, CHAT, CHANNEL, SERVER, USER, DEBUG }
        private static ConsoleColor[] _typeColor = new [] {
            ConsoleColor.Red, // CRITICAL
            ConsoleColor.DarkRed, // EXCEPTION
            ConsoleColor.Yellow, // WARNING

            ConsoleColor.Blue, // SYSTEM
            ConsoleColor.Green, // CONFIRM
            ConsoleColor.Green, // CLIENT
            ConsoleColor.Cyan, // BOT
            ConsoleColor.Green, // PLUGIN
            ConsoleColor.Magenta, // DATA

            ConsoleColor.White, // CHAT
            ConsoleColor.White, // CHANNEL
            ConsoleColor.White, // SERVER
            ConsoleColor.White, // USER

            ConsoleColor.Yellow, // DEBUG
        };

        private static void Write(ConsoleColor color, string prefix, string text) {
            Console.ForegroundColor = color;
            Console.WriteLine ($"[{prefix}] - [{DateTime.Now.ToString ("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)}] {text}");
        }

        public static void Write (Type type, string text) {
            if (LogLevel >= (int)type)
            {
                Write(GetColor (type), type.ToString(), text);
            }
        }

        public static void Exception (Exception exc) {
            Write (Type.EXCEPTION, $"[{exc.GetType().Name}] {exc.Message} - {exc.StackTrace}");
        }

        public static void System(string text) => Write(Type.SYSTEM, text);
        public static void Bot(string text) => Write(Type.BOT, text);
        public static void Plugin(string text) => Write(Type.PLUGIN, text);
        public static void Data(string text) => Write(Type.DATA, text);
        public static void Confirm(string text) => Write(Type.CONFIRM, text);

        public static void Chat(string text) => Write(Type.CHAT, text);
        public static void Client(string text) => Write(Type.CLIENT, text);
        public static void Channel(string text) => Write(Type.CHANNEL, text);
        public static void Server(string text) => Write(Type.SERVER, text);
        public static void User(string text) => Write(Type.USER, text);

        public static void Warning(string text) => Write(Type.WARNING, text);
        public static void Critical(string text) => Write(Type.CRITICAL, text);
        public static void Debug(string text) => Write(Type.DEBUG, text);


        public static ConsoleColor GetColor (Type type) {
            return _typeColor[(int)type];
        }
    }
}
