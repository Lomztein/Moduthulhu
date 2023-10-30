﻿using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Brainfuck
{
    [Descriptor ("Lomztein", "Brainfuck", "Plugin with a command that can interpret and execute Brainfuck programs.")]
    [Dependency ("Moduthulhu-Command Root")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Brainfuck/BrainfuckPlugin.cs")]
    public class BrainfuckPlugin : PluginBase {

        private BrainfuckCommand _command;

        private readonly Dictionary<ulong, char?> _input = new Dictionary<ulong, char?> ();

        public override void Initialize() {
            _command = new BrainfuckCommand { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _command);
        }

        public override void Shutdown() {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _command);
        }

        private async Task<string> Run (string program, ulong channelID) {
            if (_input.ContainsKey (channelID))
            {
                throw new InvalidOperationException($"A Brainfuck {nameof(program)} is already running in this channel.");
            }

            _input.Add (channelID, null);
            BrainfuckIntepreter intepreter = new BrainfuckIntepreter (new Func<Task<byte>> (async () => await AwaitInputAsync (channelID)));
            var result = await intepreter.Interpret (program);
            _input.Remove (channelID);
            return result;
        }

        private async Task<byte> AwaitInputAsync(ulong channelID) {
            if (_input.ContainsKey (channelID)) {
                while (_input [ channelID ] == null) {
                    await Task.Delay (1000);
                }

                return (byte)_input [ channelID ];
            }
            throw new ArgumentException ("There is no current program in channel " + channelID + ".");
        }

        public class BrainfuckCommand : PluginCommand<BrainfuckPlugin> {

            public BrainfuckCommand () {
                Name = "brainfuck";
                Description = "Brainfuck.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (string), "Brainfuck.")]
            public async Task<Result> Execute (ICommandMetadata metadata, string program) {
                string result = await ParentPlugin.Run (program, metadata.Channel.Id);
                return new Result (result, result);
            }

        }
    }
}
