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

namespace Lomztein.Moduthulhu.Plugins.Karma.Commands
{
    public class KarmaCommandSet : PluginCommandSet<KarmaPlugin> {

        public KarmaCommandSet ()
        {
            Name = "karma";
            Description = "Measure self-worth.";
            Category = StandardCategories.Fun;
            _defaultCommand = new KarmaCommand();
            _commandsInSet = new List<ICommand> {
                // TODO: me-command.
                // TODO: top-command.
                // TODO: lovers-command.
                // TODO: haters-command.
                // TODO: message-command.
            };
        }

        public class KarmaCommand : PluginCommand<KarmaPlugin>
        {
            public KarmaCommand()
            {
                Name = "karma";
                Description = "Shows karma.";
                Category = StandardCategories.Fun;
            }

            [Overload(typeof(int), "Returns your own karma.")]
            public async Task<Result> Execute(CommandMetadata data)
            {
                return await Execute(data, data.Message.Author);
            }

            [Overload(typeof(int), "Returns karma of a given user.")]
            public async Task<Result> Execute(CommandMetadata data, IUser user)
            {
                var karma = ParentPlugin.GetKarma(user.Id);
                return new Result(await GetKarmaEmbed(karma, user.GetShownName(), 3, ParentPlugin.GuildHandler), string.Empty);
            }

            [Overload(typeof(SocketGuildUser[]), "Returns top <n> karma whores.")]
            public Task<Result> Execute(CommandMetadata data, int amount)
            {
                var allKarma = ParentPlugin.GetLeaderboard();
                List<Karma> inGuild = allKarma.Where (x => ParentPlugin.GuildHandler.FindUser (x.UserId) != null).ToList ();

                var ordered = inGuild.OrderByDescending(x => x.Total).ToList();
                var inRange = ordered.GetRange(0, Math.Min(amount, inGuild.Count));
                StringBuilder result = new StringBuilder();

                if (inRange.Count == 0)
                {
                    result.Append("Nobody has yet recieved any karma.");
                }
                else
                {
                    result.Append ($"Top {amount} karma whores on this server:```");
                    foreach (var user in inRange)
                    {
                        result.Append(StringExtensions.UniformStrings(ParentPlugin.GuildHandler.GetUser (user.UserId).GetShownName(), user.ToString()) + "\n");
                    }
                    result.Append("```");
                }

                return TaskResult(inGuild, result.ToString());
            }

            private async Task<Embed> GetKarmaEmbed(Karma karma, string username, int topCount, GuildHandler messageSource)
            {
                if (karma.Upvotes == 0 && karma.Downvotes == 0)
                {
                    return new EmbedBuilder().WithTitle(":(").WithDescription($"{username} has yet to recieve any karma.").Build();
                }

                EmbedBuilder builder = new EmbedBuilder().
                    WithTitle($"Karma for {username}").
                    WithDescription($"```{karma.Upvotes} upvotes.\n{karma.Downvotes} downvotes.\n{karma.Total} total.```");

                var sortedMessages = karma.GetMessages().OrderByDescending(x => x.Total).ToList().GetRange(0, Math.Min(karma.GetMessages().Length, topCount));
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
                        topMessages.AppendLine($"> '[{userMessage.Content}]({userMessage.GetJumpUrl()})' - {message}");
                    }
                }

                builder.AddField($"Top {sortedMessages.Count} messages", topMessages.ToString());

                builder.AddField(GetTopVoters(karma, karma.GetUpvotes(), true, $"{username} is most loved by..", "upvotes", "```No one :(```", topCount, messageSource));
                builder.AddField(GetTopVoters(karma, karma.GetDownvotes(), false, $"{username} is most hated by..", "downvotes", "```No one! :)```", topCount, messageSource));

                return builder.Build();
            }

            private EmbedFieldBuilder GetTopVoters(Karma karma, ulong[] voters, bool descending, string title, string type, string ifNone, int topCount, GuildHandler guildHandler)
            {
                var sorted = voters.GroupBy(x => x);

                if (descending)
                {
                    sorted = sorted.OrderByDescending(x => x.Count());
                }
                else
                {
                    sorted = sorted.OrderBy(x => x.Count());
                }

                int count = sorted.Count();
                var voterList = sorted.ToList().GetRange(0, Math.Min(count, topCount));

                StringBuilder top = new StringBuilder("```");
                foreach (var upvote in voterList)
                {
                    IUser user = guildHandler.GetUser(upvote.Key);
                    string name = user == null ? "*User not found." : user.GetShownName();
                    top.AppendLine($"{name} - {upvote.Count()} {type}.");
                }
                top.AppendLine("```");

                return new EmbedFieldBuilder().WithName(title).WithValue(count == 0 ? ifNone : top.ToString());
            }
        }
    }
}
