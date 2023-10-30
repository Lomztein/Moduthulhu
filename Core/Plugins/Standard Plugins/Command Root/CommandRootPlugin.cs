using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Autodocumentation;
using Lomztein.AdvDiscordCommands.ExampleCommands;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard {

    [Critical]
    [Descriptor ("Moduthulhu", "Command Root", "Default container and manager of bot commands.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Core/Plugin/Standard%20Plugins/Command%20Root")]
    public class CommandPlugin : PluginBase, ICommandSet {

        private CachedValue<char> _trigger;
        private CachedValue<char> _hiddenTrigger;

        string INamed.Name { get => Plugin.GetName (GetType ()); set => throw new InvalidOperationException(); }
        string INamed.Description { get => Plugin.GetDescription (GetType ()); set => throw new InvalidOperationException(); }

        private CommandRoot _commandRoot;

        public override void PreInitialize(GuildHandler handler) {
            base.PreInitialize(handler);

            _trigger = GetConfigCache("CommandTrigger", x => '!');
            _hiddenTrigger = GetConfigCache("CommandHiddenTrigger", x => '/');

            RegisterMessageAction("AddCommand", (x) => AddCommands(x[0] as ICommand));
            RegisterMessageAction("AddCommands", (x) => AddCommands(x.Cast<ICommand>().ToArray ()));
            RegisterMessageAction("RemoveCommand", (x) => RemoveCommands(x[0] as ICommand));
            RegisterMessageAction("RemoveCommands", (x) => RemoveCommands(x.Cast<ICommand>().ToArray()));

            _commandRoot = new CommandRoot (new List<ICommand> (),
                x => _trigger.GetValue (),
                x => _hiddenTrigger.GetValue ()
                );

            _commandRoot.AddCommands(new HelpCommand());

            AddConfigInfo<char>("Set Trigger", "Set trigger character.", x => _trigger.SetValue(x), (success, x) => $"Set trigger to '{_trigger.GetValue ()}'", "character");
            AddConfigInfo("Set Trigger", "Display current trigger.", () => $"Current trigger character is '{_trigger.GetValue ()}'");
            AddConfigInfo("Reset Trigger", "Reset trigger.", () => _trigger.SetValue('!'), (success) => "Reset trigger character to '!'");
            AddConfigInfo<char>("Set Hidden", "Set hidden character.", x => _trigger.SetValue(x), (success, x) => $"Set hidden trigger to '{_hiddenTrigger.GetValue()}'", "character");
            AddConfigInfo("Set Hidden", "Display hidden character.", () => $"Current hidden trigger character is '{_hiddenTrigger.GetValue ()}'");
            AddConfigInfo("Reset Hidden", "Reset hidden.", () => _trigger.SetValue('/'), (success) => "Reset hidden trigger character to '/'");
        }

        public override void Initialize() {
            GuildHandler.MessageReceived += OnMessageRecieved;
            GuildHandler.SlashCommandExecuted += SlashCommandExecuted;
            SetStateChangeHeaders("Commands", "The following commands has been added", "The following commands has been removed");
        }

        private async Task SlashCommandExecuted(SocketSlashCommand arg)
        {
            var result = await _commandRoot.ExecuteSlashCommand(arg);
            if (result != null)
            {
                await HandleSpecials(result, arg.Channel);
                await arg.RespondAsync(result.Message, HandleEmbed(result.Value));
            }
        }

        public override void PostInitialize() {
            _commandRoot.InitializeCommands ();

            TypeDescriptions.Add(typeof(GuildHandler), "Server Handler", "This represents a handler for a specific server, and is responsible for making sure a server can communicate with the bot core.");
            TypeDescriptions.Add(typeof(LargeEmbed), "Large Embed", "Special version of [Discord.Embed] that automatically splits into multiple [Discord.Embed]s to facilitate more than 25 fields.");
            TypeDescriptions.Add(typeof(BookMessage), "Book", "Specialized [Discord.IMessage] that allows for flipping between 'pages', like in a book.");
            TypeDescriptions.Add(typeof(LargeTextMessage), "Large Message", "Specialized [Discord.IMessage] that automatically splits into multiple [Discord.IMessage]s if a required to fit.");
            TypeDescriptions.Add(typeof(uint), "Positive Integer", "An integer number that may only be positive.");
        }

        private async Task OnMessageRecieved(SocketMessage arg) {
            await AwaitAndSend (arg);
        }

        private static Embed[] HandleEmbed(object result)
        {
            if (result is Embed embed)
                return new Embed[] { embed };
            if (result is Embed[])
                return result as Embed[];
            return null;
        }

        // This is neccesary since awaiting the result in the event would halt the rest of the bot, and we don't really want that.
        private async Task AwaitAndSend(SocketMessage arg) {

            if (arg is SocketUserMessage userMessage) // Temporary solution untill this can be fixed library-side.
            {
                var result = await _commandRoot.EnterCommand (userMessage);
                if (result != null)
                {
                    await HandleSpecials(result, arg.Channel);

                    MessageReference reference = new MessageReference(arg.Id, arg.Channel.Id);
                    await arg.Channel.SendMessageAsync(result.Message, false, result.Value as Embed, null, new AllowedMentions(AllowedMentionTypes.Everyone), reference);
                }
            }
        }

        private async Task HandleSpecials (Result result, ISocketMessageChannel channel)
        {
            if (result.Exception != null)
            {
                Core.Log.Exception(result.Exception);
            }

            if (result.Value is ISendable sendable)
            {
                await sendable.SendAsync(channel);
            }

            if (result.Value is IAttachable attachable)
            {
                attachable.Attach(GuildHandler);
            }
        }

        public override void Shutdown() {
            GuildHandler.MessageReceived -= OnMessageRecieved;
            GuildHandler.SlashCommandExecuted -= SlashCommandExecuted;

            GuildHandler.GetGuild().DeleteApplicationCommandsAsync().Wait();

            ClearMessageDelegates();
            ClearConfigInfos();
        }

        public List<ICommand> GetCommands() {
            return ((ICommandSet)_commandRoot).GetCommands ();
        }

        public void AddCommands(params ICommand [ ] newCommands) {
            Log ($"Adding commands: {string.Join (", ", newCommands.Select (x => x.Name).ToArray ())}");
            
            foreach (ICommand cmd in newCommands)
            {
                AddStateAttribute("Commands", cmd.Name, _commandRoot.GetChildPrefix (GuildHandler.GuildId) + cmd.Name);
                AddSlashCommand(cmd);
            }

            ((ICommandSet)_commandRoot).AddCommands (newCommands);
            _commandRoot.InitializeCommands();
        }

        private async void AddSlashCommand(ICommand cmd)
        {
            while (true)
            {
                try
                {
                    await GuildHandler.GetGuild().CreateApplicationCommandAsync(cmd.ToSlashCommand());
                    break;
                }
                catch (TimeoutException ex)
                {
                    Log(ex.Message);
                }
                catch(Exception ex)
                {
                    Log(ex.Message);
                    break;
                }
            }
            Log($"Slash command '{cmd.Name}' succesfully added.");
        }

        public void RemoveCommands(params ICommand [ ] commands) {
            Log ($"Removing commands: {string.Join (", ", commands.Select (x => x.Name).ToArray ())}");
            ((ICommandSet)_commandRoot).RemoveCommands (commands);
        }
    }
}
