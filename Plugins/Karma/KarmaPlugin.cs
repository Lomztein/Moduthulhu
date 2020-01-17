using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Modules.Misc.Karma.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Misc.Karma
{
    [Dependency ("Moduthulhu-Command Root")]
    [Descriptor ("Lomztein", "Karma", "Keep track of an accurate representation of peoples self-worth.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Karma")]
    [GDPR(GDPRCompliance.Partial, "Stores user ID automatically to keep track of user score.")]
    public class KarmaPlugin : PluginBase {

        private CachedValue<ulong> _upvoteEmoteId;
        private CachedValue<ulong> _downvoteEmoteId;

        private CachedValue<Dictionary<ulong, Selfworth>> _karma;

        private KarmaCommand _karmaCommand;

        public override void Initialize() {
            GuildHandler.ReactionAdded += OnReactionAdded;
            GuildHandler.ReactionRemoved += OnReactionRemoved;
            _karmaCommand = new KarmaCommand { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _karmaCommand);

            _upvoteEmoteId = GetConfigCache("UpvoteEmoteId", x => x.GetGuild ().Emotes.Where (y => y.Name == "upvote").FirstOrDefault ().ZeroIfNull ());
            _downvoteEmoteId = GetConfigCache("DownvoteEmoteId", x => x.GetGuild ().Emotes.Where (y => y.Name == "downvote").FirstOrDefault ().ZeroIfNull ());

            _karma = GetDataCache("Karma", x => new Dictionary<ulong, Selfworth>());

            AddConfigInfo("Set Upvote Emote", "Get emote", () => $"Current upvote emote is '{GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result.Name}'.");
            AddConfigInfo<string>("Set Upvote Emote", "Set emote", x => _upvoteEmoteId.SetValue((GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result?.Id).GetValueOrDefault()),
                 x => $"Set upvote emote to '{x}'.", "Emote");
            AddConfigInfo("Set Downvote Emote", "Get emote", () => $"Current downvote emote is '{GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result.Name}'.");
            AddConfigInfo<string>("Set Downvote Emote", "Set emote", x => _downvoteEmoteId.SetValue((GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result?.Id).GetValueOrDefault()),
                x => $"Set downote emote to '{x}'.", "Emote");

            AddGeneralFeaturesStateAttribute("Karma", "Tracking of total upvotes / downvotes per user.");
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
            {
                return;
            }

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
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _karmaCommand);
        }

        public override JToken RequestUserData(ulong id)
        {
            if (_karma.GetValue ().ContainsKey (id))
            {
                return new JObject
                {
                    { "Karma", JObject.FromObject (_karma.GetValue ()[id]) }
                };
            }
            return null;
        }

        public override void DeleteUserData(ulong id)
        {
            if (_karma.GetValue ().ContainsKey (id))
            {
                _karma.MutateValue(x => x.Remove(id));
            }
        }

        public Dictionary<ulong, Selfworth> GetKarmaDictionary () => _karma.GetValue();

        private void ChangeKarma (IUser giver, IUser receiver, int direction) {
            if (giver.Id == receiver.Id)
            {
                return; // Can't go around giving yourself karma, ye twat.
            }
            if (!_karma.GetValue ().ContainsKey (receiver.Id))
            {
                _karma.GetValue().Add(receiver.Id, new Selfworth());
            }

            if (direction > 0)
            {
                _karma.GetValue()[receiver.Id].Upvote();
            }
            else if (direction < 0)
            {
                _karma.GetValue()[receiver.Id].Downvote();
            }

            _karma.Store();
        }

        public Selfworth GetKarma (ulong userID) {
            Selfworth result = _karma.GetValue ().GetValueOrDefault (userID);
            if (result == null)
            {
                result = new Selfworth();
            }
            return result;
        }

        public class Selfworth {

            [JsonProperty ("Upvotes")]
            public int Upvotes { get; private set; }
            [JsonProperty ("Downvotes")]
            public int Downvotes { get; private set; }

            [JsonIgnore]
            public int Total { get =>  Upvotes - Downvotes; }

            public void Upvote() => Upvotes++;
            public void Downvote() => Downvotes++;

            public override string ToString() => $"{Total} (+{Upvotes} / -{Downvotes})";

        }
    }
}
