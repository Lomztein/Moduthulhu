using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public class CustomCommand : ModuleCommand<CustomCommandsModule>, ICustomCommand {

        public CustomCommand () {
            availableOnServer = true;
            availableInDM = true;
            commandEnabled = true;
        }

        public string commandChain;

        public CommandAccessability Accessability { get; set; }
        public ulong OwnerID { get; set; }
        public string Name { get => command; }

        [Overload (typeof (object), "Call the custom command without arguments.")]
        public async Task<Result> Execute(CommandMetadata e) {
            return await e.root.EnterCommand (commandChain, e.message);
        }

        [Overload (typeof (object), "Call the custom command with any number of arguments.")]
        public async Task<Result> Execute (CommandMetadata e, params dynamic[] arguments) {
            for (int i = 0; i < arguments.Length; i++) {
                CommandVariables.Set (e.message.Id, "arg" + i.ToString (), arguments[i], true);
            }
            return await e.root.EnterCommand (commandChain, e.message);
        }

        [Overload (typeof (object), "Call the custom command where the arguments are saved into a single array.")]
        public async Task<Result> Execute(CommandMetadata e, string arrayName, params dynamic [ ] arguments) {
            CommandVariables.Set (e.message.Id, arrayName, arguments, true);
            return await e.root.EnterCommand (commandChain, e.message);
        }

        public override string AllowExecution(IMessage e) {
            return this.CheckAccessability (base.AllowExecution (e), e as SocketMessage);
        }

        public CustomCommandData SaveToData() {
            CustomChainData data = new CustomChainData ();
            data.SetBaseValues (this);
            data.commandChain = commandChain;
            
            return data;
        }
    }
}
