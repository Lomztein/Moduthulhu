using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Lomztein.ModularDiscordBot.Modules.CommandRoot;
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

        private BrainfuckCommand command = new BrainfuckCommand ();

        private Dictionary<ulong, char?> input = new Dictionary<ulong, char?> ();

        public override void Initialize() {
            throw new NotImplementedException ();
        }

        public override void Shutdown() {
            throw new NotImplementedException ();
        }

        private async Task<string> Run (string program, ulong channelID) {
            if (input.ContainsKey (channelID))
                throw new Exception ("A Brainfuck program is already running in this channel.");

            input.Add (channelID, null);
            BrainfuckIntepreter intepreter = new BrainfuckIntepreter (new Func<Task<byte>> (async () => await AwaitInputAsync (channelID)));
            return await intepreter.Interpret (program);
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
                command = "brainfuck";
                shortHelp = "Brainfuck.";
                catagory = Category.Fun;
            }

            public async Task<Result> Execute (CommandMetadata metadata, string program) {
                string result = await parentModule.Run (program, metadata.message.Channel.Id);
                return new Result (result, result);
            }

        }
    }
}
