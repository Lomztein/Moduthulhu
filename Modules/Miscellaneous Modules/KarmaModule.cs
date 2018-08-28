using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.Misc.Karma.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Misc.Karma
{
    public class KarmaModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Karma";
        public override string Description => "Keep track of an accurate representation of peoples self-worth.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        public override string [ ] RequiredModules => new string [ ] { "Lomztein_Command Root" };

        [AutoConfig] private MultiEntry<ulong, SocketGuild> upvoteEmoteId = new MultiEntry<ulong, SocketGuild> (x => x.Emotes.FirstOrDefault (y => y.Name == "upvote").ZeroIfNull (), "UpvoteEmoteID", true);
        [AutoConfig] private MultiEntry<ulong, SocketGuild> downvoteEmoteId = new MultiEntry<ulong, SocketGuild> (x => x.Emotes.FirstOrDefault (y => y.Name == "downvote").ZeroIfNull (), "DownvoteEmoteID", true);

        private Dictionary<ulong, Selfworth> karma;

        private KarmaCommand karmaCommand = new KarmaCommand ();

        public override void Initialize() {
            ParentBotClient.discordClient.ReactionAdded += OnReactionAdded;
            ParentBotClient.discordClient.ReactionRemoved += OnReactionRemoved;
            karmaCommand.ParentModule = this;
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

                if (!this.IsConfigured (guildChannel.Id))
                    return;

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
            karma = DataSerialization.DeserializeData<Dictionary<ulong, Selfworth>> ("Karma");
            if (karma == null)
                karma = new Dictionary<ulong, Selfworth> ();
        }

        private void SaveKarma () {
            DataSerialization.SerializeData (karma, "Karma");
        }

        private void ChangeKarma (IUser giver, IUser reciever, int direction) {
            if (giver.Id == reciever.Id)
                return; // Can't go around giving yourself karma, ye twat.
            if (!karma.ContainsKey (reciever.Id))
                karma.Add (reciever.Id, new Selfworth ());

            if (direction > 0)
                karma [ reciever.Id ].Upvote ();
            else if (direction < 0)
                karma [ reciever.Id ].Downvote ();

            SaveKarma ();
        }

        public Dictionary<ulong, Selfworth> GetKarmaDictionary () {
            return karma;
        }

        public Selfworth GetKarma (ulong userID) {
            Selfworth result = karma.GetValueOrDefault (userID);
            if (result == null)
                result = new Selfworth ();
            return result;
        }

        public class Selfworth {

            public int upvotes;
            public int downvotes;

            [JsonIgnore]
            public int Total { get =>  upvotes - downvotes; }

            public void Upvote() => upvotes++;
            public void Downvote() => downvotes++;

            public override string ToString() => $"{Total} (+{upvotes} / -{downvotes})";

        }
    }
}
