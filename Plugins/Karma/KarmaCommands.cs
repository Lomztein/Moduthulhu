using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Plugins.Standard;
using System.Linq;
using System.Text;
using Lomztein.Moduthulhu.Plugins.Karma;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using System.Globalization;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Plugins.Karma.Commands
{
    public class KarmaCommandSet : PluginCommandSet<KarmaPlugin> {

        public KarmaCommandSet ()
        {
            Name = "karma";
            Description = "Measure self-worth.";
            Category = StandardCategories.Fun;
            _defaultCommand = new Me();
            _commandsInSet = new List<ICommand> {
                new Me (),
                new Top (),
                new Voters ((x, y) => y.Value - x.Value, x => x.GetUpvotes (), "Lovers", "upvotes", "lovely"),
                new Voters ((x, y) => y.Value - x.Value, x => x.GetDownvotes (), "Haters", "downvotes", "hateful"),
                new MessageCommand (),
            };
        }

        public class Me : PluginCommand<KarmaPlugin>
        {
            public Me()
            {
                Name = "me";
                Description = "Shows karma.";
                Category = StandardCategories.Fun;
            }

            [Overload(typeof(int), "Returns your own karma.")]
            public async Task<Result> Execute(ICommandMetadata data)
            {
                return await Execute(data, data.Author);
            }

            [Overload(typeof(int), "Returns karma of a given user.")]
            public async Task<Result> Execute(ICommandMetadata data, IUser user)
            {
                var karma = ParentPlugin.GetKarma(user.Id);
                return new Result(await GetKarmaEmbed(karma, user.GetShownName(), 3, ParentPlugin.GuildHandler), string.Empty);
            }

            [Overload(typeof(int), "Returns karma of a given user.")]
            public async Task<Result> Execute(ICommandMetadata data, string username)
            {
                IUser user = ParentPlugin.GuildHandler.GetUser(username);
                var karma = ParentPlugin.GetKarma(user.Id);
                return new Result(await GetKarmaEmbed(karma, user.GetShownName(), 3, ParentPlugin.GuildHandler), string.Empty);
            }

            [Overload(typeof(int), "Returns karma of a given user.")]
            public async Task<Result> Execute(ICommandMetadata data, ulong userId)
            {
                var karma = ParentPlugin.GetKarma(userId);
                return new Result(await GetKarmaEmbed(karma, ParentPlugin.GuildHandler.GetUser (userId).GetShownName(), 3, ParentPlugin.GuildHandler), string.Empty);
            }

            [Overload(typeof(SocketGuildUser[]), "Returns top <n> karma whores.")]
            public Task<Result> Execute(ICommandMetadata data, int amount)
            {
                var allKarma = ParentPlugin.GetLeaderboard();
                return TaskResult (GetLeaderboardEmbed(allKarma, (x, y) => y.Total - x.Total, x => ParentPlugin.GuildHandler.FindUser (x.UserId),
                    x => x.ToString (), "No one has yet to recieve any karma :(", "Karma Leaderboard", "Top [amount] of karma hoarders are..",
                    amount), string.Empty);
            }

            private static async Task<Embed> GetKarmaEmbed(Karma karma, string username, int topCount, GuildHandler messageSource)
            {
                if (karma.Upvotes == 0 && karma.Downvotes == 0)
                {
                    return new EmbedBuilder().WithTitle(":(").WithDescription($"{username} has yet to recieve any karma.").Build();
                }

                EmbedBuilder builder = new EmbedBuilder().
                    WithTitle($"Karma for {username}").
                    WithDescription($"```{karma.Upvotes} upvotes.\n{karma.Downvotes} downvotes.\n{karma.Total} total.```");



                builder.AddField(await GetTopMessage ($"Top [count] message", (x, y) => y.Total - x.Total, karma.GetMessages (), topCount, messageSource));
                builder.AddField(await GetTopMessage ($"Buttom [count] message", (x, y) => x.Total - y.Total, karma.GetMessages (), topCount, messageSource));

                builder.AddField(GetTopVoters((x, y) => y - x, karma.GetUpvotes(), $"{username} is most loved by..", "upvotes", "```No one :(```", topCount, messageSource));
                builder.AddField(GetTopVoters((x, y) => y - x, karma.GetDownvotes (), $"{username} is most hated by..", "downvotes", "```No one! :)```", topCount, messageSource));

                return builder.Build();
            }

            private static EmbedFieldBuilder GetTopVoters(Func<int, int, int> comparer, ulong[] voters, string title, string type, string ifNone, int topCount, GuildHandler guildHandler)
            {
                var sorted = voters.Distinct().ToList();
                var dict = sorted.ToDictionary(x => x, x => voters.Count (y => x == y)).ToList ();
                dict.Sort(new Comparison<KeyValuePair<ulong, int>> ((x, y) => comparer(x.Value, y.Value)));

                int count = sorted.Count();
                var voterList = dict.ToList().GetRange(0, Math.Min(count, topCount));

                StringBuilder top = new StringBuilder("```");
                foreach (var upvote in voterList)
                {
                    IUser user = guildHandler.FindUser(upvote.Key);
                    string name = user == null ? "*User not found." : user.GetShownName();
                    top.AppendLine($"{name} - {upvote.Value} {type}.");
                }
                top.AppendLine("```");

                return new EmbedFieldBuilder().WithName(title).WithValue(count == 0 ? ifNone : top.ToString());
            }

            private static async Task<EmbedFieldBuilder> GetTopMessage (string name, Func<Message, Message, int> comparer, Message[] messages, int topCount, GuildHandler messageSource)
            {
                var list = messages.ToList();
                list.Sort(new Comparison<Message>((x, y) => comparer(x, y)));
                var sortedMessages = list.GetRange(0, Math.Min(messages.Length, topCount));
                StringBuilder topMessages = new StringBuilder();
                foreach (var message in sortedMessages)
                {
                    IMessage userMessage = await messageSource.GetTextChannel(message.ChannelId)?.GetMessageAsync(message.Id);
                    if (userMessage == null)
                    {
                        topMessages.AppendLine($"*Message or channel deleted.* - {message}");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty (userMessage.Content))
                        {
                            topMessages.AppendLine($"> [*Embed*]({userMessage.GetJumpUrl()}) - {message}");
                        }
                        else
                        {
                            topMessages.AppendLine($"> [{userMessage.Content.Replace('\n', ' ')}]({userMessage.GetJumpUrl()}) - {message}");
                        }
                    }
                }
                return new EmbedFieldBuilder().WithName(name.Replace ("[count]", sortedMessages.Count.ToString ())).WithValue(topMessages.ToString());
            }
        }

        public class Top : PluginCommand<KarmaPlugin>
        {
            public Top()
            {
                Name = "top";
                Description = "Display leaderboard";
                Aliases = new[] { "leaderboard" };
                Category = StandardCategories.Fun;
            }

            [Overload(typeof(SocketGuildUser[]), "Returns top karma leaderboard.")]
            public Task<Result> Execute(ICommandMetadata data)
            {
                var allKarma = ParentPlugin.GetLeaderboard();
                return TaskResult(GetLeaderboardEmbed(allKarma, (x, y) => y.Total - x.Total, x => ParentPlugin.GuildHandler.FindUser(x.UserId),
                    x => x.ToString(), "No one has yet to recieve any karma :(", "Karma Leaderboard", "Top karma hoarders are..",
                    allKarma.Length), string.Empty);
            }

            [Overload(typeof(SocketGuildUser[]), "Returns top <n> karma whores.")]
            public Task<Result> Execute(ICommandMetadata data, int amount)
            {
                var allKarma = ParentPlugin.GetLeaderboard();
                return TaskResult(GetLeaderboardEmbed(allKarma, (x, y) => y.Total - x.Total, x => ParentPlugin.GuildHandler.FindUser(x.UserId),
                    x => x.ToString(), "No one has yet to recieve any karma :(", "Karma Leaderboard", "Top [amount] of karma hoarders are..",
                    amount), string.Empty);
            }
        }

        public class Voters : PluginCommand<KarmaPlugin>
        {
            public Voters(Func<KeyValuePair<ulong, int>, KeyValuePair<ulong, int>, int> sortFunc, Func<Message, ulong[]> voteSelector, string type, string voteType, string adjective)
            {
                Name = type.ToLowerInvariant ();
                Description = $"Get top {type.ToLowerInvariant ()}s";
                Category = StandardCategories.Fun;

                _sortFunc = sortFunc;
                _voteSelector = voteSelector;
                _adjective = adjective;
                _type = type;
                _voteType = voteType;
            }

            private Func<KeyValuePair<ulong, int>, KeyValuePair<ulong, int>, int> _sortFunc;
            private Func<Message, ulong[]> _voteSelector;
            private string _adjective;
            private string _type;
            private string _voteType;

            [Overload(typeof(LargeEmbed), "Display top voters.")]
            public Task<Result> Execute(ICommandMetadata data)
            {
                var allKarma = ParentPlugin.GetLeaderboard();
                var allVotes = allKarma.SelectMany(x => x.GetMessages()).SelectMany (x => _voteSelector (x));
                var groups = allVotes.GroupBy(x => x);
                var dict = groups.ToDictionary(x => x.Key, x => allVotes.Count (y => x.Key == y));
                var users = dict.Keys.ToArray();

                return TaskResult(GetLeaderboardEmbed(users, (x, y) => dict[y] - dict[x], x => ParentPlugin.GuildHandler.FindUser(x),
                    x => dict[x].ToString() + " " + _voteType, $"No one has yet to be {_adjective}", $"{_type} Leaderboard", $"The most {_adjective} people on this server are..",
                    dict.Count), string.Empty);
            }
        }

        public class MessageCommand : PluginCommand<KarmaPlugin>
        {
            public MessageCommand ()
            {
                Name = "message";
                Description = "Check message voters";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (Embed), "Show all who downvoted / upvoted a specific message in the current channel.")]
            public Task<Result> Execute (ICommandMetadata data, ulong id)
            {
                return Execute(data, data.ChannelId.Value, id);
            }

            [Overload (typeof (Embed), "Show all who downvoted / upvoted a specific message given by channel and message ID.")]
            public async Task<Result> Execute (ICommandMetadata data, ulong channelId, ulong messageId)
            {
                ITextChannel channel = ParentPlugin.GuildHandler.FindTextChannel(channelId);
                IMessage message = await channel?.GetMessageAsync(messageId);

                if (channel == null)
                {
                    throw new ArgumentException("The channel could not be found.");
                }

                if (message == null)
                {
                    throw new ArgumentException("The message could not be found.");
                }

                var karma = ParentPlugin.GetKarma(message.Author.Id);
                var karmaMessage = karma.GetMessages().FirstOrDefault(x => x.Id == message.Id);
                
                if (karmaMessage == null)
                {
                    throw new ArgumentException("The message given has neither upvotes or downvotes.");
                }

                var upvoters = karmaMessage.GetUpvotes().Select(x => ParentPlugin.GuildHandler.FindUser(x));
                var downvoters = karmaMessage.GetDownvotes().Select(x => ParentPlugin.GuildHandler.FindUser(x));
                string ups = string.Join(", ", upvoters.Select(x => UserToString(x)));
                string downs = string.Join(", ", downvoters.Select(x => UserToString(x)));

                string UserToString(IUser user) => user == null ? "*Missing User*" : user.GetShownName();

                return new Result(new EmbedBuilder().WithTitle("Voters of Message")
                    .WithDescription($"> [{message.Content.Replace('\n', ' ')}]({message.GetJumpUrl ()}) - {karmaMessage.ToString()}")
                    .AddField("The following people upvoted the message..", upvoters.Count() == 0 ? "```No one :(```" : $"```{ups}```")
                    .AddField("The following people downvoted the message..", downvoters.Count () == 0 ? "```No one! :)```" : $"```{downs}```")
                    .Build(), string.Empty);


            }
            [Overload(typeof (Embed), "Show all who downvoted / upvoted a specific message given by URL.")]
            public Task<Result> Execute (ICommandMetadata data, string messageUrl)
            {
                (ulong channelId, ulong messageId) = messageUrl.ParseMessageUrl();
                return Execute(data, channelId, messageId);
            }
        }

        private static LargeEmbed GetLeaderboardEmbed<T>(T[] leaderboard, Func<T, T, int> sortFunction, Func<T, IUser> getUser, Func<T, string> toString, string ifNone, string title, string description, int amount)
        {
            List<T> inGuild = leaderboard.Where(x => getUser(x) != null).ToList();

            inGuild.Sort(new Comparison<T>((x, y) => sortFunction(x, y)));
            var inRange = inGuild.GetRange(0, Math.Min(amount, inGuild.Count));
            EmbedBuilder result = new EmbedBuilder().WithTitle(title);

            if (inRange.Count == 0)
            {
                result.WithDescription(ifNone);
            }
            else
            {
                result.WithDescription(description.Replace("[amount]", amount.ToString(CultureInfo.InvariantCulture)));

                List<EmbedFieldBuilder> embeds = new List<EmbedFieldBuilder> { new EmbedFieldBuilder() };
                StringBuilder entry = new StringBuilder("```");
                int curLength = 0;
                int index = 0;

                foreach (var user in inRange)
                {
                    string cur = ++index + " - " + AdvDiscordCommands.Extensions.StringExtensions.UniformStrings(getUser(user).GetShownName(), toString(user));
                    curLength += cur.Length;

                    if (curLength > 1450) // 50 characters padding, just to be on the super safe side
                    {
                        entry.Append("```");
                        embeds.Last().WithName($"Group {embeds.Count}").WithValue(entry.ToString());
                        embeds.Add(new EmbedFieldBuilder());
                        entry.Clear();
                    }
                    else
                    {
                        entry.AppendLine(cur);
                    }
                }

                entry.Append("```");
                embeds.Last().WithName($"Group {embeds.Count}").WithValue(entry.ToString());
                return new LargeEmbed(result, embeds);
            }

            return new LargeEmbed(result, Array.Empty<EmbedFieldBuilder>());
        }
    }
}
