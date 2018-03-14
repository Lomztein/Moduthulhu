using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using System.Collections.Generic;

namespace Lomztein.Moduthulhu.Modules.CommandRoot
{
    public class CommandRootModule : ModuleBase, ICommandSet {

        public override string Name => "Command Root";
        public override string Description => "A base module for implementing the Advanced Discord Commands framework.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public AdvDiscordCommands.Framework.CommandRoot commandRoot;

        public override void PreInitialize() {
            commandRoot = new AdvDiscordCommands.Framework.CommandRoot ();
        }

        public override void Initialize() {
            ParentBotClient.discordClient.MessageReceived += OnMessageRecieved;
        }

        public override void PostInitialize() {
            commandRoot.InitializeCommands ();
        }

        private Task OnMessageRecieved(SocketMessage arg) {
            AwaitAndSend (arg);
            return Task.CompletedTask;
        }

        // This is neccesary since awaiting the result in the event would halt the rest of the bot, and we don't really want that.
        private async void AwaitAndSend(SocketMessage arg) {
            try {
                var result = await commandRoot.EnterCommand (arg as SocketUserMessage);
                if (result != null)
                    await MessageControl.SendMessage (arg.Channel as ITextChannel, result?.message, false, result?.value as Embed);
            } catch (Exception e) {
                Log.Write (e);
            }
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.MessageReceived -= OnMessageRecieved;
        }

        public List<Command> GetCommands() {
            return ((ICommandSet)commandRoot).GetCommands ();
        }

        public void AddCommands(params Command [ ] newCommands) {
            ((ICommandSet)commandRoot).AddCommands (newCommands);
        }

        public void RemoveCommands(params Command [ ] commands) {
            ((ICommandSet)commandRoot).RemoveCommands (commands);
        }
    }
}
