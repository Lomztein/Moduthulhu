using Discord;
using Discord.WebSocket;
using Lomztein.ModularDiscordBot.Core.Configuration;
using Lomztein.ModularDiscordBot.Core.IO;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Lomztein.ModularDiscordBot.Modules.CommandRoot;
using Lomztein.ModularDiscordBot.Modules.Misc.Karma.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Karma
{
    public class KarmaModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Karma";
        public override string Description => "Keep track of an accurate representation of peoples self-worth.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        public override string [ ] RequiredModules => new string [ ] { "Lomztein_Command Root" };

        private MultiEntry<ulong> upvoteEmoteId;
        private MultiEntry<ulong> downvoteEmoteId;

        private Dictionary<ulong, int> karma;

        private KarmaCommand karmaCommand = new KarmaCommand ();

        public void Configure() {
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();
            upvoteEmoteId = Configuration.GetEntries (guilds, "UpvoteEmoteID", guilds.Select (x => {
                Emote emote = x.Emotes.FirstOrDefault (y => y.Name == "upvote");
                return emote != null ? emote.Id : 0;
            }));

            downvoteEmoteId = Configuration.GetEntries (guilds, "DownvoteEmoteID", guilds.Select (x => {
                Emote emote = x.Emotes.FirstOrDefault (y => y.Name == "downvote");
                return emote != null ? emote.Id : 0;
            }));
        }

        public override void Initialize() {
            ParentBotClient.discordClient.ReactionAdded += OnReactionAdded;
            ParentBotClient.discordClient.ReactionRemoved += OnReactionRemoved;
            karmaCommand.parentModule = this;
            ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot.AddCommands (karmaCommand);
            LoadKarma ();
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            OnReactionChanged (arg1, arg3, -1);
            return Task.CompletedTask;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            OnReactionChanged (arg1, arg3, 1);
            return Task.CompletedTask;
        }

        private async void OnReactionChanged(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction, int direction) {
            IUserMessage message = await cache.DownloadAsync ();
            
            if (message == null)
                return;

            if (reaction.Channel is SocketGuildChannel guildChannel && reaction.Emote is Emote emote) {
                if (emote.Id == upvoteEmoteId.GetEntry (guildChannel.Guild)) {
                    ChangeKarma (reaction.User.Value, message.Author, direction * 1);
                }

                if (emote.Id == downvoteEmoteId.GetEntry (guildChannel.Guild)) {
                    ChangeKarma (reaction.User.Value, message.Author, direction * -1);
                }
            }
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.ReactionAdded -= OnReactionAdded;
            ParentBotClient.discordClient.ReactionRemoved -= OnReactionRemoved;
            ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot.RemoveCommands (karmaCommand);
        }

        private void LoadKarma () {
            karma = DataSerialization.DeserializeData<Dictionary<ulong, int>> ("Karma");
            if (karma == null)
                karma = new Dictionary<ulong, int> ();
        }

        private void SaveKarma () {
            DataSerialization.SerializeData (karma, "Karma");
        }

        private void ChangeKarma (IUser giver, IUser reciever, int change) {
            if (giver.Id == reciever.Id)
                return; // Can't go around giving yourself karma, ye twat.
            if (!karma.ContainsKey (reciever.Id))
                karma.Add (reciever.Id, 0);

            karma [ reciever.Id ] += change;
            SaveKarma ();
        }

        public int GetKarma (ulong userID) {
            return karma.GetValueOrDefault (userID);
        }
    }
}
