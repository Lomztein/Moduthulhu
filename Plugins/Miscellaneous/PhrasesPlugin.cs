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

namespace Lomztein.Moduthulhu.Modules.Phrases
{
    [Descriptor ("Lomztein", "Response Phrases", "Responds to certain phrases from certain users in certain channels with a certain chance.")]
    public class PhrasesPlugin : PluginBase {

        private CachedValue<List<Phrase>> _phrases;

        public override void Initialize() {
            GuildHandler.MessageReceived += OnMessageRecieved;
            _phrases = GetConfigCache("Phrases", x => new List<Phrase>());

            AddConfigInfo("Add Phrase", "Add empty phrase.", new Action(() => { _phrases.GetValue().Add(new Phrase()); _phrases.Store(); }), () => $"Added new empty response phrase at index {_phrases.GetValue ().Count - 1}. You must edit it using the other config options available here.");
            AddConfigInfo("Remove Phrase", "Remove phrase.", new Action<int>((x) => { _phrases.GetValue().RemoveAt(x); _phrases.Store(); }), () => $"Removed response phrase at the given index.", "Index");
            AddConfigInfo("List Phrases", "List current phrases.", () => "Current phrases:\n" + string.Join('\n', _phrases.GetValue().Select(x => _phrases.GetValue ().IndexOf (x) + " -> " + PhraseToString (x))));
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<int, string>((x, y) => { _phrases.GetValue()[x].triggerPhrase = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Index", "Trigger");

            AddConfigInfo("Set Phrase User", "Set user", new Action<int, SocketGuildUser>((x, y) => { _phrases.GetValue()[x].userID = y.Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, string>((x, y) => { _phrases.GetValue()[x].userID = GuildHandler.FindUser (y).Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "Username");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, ulong>((x, y) => { _phrases.GetValue()[x].userID = y; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User ID");

            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, SocketTextChannel>((x, y) => { _phrases.GetValue()[x].channelID = y.Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Index", "Channel");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, string>((x, y) => { _phrases.GetValue()[x].channelID = GuildHandler.FindTextChannel (y).Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Index", "Channel Name");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, ulong>((x, y) => { _phrases.GetValue()[x].channelID = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Index", "Channel ID");

            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<int, double>((x, y) => { _phrases.GetValue()[x].chance = Math.Clamp (x, 0d, 100d); _phrases.Store(); }), () => $"Phrase chance updated.", "Index", "Chance");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<int, string>((x, y) => { _phrases.GetValue()[x].response = y; _phrases.Store(); }), () => $"Phrase response updated.", "Index", "Response");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<int, string>((x, y) => { _phrases.GetValue()[x].emoji = y; _phrases.Store(); }), () => $"Phrase Emoji updated.", "Index", "Emoji");
        }

        private async Task OnMessageRecieved(SocketMessage message) {
            await CheckAndRespond (message);
        }

        private async Task CheckAndRespond(SocketMessage message)
        {
            string response = null;
            Emoji emoji = null;

            foreach (Phrase phrase in _phrases.GetValue ())
            {
                (response, emoji) = phrase.CheckAndReturnResponse(message as SocketUserMessage);
                if (response != null || emoji?.Name != null)
                    break;
            }

            if (!string.IsNullOrEmpty(response))
                await MessageControl.SendMessage(message.Channel as ITextChannel, response);
            if (!string.IsNullOrEmpty(emoji?.Name))
                await (message as SocketUserMessage).AddReactionAsync(emoji);
        }

        public override void Shutdown() {
            GuildHandler.MessageReceived -= OnMessageRecieved;
        }

        private string PhraseToString (Phrase phrase)
        {
            return $"Trigger: {phrase.triggerPhrase}, " +
                "User: " + (GuildHandler.GetUser (phrase.userID) == null ? "Unspecific, " : GuildHandler.GetUser (phrase.userID).GetShownName ()) +
                "Channel: " + (GuildHandler.GetUser(phrase.userID) == null ? "Unspecific, " : GuildHandler.GetUser(phrase.userID).GetShownName()) + "" +
                $"Chance: {Math.Round (phrase.chance, 2)}%, Response: {phrase.response}, Emoji: {phrase.emoji}";
        }

        public class Phrase {

            public string triggerPhrase = "";
            public ulong userID = 0;
            public ulong channelID = 0;
            public double chance = 100d;

            public string response = "";
            public string emoji = "";

            public (string res, Emoji emo) CheckAndReturnResponse (SocketUserMessage message) {
                if (string.IsNullOrEmpty (triggerPhrase) || message.Content.StartsWith (triggerPhrase)) { // Check if the message content fits the trigger, or if there is no trigger.
                    if (userID == 0 || userID == message.Author.Id) { // Check if there is a required user, and if it is the correct user.
                        if (channelID == 0 || channelID == message.Channel.Id) { // Ditto, but for channels.
                            if (new Random ().NextDouble () * 100d < chance) {
                                return (response, new Emoji (emoji));
                            }
                        }
                    }
                }

                return (null, null);
            }
        }
    }
}
