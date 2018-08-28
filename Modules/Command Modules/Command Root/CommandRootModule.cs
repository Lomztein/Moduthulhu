using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Lomztein.Moduthulhu.Core.Bot;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Bot.Misc;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.AdvDiscordCommands.Framework;

namespace Lomztein.Moduthulhu.Modules.CommandRoot {
    public class CommandRootModule : ModuleBase, ICommandSet, IConfigurable<MultiConfig> {

        public override string Name => "Command Root";
        public override string Description => "A base module for implementing the Advanced Discord Commands framework.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        string INamed.Name { get => Name; set => throw new NotImplementedException (); }
        string INamed.Description { get => Description; set => throw new NotImplementedException (); }

        [AutoConfig] private MultiEntry<char, SocketGuild> trigger = new MultiEntry<char, SocketGuild> (x => '!', "Trigger");
        [AutoConfig] private MultiEntry<char, SocketGuild> hiddenTrigger = new MultiEntry<char, SocketGuild> (x => '/', "HiddenTrigger");

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        public AdvDiscordCommands.Framework.CommandRoot commandRoot;

        public override void PreInitialize() {
            commandRoot = new AdvDiscordCommands.Framework.CommandRoot (new List<ICommand> (), new Executor ());
            commandRoot.CommandExecutor.Trigger = new Func<ulong, char> (x => trigger.GetEntry (new FakeEntity<ulong> (x)));
            commandRoot.CommandExecutor.HiddenTrigger = new Func<ulong, char> (x => hiddenTrigger.GetEntry (new FakeEntity<ulong> (x)));
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
                var result = await commandRoot.EnterCommand (arg.Content, arg as SocketUserMessage);
                if (result != null) {

                    if (result.Exception != null)
                        Log.Write (Log.Type.EXCEPTION, result.Exception.TargetSite.Name);

                    if (result.Value is ISendable sendable)
                        await sendable.SendAsync (arg.Channel);

                    await MessageControl.SendMessage (arg.Channel as ITextChannel, result?.GetMessage (), false, result?.Value as Embed);
                }
            } catch (Exception e) {
                Log.Write (e);
            }
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.MessageReceived -= OnMessageRecieved;
        }

        public List<ICommand> GetCommands() {
            return ((ICommandSet)commandRoot).GetCommands ();
        }

        public void AddCommands(params ICommand [ ] newCommands) {
            ((ICommandSet)commandRoot).AddCommands (newCommands);
        }

        public void RemoveCommands(params ICommand [ ] commands) {
            ((ICommandSet)commandRoot).RemoveCommands (commands);
        }
    }
}
