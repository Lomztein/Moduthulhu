﻿using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Bot.Misc;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Cross;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Command {

    public class CommandRootModule : ModuleBase, ICommandSet, IConfigurable<MultiConfig> {

        public override string Name => "Command Root";
        public override string Description => "A base module for implementing the Advanced Discord Commands framework.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        string INamed.Name { get => Name; set => throw new NotImplementedException (); }
        string INamed.Description { get => Description; set => throw new NotImplementedException (); }

        [AutoConfig] private MultiEntry<char> trigger = new MultiEntry<char> (x => '!', "Trigger");
        [AutoConfig] private MultiEntry<char> hiddenTrigger = new MultiEntry<char> (x => '/', "HiddenTrigger");

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        public CommandRoot commandRoot;

        public override void PreInitialize() {

            commandRoot = new CommandRoot (new List<ICommand> (),
                x => trigger.GetEntry (new FakeEntity<ulong> (x)),
                x => hiddenTrigger.GetEntry (new FakeEntity<ulong> (x))
                );

        }

        public override void Initialize() {
            ParentShard.MessageReceived += OnMessageRecieved;
        }

        public override void PostInitialize() {
            commandRoot.InitializeCommands ();
        }

        private async Task OnMessageRecieved(SocketMessage arg) {
            await AwaitAndSend (arg);
        }

        // This is neccesary since awaiting the result in the event would halt the rest of the bot, and we don't really want that.
        private async Task AwaitAndSend(SocketMessage arg) {

            var result = await commandRoot.EnterCommand (arg.Content, arg as SocketUserMessage, arg.GetGuild ().Id);
            if (result != null) {

                if (result.Exception != null)
                    Log.Write (result.Exception);

                if (result.Value is ISendable sendable)
                    await sendable.SendAsync (arg.Channel);

                await MessageControl.SendMessage (arg.Channel as ITextChannel, result?.GetMessage (), false, result?.Value as Embed);
            }

        }

        public override void Shutdown() {
            ParentShard.MessageReceived -= OnMessageRecieved;
        }

        public List<ICommand> GetCommands() {
            return ((ICommandSet)commandRoot).GetCommands ();
        }

        public void AddCommands(params ICommand [ ] newCommands) {
            this.Log ($"Adding commands: {newCommands.Select (x => x.Name).ToArray ().Singlify ()}");
            ((ICommandSet)commandRoot).AddCommands (newCommands);
        }

        public void RemoveCommands(params ICommand [ ] commands) {
            this.Log ($"Removing commands: {commands.Select (x => x.Name).ToArray ().Singlify ()}");
            ((ICommandSet)commandRoot).RemoveCommands (commands);
        }
    }
}
