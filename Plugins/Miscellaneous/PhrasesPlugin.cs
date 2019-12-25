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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Modules.Phrases
{
    [Descriptor ("Lomztein", "Response Phrases", "Responds to certain phrases from certain users in certain channels with a certain chance.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Miscellaneous/PhrasesPlugin.cs")]
    [GDPR (GDPRCompliance.Partial, "Other users may create phrases for each other, thus storing each others ID in a phrase.")]
    public class PhrasesPlugin : PluginBase {

        private CachedValue<List<Phrase>> _phrases;

        public override void Initialize() {
            GuildHandler.MessageReceived += OnMessageRecieved;
            _phrases = GetConfigCache("Phrases", x => new List<Phrase>());

            #region
            AddConfigInfo("Add Phrase", "Add empty phrase.", () => { _phrases.GetValue().Add(new Phrase()); _phrases.Store(); }, () => $"Added new empty response phrase at index {_phrases.GetValue ().Count - 1}. You must edit it using the other config options available here.");
            AddConfigInfo<int>("Remove Phrase", "Remove phrase.", x => { _phrases.GetValue().RemoveAt(x); _phrases.Store(); }, x => $"Removed response phrase at the index {x}.", "Index");
            AddConfigInfo("List Phrases", "List current phrases.", () => "Current phrases:\n" + string.Join('\n', _phrases.GetValue().Select(x => _phrases.GetValue ().IndexOf (x) + " -> " + x.ToString (GuildHandler))));
            
            AddConfigInfo("Set Phrase Trigger", "Set trigger", () => { _phrases.GetValue().LastOrDefault().Trigger = string.Empty; _phrases.Store(); }, () => $"Most recent phrase trigger reset.");
            AddConfigInfo<string>("Set Phrase Trigger", "Set trigger", y => { _phrases.GetValue().LastOrDefault().Trigger = y; _phrases.Store(); }, y => $"Most recent phrase trigger set to {y}.", "Trigger");
            AddConfigInfo<int>("Set Phrase Trigger", "Set trigger", x => { _phrases.GetValue()[x].Trigger = string.Empty; _phrases.Store(); }, x => $"Phrase trigger at index {x} reset.", "Index");
            AddConfigInfo<int, string>("Set Phrase Trigger", "Set trigger", (x, y) => { _phrases.GetValue()[x].Trigger = y; _phrases.Store(); }, (x, y) => $"Phrase trigger at index {x} updated to '{y}'.", "Index", "Trigger");

            AddConfigInfo("Set Phrase User", "Set user", () => { _phrases.GetValue().LastOrDefault ().UserId = 0; _phrases.Store(); }, () => $"Most recent phrase user reset.");
            AddConfigInfo<SocketGuildUser>("Set Phrase User", "Set user", y => { _phrases.GetValue().LastOrDefault ().UserId = y.Id; _phrases.Store(); }, x => $"Most recent phrase user updated to '{x.GetShownName ()}'.", "User");
            AddConfigInfo<string>("Set Phrase User", "Set user", y => { _phrases.GetValue().LastOrDefault ().UserId = GuildHandler.GetUser(y).Id; _phrases.Store(); }, y => $"Most recent phrase user updated to '{GuildHandler.GetUser (y).GetShownName ()}'.", "Username");
            AddConfigInfo<ulong>("Set Phrase User", "Set user", y => { _phrases.GetValue().LastOrDefault ().UserId = y; _phrases.Store(); }, y => $"Most recent phrase user updated to '{GuildHandler.GetUser (y).GetShownName ()}.", "User ID");

            AddConfigInfo<int>("Set Phrase User", "Set user", x => { _phrases.GetValue()[x].UserId = 0; _phrases.Store(); }, x => $"Phrase user at index {x} reset.", "Index");
            AddConfigInfo<int, SocketGuildUser>("Set Phrase User", "Set user", (x, y) => { _phrases.GetValue()[x].UserId = y.Id; _phrases.Store(); }, (x, y) => $"Phrase user at index {x} updated to '{y.GetShownName()}'.", "Index", "User");
            AddConfigInfo<int, string>("Set Phrase User", "Set user", (x, y) => { _phrases.GetValue()[x].UserId = GuildHandler.FindUser (y).Id; _phrases.Store(); }, (x, y) => $"Phrase user at index {x} updated to '{GuildHandler.GetUser (y).GetShownName ()}'.", "Index", "Username");
            AddConfigInfo<int, ulong>("Set Phrase User", "Set user", (x, y) => { _phrases.GetValue()[x].UserId = y; _phrases.Store(); }, (x, y) => $"Phrase user at index {x} updated to '{GuildHandler.GetUser(y).GetShownName ()}'.", "Index", "User ID");

            AddConfigInfo("Set Phrase Channel", "Set channel", () => { _phrases.GetValue().LastOrDefault ().ChannelId = 0; _phrases.Store(); }, () => $"Most recent phrase channel reset.");
            AddConfigInfo<SocketTextChannel>("Set Phrase Channel", "Set channel", y => { _phrases.GetValue().LastOrDefault ().ChannelId = y.Id; _phrases.Store(); }, x => $"Most recent phrase channel updated to '{x.Name}'.", "Channel");
            AddConfigInfo<string>("Set Phrase Channel", "Set channel", y => { _phrases.GetValue().LastOrDefault().ChannelId = GuildHandler.GetTextChannel(y).Id; _phrases.Store(); }, x => $"Most recent phrase channel updated to '{GuildHandler.GetTextChannel (x).Name}'.", "Channel Name");
            AddConfigInfo<ulong>("Set Phrase Channel", "Set channel", y => { _phrases.GetValue().LastOrDefault().ChannelId = y; _phrases.Store(); }, x => $"Most recent phrase channel updated to '{GuildHandler.GetTextChannel (x).Name}'.", "Channel ID");

            AddConfigInfo<int>("Set Phrase Channel", "Set channel", x => { _phrases.GetValue()[x].ChannelId = 0; _phrases.Store(); }, x => $"Phrase channel at index {x} reset.", "Index");
            AddConfigInfo<int, SocketTextChannel>("Set Phrase Channel", "Set channel", (x, y) => { _phrases.GetValue()[x].ChannelId = y.Id; _phrases.Store(); }, (x, y) => $"Phrase channel at index {x} updated to '{y.Name}'.", "Index", "Channel");
            AddConfigInfo<int, string>("Set Phrase Channel", "Set channel", (x, y) => { _phrases.GetValue()[x].ChannelId = GuildHandler.GetTextChannel (y).Id; _phrases.Store(); }, (x, y) => $"Phrase channel at index {x} updated to '{GuildHandler.GetTextChannel(y).Name}'.", "Index", "Channel Name");
            AddConfigInfo<int, ulong>("Set Phrase Channel", "Set channel", (x, y) => { _phrases.GetValue()[x].ChannelId = y; _phrases.Store(); }, (x, y) => $"Phrase channel at index {x} '{GuildHandler.GetTextChannel (y)}'.", "Index", "Channel ID");

            AddConfigInfo("Set Phrase Chance", "Set chance", () => { _phrases.GetValue().LastOrDefault ().Chance = 0; _phrases.Store(); }, () => $"Most recent chance reset.");
            AddConfigInfo<double>("Set Phrase Chance", "Set chance", y => { _phrases.GetValue().LastOrDefault ().Chance = Math.Clamp(y, 0d, 100d); _phrases.Store(); }, y => $"Most recent phrase chance updated to {y}.", "Chance");
            AddConfigInfo("Set Phrase Response", "Set response", () => { _phrases.GetValue().LastOrDefault().Response = string.Empty; _phrases.Store(); }, () => $"Most recent phrase response reset.");
            AddConfigInfo<string>("Set Phrase Response", "Set response", y => { _phrases.GetValue().LastOrDefault().Response = y; _phrases.Store(); }, x => $"Most recent phrase response updated {x}.", "Response");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", () => { _phrases.GetValue().LastOrDefault().Emoji = string.Empty; _phrases.Store(); }, () => $"Most recent phrase emoji reset.");
            AddConfigInfo<string>("Set Phrase Emoji", "Set emoji", y => { _phrases.GetValue().LastOrDefault().Emoji = y; _phrases.Store(); }, x => $"Most recent phrase emoji updated {x}.", "Emoji");

            AddConfigInfo<int>("Set Phrase Chance", "Set chance", x => { _phrases.GetValue()[x].Chance = 100; _phrases.Store(); }, x => $"Phrase at index {x} chance reset.", "Index");
            AddConfigInfo<int, double>("Set Phrase Chance", "Set chance", (x, y) => { _phrases.GetValue()[x].Chance = Math.Clamp (y, 0d, 100d); _phrases.Store(); }, (x, y) => $"Phrase at index {x} chance updated to {y}.", "Index", "Chance");
            AddConfigInfo<int>("Set Phrase Response", "Set response", x => { _phrases.GetValue()[x].Response = string.Empty; _phrases.Store(); }, x => $"Phrase at index {x} response reset.", "Index");
            AddConfigInfo<int, string>("Set Phrase Response", "Set response", (x, y) => { _phrases.GetValue()[x].Response = y; _phrases.Store(); }, (x, y) => $"Phrase at index {x} response updated to {y}.", "Index", "Response");
            AddConfigInfo<int>("Set Phrase Emoji", "Set emoji", x => { _phrases.GetValue()[x].Emoji = string.Empty; _phrases.Store(); }, x => $"Phrase at index {x} emoji reset.", "Index");
            AddConfigInfo<int, string>("Set Phrase Emoji", "Set emoji", (x, y) => { _phrases.GetValue()[x].Emoji = y; _phrases.Store(); }, (x, y) => $"Phrase at index {x} emoji updated to {y}.", "Index", "Emoji");
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

        public override JToken RequestUserData(ulong id)
        {
            var phrases = _phrases.GetValue().Where(x => x.UserId == id);
            if (phrases.Count () > 0)
            {
                return JArray.FromObject(phrases.ToArray());
            }
            return null;
        }

        public override void DeleteUserData(ulong id)
        {
            if (_phrases.GetValue ().Any (x => x.UserId == id))
            {
                _phrases.MutateValue(x => x.RemoveAll(y => y.UserId == id));
            }
        }

        public class Phrase {

            [JsonProperty ("Trigger")]
            public string Trigger { get; set; } = string.Empty;
            [JsonProperty ("UserId")]
            public ulong UserId { get; set; }
            [JsonProperty ("ChannelId")]
            public ulong ChannelId { get; set; }
            [JsonProperty ("Chance")]
            public double Chance { get; set; } = 100d;

            [JsonProperty ("Response")]
            public string Response { get; set; } = string.Empty;
            [JsonProperty ("Emoji")]
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
                    "User: " + (guildHandler.FindUser(UserId) == null ? "Any" : guildHandler.GetUser(UserId).GetShownName()) + ", " +
                    "Channel: " + (guildHandler.FindTextChannel(ChannelId) == null ? "Any" : guildHandler.FindTextChannel(ChannelId).Name) + ", " +
                    $"Chance: {Math.Round(Chance, 2)}%, Response: {Response}, Emote: {Emoji}";
            }
        }
    }
}
