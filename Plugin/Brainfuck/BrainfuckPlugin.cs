using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Brainfuck
{
    [Descriptor ("Lozmtein", "Brainfuck", "Plugin with a command that can interpret and execute Brainfuck programs.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Brainfuck/BrainfuckPlugin.cs")]
    public class BrainfuckPlugin : PluginBase {

        private BrainfuckCommand command;

        private Dictionary<ulong, char?> input = new Dictionary<ulong, char?> ();

        public override void Initialize() {
            command = new BrainfuckCommand () { ParentPlugin = this };
            SendMessage("Lomztein-Command Root", "AddCommand", command);
        }

        public override void Shutdown() {
            SendMessage("Lomztein-Command Root", "RemoveCommand", command);
        }

        private async Task<string> Run (string program, ulong channelID) {
            if (input.ContainsKey (channelID))
                throw new Exception ("A Brainfuck program is already running in this channel.");

            input.Add (channelID, null);
            BrainfuckIntepreter intepreter = new BrainfuckIntepreter (new Func<Task<byte>> (async () => await AwaitInputAsync (channelID)));
            var result = await intepreter.Interpret (program);
            input.Remove (channelID);
            return result;
        }

        private async Task<byte> AwaitInputAsync(ulong channelID) {
            if (input.ContainsKey (channelID)) {
                while (input [ channelID ] == null) {
                    await Task.Delay (1000);
                }

                return (byte)input [ channelID ];
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
            public async Task<Result> Execute (CommandMetadata metadata, string program) {
                string result = await ParentPlugin.Run (program, metadata.Message.Channel.Id);
                return new Result (result, result);
            }

        }
    }
}
