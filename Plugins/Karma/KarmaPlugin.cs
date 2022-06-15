using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Karma.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    [Dependency ("Moduthulhu-Command Root")]
    [Descriptor ("Lomztein", "Karma", "Keep track of an accurate representation of peoples self-worth.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Karma")]
    [GDPR(GDPRCompliance.Partial, "Stores user ID automatically to keep track of user score.")]
    public class KarmaPlugin : PluginBase {

        private CachedValue<ulong> _upvoteEmoteId;
        private CachedValue<ulong> _downvoteEmoteId;

        private KarmaCommandSet _karmaCommand;
        private IKarmaRepository _karmaRepo = new CachedValueKarmaRepository ();

        public override void Initialize() {
            GuildHandler.ReactionAdded += OnReactionAdded;
            GuildHandler.ReactionRemoved += OnReactionRemoved;
            _karmaCommand = new KarmaCommandSet { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _karmaCommand);

            _upvoteEmoteId = GetConfigCache("UpvoteEmoteId", x => x.GetGuild ().Emotes.Where (y => y.Name == "upvote").FirstOrDefault ().ZeroIfNull ());
            _downvoteEmoteId = GetConfigCache("DownvoteEmoteId", x => x.GetGuild ().Emotes.Where (y => y.Name == "downvote").FirstOrDefault ().ZeroIfNull ());

            AddConfigInfo("Set Upvote Emote", "Get emote", () => $"Current upvote emote is '{GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result.Name}'.");
            AddConfigInfo<string>("Set Upvote Emote", "Set emote", x => _upvoteEmoteId.SetValue((GuildHandler.GetGuild().GetEmoteAsync(_upvoteEmoteId.GetValue()).Result?.Id).GetValueOrDefault()),
                 (success, x) => $"Set upvote emote to '{x}'.", "Emote");
            AddConfigInfo("Set Downvote Emote", "Get emote", () => $"Current downvote emote is '{GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result.Name}'.");
            AddConfigInfo<string>("Set Downvote Emote", "Set emote", x => _downvoteEmoteId.SetValue((GuildHandler.GetGuild().GetEmoteAsync(_downvoteEmoteId.GetValue()).Result?.Id).GetValueOrDefault()),
                (success, x) => $"Set downote emote to '{x}'.", "Emote");

            AddGeneralFeaturesStateAttribute("Karma", "Tracking of total upvotes / downvotes per user.");
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3) {
            await OnReactionChanged (arg1, arg3, -1);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3) {
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
                    VoteAction action = direction == 1 ? VoteAction.AddUpvote : VoteAction.RemoveUpvote;
                    ChangeKarma (reaction.User.Value, message.Channel, message, action);
                }

                if (emote.Id == _downvoteEmoteId.GetValue ()) {
                    VoteAction action = direction == 1 ? VoteAction.AddDownvote : VoteAction.RemoveDownvote;
                    ChangeKarma (reaction.User.Value, message.Channel, message, action);
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
            Karma karma = _karmaRepo.GetKarma(GuildHandler.GuildId, id);
            if (karma != null)
            {
                return new JObject
                {
                    { "Karma", JObject.FromObject (karma) }
                };
            }
            return null;
        }

        public override void DeleteUserData(ulong id)
        {
            // _karmaRepo.DeleteUserData(id);
        }

        private void ChangeKarma (IUser giver, IMessageChannel channel, IUserMessage message, VoteAction action) {
            if (giver.Id == message.Author.Id)
            {
                return; // Can't go around giving yourself karma, ye twat.
            }
            _karmaRepo.ChangeKarma(GuildHandler.GuildId, giver.Id, message.Author.Id, channel.Id, message.Id, action);
        }

        public Karma GetKarma (ulong userID) {
            Karma result = _karmaRepo.GetKarma(GuildHandler.GuildId, userID);
            if (result == null)
            {
                result = new Karma(userID);
            }
            return result;
        }

        public Karma[] GetLeaderboard() => _karmaRepo.GetLeaderboard(GuildHandler.GuildId);
    }
}
