using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard {


    [Descriptor ("Lomztein", "Command Root", "Default container and manager of bot commands.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    public class CommandPlugin : PluginBase, ICommandSet {

        private CachedValue<char> _trigger;
        private CachedValue<char> _hiddenTrigger;

        string INamed.Name { get => Name; set => throw new InvalidOperationException(); }
        string INamed.Description { get => Description; set => throw new InvalidOperationException(); }

        private CommandRoot _commandRoot;

        public override void PreInitialize(GuildHandler handler) {
            base.PreInitialize(handler);

            _trigger = GetConfigCache("CommandTrigger", x => '!');
            _hiddenTrigger = GetConfigCache("CommandHiddenTrigger", x => '/');

            RegisterMessageAction("AddCommand", (x) => AddCommands((ICommand)x));
            RegisterMessageAction("AddCommands", (x) => AddCommands((ICommand[])x));
            RegisterMessageAction("RemoveCommand", (x) => RemoveCommands((ICommand)x));
            RegisterMessageAction("RemoveCommands", (x) => RemoveCommands((ICommand[])x));

            _commandRoot = new CommandRoot (new List<ICommand> (),
                x => _trigger.GetValue (),
                x => _hiddenTrigger.GetValue ()
                );
        }

        public override void Initialize() {
            GuildHandler.MessageReceived += OnMessageRecieved;
        }

        public override void PostInitialize() {
            _commandRoot.InitializeCommands ();
        }

        private async Task OnMessageRecieved(SocketMessage arg) {
            await AwaitAndSend (arg);
        }

        // This is neccesary since awaiting the result in the event would halt the rest of the bot, and we don't really want that.
        private async Task AwaitAndSend(SocketMessage arg) {

            var result = await _commandRoot.EnterCommand (arg.Content, arg as SocketUserMessage, arg.GetGuild ().Id);
            if (result != null) {

                if (result.Exception != null)
                {
                    Log(result.Exception.Message);
                }

                if (result.Value is ISendable sendable)
                    await sendable.SendAsync (arg.Channel);

                await MessageControl.SendMessage (arg.Channel as ITextChannel, result?.GetMessage (), false, result?.Value as Embed);
            }
        }

        public override void Shutdown() {
            GuildHandler.MessageReceived -= OnMessageRecieved;

            UnregisterMessageAction("AddCommand");
            UnregisterMessageAction("AddCommands");
            UnregisterMessageAction("RemoveCommand");
            UnregisterMessageAction("RemoveCommands");
        }

        public List<ICommand> GetCommands() {
            return ((ICommandSet)_commandRoot).GetCommands ();
        }

        public void AddCommands(params ICommand [ ] newCommands) {
            Log ($"Adding commands: {newCommands.Select (x => x.Name).ToArray ().Singlify ()}");
            ((ICommandSet)_commandRoot).AddCommands (newCommands);
        }

        public void RemoveCommands(params ICommand [ ] commands) {
            Log ($"Removing commands: {commands.Select (x => x.Name).ToArray ().Singlify ()}");
            ((ICommandSet)_commandRoot).RemoveCommands (commands);
        }
    }
}
