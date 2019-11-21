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

            #region
            AddConfigInfo("Add Phrase", "Add empty phrase.", new Action(() => { _phrases.GetValue().Add(new Phrase()); _phrases.Store(); }), () => $"Added new empty response phrase at index {_phrases.GetValue ().Count - 1}. You must edit it using the other config options available here.");
            AddConfigInfo("Remove Phrase", "Remove phrase.", new Action<int>((x) => { _phrases.GetValue().RemoveAt(x); _phrases.Store(); }), () => $"Removed response phrase at the given index.", "Index");
            AddConfigInfo("List Phrases", "List current phrases.", () => "Current phrases:\n" + string.Join('\n', _phrases.GetValue().Select(x => _phrases.GetValue ().IndexOf (x) + " -> " + PhraseToString (x))));
            
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action(() => { _phrases.GetValue().LastOrDefault().triggerPhrase = string.Empty; _phrases.Store(); }), () => $"Phrase trigger reset.");
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().triggerPhrase = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Trigger");
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<int>((x) => { _phrases.GetValue()[x].triggerPhrase = string.Empty; _phrases.Store(); }), () => $"Phrase trigger reset.", "Index");
            AddConfigInfo("Set Phrase Trigger", "Set trigger", new Action<int, string>((x, y) => { _phrases.GetValue()[x].triggerPhrase = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Index", "Trigger");

            AddConfigInfo("Set Phrase User", "Set user", new Action(() => { _phrases.GetValue().LastOrDefault ().userID = 0; _phrases.Store(); }), () => $"Phrase user reset.", "Index", "User");
            AddConfigInfo("Set Phrase User", "Set user", new Action<SocketGuildUser>((y) => { _phrases.GetValue().LastOrDefault ().userID = y.Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User");
            AddConfigInfo("Set Phrase User", "Set user", new Action<string>((y) => { _phrases.GetValue().LastOrDefault ().userID = GuildHandler.FindUser(y).Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "Username");
            AddConfigInfo("Set Phrase User", "Set user", new Action<ulong>((y) => { _phrases.GetValue().LastOrDefault ().userID = y; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User ID");

            AddConfigInfo("Set Phrase User", "Set user", new Action<int>((x) => { _phrases.GetValue()[x].userID = 0; _phrases.Store(); }), () => $"Phrase user reset.", "Index");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, SocketGuildUser>((x, y) => { _phrases.GetValue()[x].userID = y.Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, string>((x, y) => { _phrases.GetValue()[x].userID = GuildHandler.FindUser (y).Id; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "Username");
            AddConfigInfo("Set Phrase User", "Set user", new Action<int, ulong>((x, y) => { _phrases.GetValue()[x].userID = y; _phrases.Store(); }), () => $"Phrase user updated.", "Index", "User ID");

            AddConfigInfo("Set Phrase Channel", "Set channel", new Action(() => { _phrases.GetValue().LastOrDefault ().channelID = 0; _phrases.Store(); }), () => $"Phrase channel reset.");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<SocketTextChannel>((y) => { _phrases.GetValue().LastOrDefault ().channelID = y.Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Channel");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().channelID = GuildHandler.FindTextChannel(y).Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Channel Name");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<ulong>((y) => { _phrases.GetValue().LastOrDefault().channelID = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Channel ID");

            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int>((x) => { _phrases.GetValue()[x].channelID = 0; _phrases.Store(); }), () => $"Phrase channel reset.", "Index");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, SocketTextChannel>((x, y) => { _phrases.GetValue()[x].channelID = y.Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Index", "Channel");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, string>((x, y) => { _phrases.GetValue()[x].channelID = GuildHandler.FindTextChannel (y).Id; _phrases.Store(); }), () => $"Phrase channel updated.", "Index", "Channel Name");
            AddConfigInfo("Set Phrase Channel", "Set channel", new Action<int, ulong>((x, y) => { _phrases.GetValue()[x].channelID = y; _phrases.Store(); }), () => $"Phrase trigger updated.", "Index", "Channel ID");

            AddConfigInfo("Set Phrase Chance", "Set chance", new Action(() => { _phrases.GetValue().LastOrDefault ().chance = 0; _phrases.Store(); }), () => $"Phrase chance reset.");
            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<double>((y) => { _phrases.GetValue().LastOrDefault ().chance = Math.Clamp(y, 0d, 100d); _phrases.Store(); }), () => $"Phrase chance updated.", "Chance");
            AddConfigInfo("Set Phrase Response", "Set response", new Action(() => { _phrases.GetValue().LastOrDefault().response = string.Empty; _phrases.Store(); }), () => $"Phrase response reset.");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().response = y; _phrases.Store(); }), () => $"Phrase response updated.", "Response");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action(() => { _phrases.GetValue().LastOrDefault().emoji = string.Empty; _phrases.Store(); }), () => $"Phrase Emoji reset.");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<string>((y) => { _phrases.GetValue().LastOrDefault().emoji = y; _phrases.Store(); }), () => $"Phrase Emoji updated.", "Emoji");

            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<int>((x) => { _phrases.GetValue()[x].chance = 100; _phrases.Store(); }), () => $"Phrase chance reset.", "Index");
            AddConfigInfo("Set Phrase Chance", "Set chance", new Action<int, double>((x, y) => { _phrases.GetValue()[x].chance = Math.Clamp (y, 0d, 100d); _phrases.Store(); }), () => $"Phrase chance updated.", "Index", "Chance");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<int>((x) => { _phrases.GetValue()[x].response = string.Empty; _phrases.Store(); }), () => $"Phrase response reset.", "Index");
            AddConfigInfo("Set Phrase Response", "Set response", new Action<int, string>((x, y) => { _phrases.GetValue()[x].response = y; _phrases.Store(); }), () => $"Phrase response updated.", "Index", "Response");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<int>((x) => { _phrases.GetValue()[x].emoji = string.Empty; _phrases.Store(); }), () => $"Phrase Emoji reset.", "Index");
            AddConfigInfo("Set Phrase Emoji", "Set emoji", new Action<int, string>((x, y) => { _phrases.GetValue()[x].emoji = y; _phrases.Store(); }), () => $"Phrase Emoji updated.", "Index", "Emoji");
            #endregion
        }

        private async Task OnMessageRecieved(SocketMessage message) {
            await CheckAndRespond (message);
        }

        private async Task CheckAndRespond(SocketMessage message)
        {
            string response = null;
            IEmote emote = null;

            foreach (Phrase phrase in _phrases.GetValue ())
            {
                (response, emote) = phrase.CheckAndReturnResponse(message as SocketUserMessage);
                if (response != null || emote?.Name != null)
                    break;
            }

            if (!string.IsNullOrEmpty(response))
                await MessageControl.SendMessage(message.Channel as ITextChannel, response);
            if (!string.IsNullOrEmpty(emote?.Name))
                await (message as SocketUserMessage).AddReactionAsync(emote);
        }

        public override void Shutdown() {
            GuildHandler.MessageReceived -= OnMessageRecieved;
        }

        private string PhraseToString (Phrase phrase)
        {
            return $"Trigger: {phrase.triggerPhrase}, " +
                "User: " + (GuildHandler.GetUser (phrase.userID) == null ? "Any, " : GuildHandler.GetUser (phrase.userID).GetShownName ()) + ", " +
                "Channel: " + (GuildHandler.GetChannel(phrase.channelID) == null ? "Any, " : GuildHandler.GetChannel(phrase.channelID).Name) + ", " +
                $"Chance: {Math.Round (phrase.chance, 2)}%, Response: {phrase.response}, Emote: {phrase.emoji}";
        }

        public class Phrase {

            public string triggerPhrase = string.Empty;
            public ulong userID = 0;
            public ulong channelID = 0;
            public double chance = 100d;

            public string response = string.Empty;
            public string emoji = string.Empty;

            public (string res, IEmote emo) CheckAndReturnResponse (SocketUserMessage message) {
                if (string.IsNullOrEmpty (triggerPhrase) || message.Content.StartsWith (triggerPhrase)) { // Check if the message content fits the trigger, or if there is no trigger.
                    if (userID == 0 || userID == message.Author.Id) { // Check if there is a required user, and if it is the correct user.
                        if (channelID == 0 || channelID == message.Channel.Id) { // Ditto, but for channels.
                            if (new Random ().NextDouble () * 100d < chance) {
                                return (response, Emote.Parse (emoji));
                            }
                        }
                    }
                }

                return (null, null);
            }
        }
    }
}
