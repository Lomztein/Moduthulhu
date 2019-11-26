using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Modules.Misc.Karma.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Misc.Karma
{
    [Dependency ("Lomztein-Command Root")]
    [Descriptor ("Lomztein", "Karma", "Keep track of an accurate representation of peoples self-worth.")]
    public class KarmaPlugin : PluginBase {

        private CachedValue<ulong> _upvoteEmoteId;
        private CachedValue<ulong> _downvoteEmoteId;

        private CachedValue<Dictionary<ulong, Selfworth>> _karma;

        private KarmaCommand karmaCommand = new KarmaCommand ();

        public override void Initialize() {
            GuildHandler.ReactionAdded += OnReactionAdded;
            GuildHandler.ReactionRemoved += OnReactionRemoved;
            SendMessage("Lomztein-Command Root", "AddCommand", karmaCommand);
            karmaCommand.ParentPlugin = this;

            _upvoteEmoteId = GetConfigCache("UpvoteEmoteId", x => x.GetGuild ().Emotes.Where (y => y.Name == "upvote").FirstOrDefault ().ZeroIfNull ());
            _downvoteEmoteId = GetConfigCache("DownvoteEmoteId", x => x.GetGuild ().Emotes.Where (y => y.Name == "downvote").FirstOrDefault ().ZeroIfNull ());

            _karma = GetDataCache("Karma", x => new Dictionary<ulong, Selfworth>());

            AddConfigInfo("Set Upvote Emote", "Get emote", () => $"Current upvote emote is '{GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result.Name}'.");
            AddConfigInfo("Set Upvote Emote", "Set emote", new Action<string>(x => _upvoteEmoteId.SetValue((GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result?.Id).GetValueOrDefault())),
                 () => $"Set upvote emote to '{GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result?.Name}'.", "Emote");
            AddConfigInfo("Set Downvote Emote", "Get emote", () => $"Current downvote emote is '{GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result.Name}'.");
            AddConfigInfo("Set Downvote Emote", "Set emote", new Action<string>(x => _downvoteEmoteId.SetValue((GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result?.Id).GetValueOrDefault())),
                () => $"Set downote emote to '{GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result?.Name}'.", "Emote");
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            await OnReactionChanged (arg1, arg3, -1);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            await OnReactionChanged (arg1, arg3, 1);
        }

        private async Task OnReactionChanged(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction, int direction) {
            IUserMessage message = await cache.DownloadAsync ();
            
            if (message == null)
                return;

            if (reaction.Channel is SocketGuildChannel guildChannel && reaction.Emote is Emote emote) {

                if (emote.Id == _upvoteEmoteId.GetValue ()) {
                    ChangeKarma (reaction.User.Value, message.Author, direction * 1);
                }

                if (emote.Id == _downvoteEmoteId.GetValue ()) {
                    ChangeKarma (reaction.User.Value, message.Author, direction * -1);
                }
            }
        }

        public override void Shutdown() {
            GuildHandler.ReactionAdded -= OnReactionAdded;
            GuildHandler.ReactionRemoved -= OnReactionRemoved;
            SendMessage("Lomztein-Command Root", "RemoveCommand", karmaCommand);
        }

        public Dictionary<ulong, Selfworth> GetKarmaDictionary () => _karma.GetValue();

        private void ChangeKarma (IUser giver, IUser receiver, int direction) {
            if (giver.Id == receiver.Id)
                return; // Can't go around giving yourself karma, ye twat.
            if (!_karma.GetValue ().ContainsKey (receiver.Id))
                _karma.GetValue ().Add (receiver.Id, new Selfworth ());

            if (direction > 0)
                _karma.GetValue ()[ receiver.Id ].Upvote ();
            else if (direction < 0)
                _karma.GetValue ()[ receiver.Id ].Downvote ();

            _karma.Store();
        }

        public Selfworth GetKarma (ulong userID) {
            Selfworth result = _karma.GetValue ().GetValueOrDefault (userID);
            if (result == null)
                result = new Selfworth ();
            return result;
        }

        public class Selfworth {

            public int Upvotes;
            public int Downvotes;

            [JsonIgnore]
            public int Total { get =>  Upvotes - Downvotes; }

            public void Upvote() => Upvotes++;
            public void Downvote() => Downvotes++;

            public override string ToString() => $"{Total} (+{Upvotes} / -{Downvotes})";

        }
    }
}
