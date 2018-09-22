using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Brainfuck
{
    public class BrainfuckModule : ModuleBase {

        public override string Name => "Brainfuck";
        public override string Description => "why does this exist";
        public override string Author => "I don't want my name on this.";

        public override bool Multiserver => true;

        private BrainfuckCommand command;

        private Dictionary<ulong, char?> input = new Dictionary<ulong, char?> ();

        public override void Initialize() {
            command = new BrainfuckCommand () { ParentModule = this };
            ParentModuleHandler.GetModule<CommandRootModule> ().AddCommands (command);
        }

        public override void Shutdown() {
            ParentModuleHandler.GetModule<CommandRootModule> ().RemoveCommands (command);
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

        public class BrainfuckCommand : ModuleCommand<BrainfuckModule> {

            public BrainfuckCommand () {
                Name = "brainfuck";
                Description = "Brainfuck.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (string), "Brainfuck.")]
            public async Task<Result> Execute (CommandMetadata metadata, string program) {
                string result = await ParentModule.Run (program, metadata.Message.Channel.Id);
                return new Result (result, result);
            }

        }
    }
}
