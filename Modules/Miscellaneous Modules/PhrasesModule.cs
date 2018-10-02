using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Configuration.Management.Converters;
using Lomztein.Moduthulhu.Core.Configuration.Management;

namespace Lomztein.Moduthulhu.Modules.Misc.Phrases
{
    public class PhrasesModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Response Phrases";
        public override string Description => "Responds to certain phrases from certain people in certain channels with a certain chance.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        [AutoConfig] private MultiEntry<List<Phrase>, SocketGuild> phrases = new MultiEntry<List<Phrase>, SocketGuild> (x => new List<Phrase> () { new Phrase (), new Phrase () }, "Phrases", true);

        public override void Initialize() {
            ParentShard.MessageReceived += OnMessageRecieved;
            ConfigurationManager.AddConverter (new PhraseConfigConverter ());
        }

        private Task OnMessageRecieved(SocketMessage message) {
            CheckAndRespond (message);
            return Task.CompletedTask;
        }

        private async void CheckAndRespond (SocketMessage message) {

            if (message is SocketUserMessage userMessage && userMessage.GetGuild () != null) {

                if (!this.IsConfigured (userMessage.GetGuild ().Id))
                    return;

                string response = null;
                Emoji emoji = null;

                foreach (Phrase phrase in phrases.GetEntry (userMessage.GetGuild ())) {
                    (response, emoji) = phrase.CheckAndReturnResponse (userMessage);
                    if (response != null || emoji?.Name != null)
                        break;
                }

                if (!string.IsNullOrEmpty (response))
                    await MessageControl.SendMessage (message.Channel as ITextChannel, response);
                if (!string.IsNullOrEmpty (emoji?.Name))
                    await userMessage.AddReactionAsync (emoji);
            }
        }

        public override void Shutdown() {
            ParentShard.MessageReceived -= OnMessageRecieved;
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

            public override string ToString() {
                return $"TRIGGER: {triggerPhrase}, USERID: {userID}, CHANNELID: {channelID}, CHANCE: {chance}, RESPONSE: {response}, EMOJI: {emoji}";
            }
        }

        public class PhraseConfigConverter : IConfigConverter {

            public Type TargetType => typeof (Phrase);

            public object Convert(Type targetType, params string[] input) {
                Phrase phrase = new Phrase () {
                    triggerPhrase = input[0],
                    userID = ulong.Parse (input[1]),
                    channelID = ulong.Parse (input[2]),
                    chance = double.Parse (input[3]),
                    response = input[4],
                    emoji = input[5]
                };
                return phrase;
            }
        }
    }
}
