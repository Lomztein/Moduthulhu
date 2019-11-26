using System;
using System.Collections.Generic;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System.Linq;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;

namespace Lomztein.Moduthulhu.Modules.Phrases
{
    [Descriptor ("Lomztein", "Response Phrases", "Responds to certain phrases from certain users in certain channels with a certain chance.")]
    public class PhrasesPlugin : PluginBase {

        private CachedValue<List<Phrase>> _phrases;

        public override void Initialize() {
            GuildHandler.MessageReceived += OnMessageRecieved;
            _phrases = GetConfigCache("Phrases", x => new List<Phrase>());

            #region
            AddConfigInfo("Add Phrase", "Add empty phrase.", new Action(() => { _phrases.GetValue().Add(new Phrase()); _phrases.Store(); }), () => $"Added new empty response phrase at index {_phrases.GetValue ().Count - 1}. You must edit it using the other config options available here.");
            AddConfigInfo("Remove Phrase", "Remove phrase.", new Action<int>((x) => { _phrases.GetValue().RemoveAt(x); _phrases.Store(); }), () => $"Removed response phrase at the given index.", "Index");
            AddConfigInfo("List Phrases", "List current phrases.", () => "Current phrases:\n" + string.Join('\n', _phrases.GetValue().Select(x => _phrases.GetValue ().IndexOf (x) + " -> " + x.ToString (GuildHandler))));
            
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action(() => { _phrases.GetValue().LastOrDefault().Trigger = string.Empty; _phrases.Store(); }), () => $"Phrase trigger reset.");
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().Trigger = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Trigger");
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<int>((x) => { _phrases.GetValue()[x].Trigger = string.Empty; _phrases.Store(); }), () => $"Phrase trigger reset.", "Index");
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<int, string>((x, y) => { _phrases.GetValue()[x].Trigger = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Index", "Trigger");

            AddConfigInfo("Set Phrase User", "Set user", new Action(() => { _phrases.GetValue().LastOrDefault ().UserId = 0; _phrases.Store(); }), () => $"Phrase user reset.");
            AddConfigInfo("Set Phrase User", "Set user", new Action<SocketGuildUser>((y) => { _phrases.GetValue().LastOrDefault ().UserId = y.Id; _phrases.Store(); }), () => $"Phrase user updated.", "User");
            AddConfigInfo("Set Phrase User", "Set user", new Action<string>((y) => { _phrases.GetValue().LastOrDefault ().UserId = GuildHandler.FindUser(y).Id; _phrases.Store(); }), () => $"Phrase user updated.", "Username");
            AddConfigInfo("Set Phrase User", "Set user", new Action<ulong>((y) => { _phrases.GetValue().LastOrDefault ().UserId = y; _phrases.Store(); }), () => $"Phrase user updated.", "User ID");

            AddConfigInfo("Set Phrase User", "Set user", new Action<int>((x) => { _phrases.GetValue()[x].UserId = 0; _phrases.Store(); }), () => $"Phrase user reset.", "Index");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, SocketGuildUser>((x, y) => { _phrases.GetValue()[x].UserId = y.Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, string>((x, y) => { _phrases.GetValue()[x].UserId = GuildHandler.FindUser (y).Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "Username");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, ulong>((x, y) => { _phrases.GetValue()[x].UserId = y; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User ID");

            AddConfigInfo("Set Phrase Channel", "Set channel", new Action(() => { _phrases.GetValue().LastOrDefault ().ChannelId = 0; _phrases.Store(); }), () => $"Phrase channel reset.");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<SocketTextChannel>((y) => { _phrases.GetValue().LastOrDefault ().ChannelId = y.Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Channel");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().ChannelId = GuildHandler.FindTextChannel(y).Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Channel Name");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<ulong>((y) => { _phrases.GetValue().LastOrDefault().ChannelId = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Channel ID");

            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int>((x) => { _phrases.GetValue()[x].ChannelId = 0; _phrases.Store(); }), () => $"Phrase channel reset.", "Index");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, SocketTextChannel>((x, y) => { _phrases.GetValue()[x].ChannelId = y.Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Index", "Channel");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, string>((x, y) => { _phrases.GetValue()[x].ChannelId = GuildHandler.FindTextChannel (y).Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Index", "Channel Name");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, ulong>((x, y) => { _phrases.GetValue()[x].ChannelId = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Index", "Channel ID");

            AddConfigInfo("Set Phrase Chance", "Set chance", new Action(() => { _phrases.GetValue().LastOrDefault ().Chance = 0; _phrases.Store(); }), () => $"Phrase chance reset.");
            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<double>((y) => { _phrases.GetValue().LastOrDefault ().Chance = Math.Clamp(y, 0d, 100d); _phrases.Store(); }), () => $"Phrase chance updated.", "Chance");
            AddConfigInfo("Set Phrase Response", "Set response", new Action(() => { _phrases.GetValue().LastOrDefault().Response = string.Empty; _phrases.Store(); }), () => $"Phrase response reset.");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().Response = y; _phrases.Store(); }), () => $"Phrase response updated.", "Response");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action(() => { _phrases.GetValue().LastOrDefault().Emoji = string.Empty; _phrases.Store(); }), () => $"Phrase Emoji reset.");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().Emoji = y; _phrases.Store(); }), () => $"Phrase Emoji updated.", "Emoji");

            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<int>((x) => { _phrases.GetValue()[x].Chance = 100; _phrases.Store(); }), () => $"Phrase chance reset.", "Index");
            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<int, double>((x, y) => { _phrases.GetValue()[x].Chance = Math.Clamp (y, 0d, 100d); _phrases.Store(); }), () => $"Phrase chance updated.", "Index", "Chance");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<int>((x) => { _phrases.GetValue()[x].Response = string.Empty; _phrases.Store(); }), () => $"Phrase response reset.", "Index");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<int, string>((x, y) => { _phrases.GetValue()[x].Response = y; _phrases.Store(); }), () => $"Phrase response updated.", "Index", "Response");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<int>((x) => { _phrases.GetValue()[x].Emoji = string.Empty; _phrases.Store(); }), () => $"Phrase Emoji reset.", "Index");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<int, string>((x, y) => { _phrases.GetValue()[x].Emoji = y; _phrases.Store(); }), () => $"Phrase Emoji updated.", "Index", "Emoji");
            #endregion
        }

        private async Task OnMessageRecieved(SocketMessage message) {
            await CheckAndRespond (message);
        }

        private async Task CheckAndRespond(SocketMessage message)
        {
            string response = null;
            string emote = null;

            if (message.Author.Id == GuildHandler.BotUser.Id)
            {
                return;
            }

            foreach (Phrase phrase in _phrases.GetValue ())
            {
                (response, emote) = phrase.CheckAndReturnResponse(message as SocketUserMessage);
                if (string.IsNullOrWhiteSpace(response) && string.IsNullOrWhiteSpace(emote))
                {
                    continue;
                }
                break;
            }



            if (!string.IsNullOrWhiteSpace(response))
            {
                await MessageControl.SendMessage(message.Channel as ITextChannel, response);
            }

            if (!string.IsNullOrWhiteSpace (emote) && Emote.TryParse(emote, out Emote parsedEmote))
            {
                await (message as SocketUserMessage).AddReactionAsync(parsedEmote);
            }
        }

        public override void Shutdown() {
            GuildHandler.MessageReceived -= OnMessageRecieved;
        }

        public class Phrase {

            public string Trigger { get; set; } = string.Empty;
            public ulong UserId { get; set; }
            public ulong ChannelId { get; set; }
            public double Chance { get; set; } = 100d;

            public string Response { get; set; } = string.Empty;
            public string Emoji { get; set; } = string.Empty;

            public (string res, string emo) CheckAndReturnResponse (SocketUserMessage message) {
                if (string.IsNullOrEmpty (Trigger) || message.Content.StartsWith (Trigger)) { // Check if the message content fits the trigger, or if there is no trigger.
                    if (UserId == 0 || UserId == message.Author.Id) { // Check if there is a required user, and if it is the correct user.
                        if (ChannelId == 0 || ChannelId == message.Channel.Id) { // Ditto, but for channels.
                            if (new Random ().NextDouble () * 100d < Chance) {
                                return (Response, Emoji);
                            }
                        }
                    }
                }

                return (null, null);
            }

            public string ToString (GuildHandler guildHandler)
            {
                return $"Trigger: {Trigger}, " +
                    "User: " + (guildHandler.GetUser(UserId) == null ? "Any" : guildHandler.GetUser(UserId).GetShownName()) + ", " +
                    "Channel: " + (guildHandler.GetChannel(ChannelId) == null ? "Any" : guildHandler.GetChannel(ChannelId).Name) + ", " +
                    $"Chance: {Math.Round(Chance, 2)}%, Response: {Response}, Emote: {Emoji}";
            }
        }
    }
}
