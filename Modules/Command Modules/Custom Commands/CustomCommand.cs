using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using Lomztein.AdvDiscordCommands.Framework.Categories;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public class CustomCommand : ModuleCommand<CustomCommandsModule>, ICustomCommand {

        public CustomCommand () {
            AvailableOnServer = true;
            AvailableInDM = true;
            CommandEnabled = true;
            Category = CustomCategory;
        }

        public string commandChain;

        public static readonly Category CustomCategory = new Category ("Custom", "Commands created from command-chains, and saved for later use.");
        public CommandAccessability Accessability { get; set; }
        public ulong OwnerID { get; set; }

        [Overload (typeof (object), "Call the custom command without arguments.")]
        public async Task<Result> Execute(CommandMetadata e) {
            return await e.Root.EnterCommand (commandChain, e.Message as IUserMessage);
        }

        [Overload (typeof (object), "Call the custom command with any number of arguments.")]
        public async Task<Result> Execute (CommandMetadata e, params dynamic[] arguments) {
            for (int i = 0; i < arguments.Length; i++) {
                CommandVariables.Set (e.Message.Id, "arg" + i.ToString (), arguments[i], true);
            }
            return await e.Root.EnterCommand (commandChain, e.Message as IUserMessage);
        }

        [Overload (typeof (object), "Call the custom command where the arguments are saved into a single array.")]
        public async Task<Result> Execute(CommandMetadata e, string arrayName, params dynamic [ ] arguments) {
            CommandVariables.Set (e.Message.Id, arrayName, arguments, true);
            return await e.Root.EnterCommand (commandChain, e.Message as IUserMessage);
        }

        public override string AllowExecution(CommandMetadata data) {
            return this.CheckAccessability (base.AllowExecution (data), data.Message as SocketMessage);
        }

        public CustomCommandData SaveToData() {
            CustomChainData data = new CustomChainData ();
            data.SetBaseValues (this);
            data.commandChain = commandChain;
            
            return data;
        }
    }
}
