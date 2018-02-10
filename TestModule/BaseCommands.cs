using System;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.ExampleCommands;
using System.Collections.Generic;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.TestModule {

    public class BaseCommands : ModuleBase {

        public override string Name => "Base Commands";
        public override string Author => "Lomztein";
        public override string Description => "A module containing all the basic example commands from the command framework.";

        private List<Command> commands = new List<Command> () {
                    new HelpCommand (),
                    new PrintCommand (),
                    new MathCommandSet (),
                    new FlowCommandSet (),
                    new VariableCommandSet (),
                    new DiscordCommandSet (),
                    new MiscCommandSet (),
                    new CallstackCommand ()
        };

        public override void Initialize() {
            CommandRoot commandRoot = ParentBotClient.GetCommandRoot ();
            commandRoot.AddCommands (commands.ToArray ());

            ParentBotClient.discordClient.MessageReceived += MessageRecievedEvent;
        }

        private async Task MessageRecievedEvent(SocketMessage arg) {
            var result = await ParentBotClient.GetCommandRoot ().EnterCommand (arg as SocketUserMessage);
            await arg.Channel.SendMessageAsync (result?.message, false, result?.value as Embed);
        }

        public override void Shutdown() {
            CommandRoot commandRoot = ParentBotClient.GetCommandRoot ();
            commandRoot.RemoveCommands (commands.ToArray ());
        }
    }

}
