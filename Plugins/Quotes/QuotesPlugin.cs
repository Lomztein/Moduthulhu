using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Quotes
{
    [Descriptor("Lomztein", "Quotes", "Allow the quoting of people or anything that can speak I guess, it's $CurrentYear, we don't descriminate. Totally not almost exactly the same as pins, I swear.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Quotes/QuotesPlugin.cs")]
    [Dependency("Moduthulhu-Command Root")]
    public class QuotesPlugin : PluginBase
    {
        private CachedValue<List<Quote>> _quotes;
        private QuoteCommand _quoteCommand;
        private UnquoteCommand _unquoteCommand;

        public override void Initialize()
        {
            _quotes = GetDataCache("Quotes", x => new List<Quote>());
            _quoteCommand = new QuoteCommand() { ParentPlugin = this };
            _unquoteCommand = new UnquoteCommand() { ParentPlugin = this };

            SendMessage("Moduthulhu-Command Root", "AddCommand", _quoteCommand);
            SendMessage("Moduthulhu-Command Root", "AddCommand", _unquoteCommand);
        }

        public override void Shutdown()
        {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _quoteCommand);
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _unquoteCommand);
        }

        public Quote GetRandomQuote ()
        {
            Random random = new Random();
            List<Quote> quotes = _quotes.GetValue();
            return quotes[random.Next(quotes.Count)];
        }

        public Quote GetQuote(int index) => _quotes.GetValue()[index];

        public Quote AddQuote (Quote newQuote)
        {
            _quotes.MutateValue(x => x.Add(newQuote));
            return newQuote;
        }

        public Quote RemoveQuote (int index)
        {
            List<Quote> quotes = _quotes.GetValue();
            if (index >= 0 && index < quotes.Count)
            {
                Quote quote = quotes[index];
                _quotes.MutateValue(x => x.RemoveAt(index));
                return quote;
            }
            else
            {
                throw new InvalidOperationException($"ID is out of range, it must be between 0 and '{quotes.Count - 1}'.");
            }
        }

        public Quote RemoveQuote (string content)
        {
            List<Quote> quotes = _quotes.GetValue();
            if (quotes.Any(x => x.Content == content))
            {
                Quote quote = quotes.First(x => x.Content == content);
                _quotes.MutateValue(x => x.Remove(quote));
                return quote;
            }
            else
            {
                throw new InvalidOperationException($"There are no current quotes with the content '{content}'.");
            }
        }

        public Embed GetQuoteEmbed (Quote quote)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithDescription(quote.Content).WithFooter(" - " + quote.Author + ", " + quote.Year);
            return builder.Build();
        }
    }

    public class Quote
    {
        [JsonProperty("Content")]
        public readonly string Content;
        [JsonProperty("Author")]
        public readonly string Author;
        [JsonProperty("Year")]
        public readonly int Year;

        public Quote(string content, string author, int year)
        {
            Content = content;
            Author = author;
            Year = year;
        }
    }

    public class QuoteCommand : PluginCommand<QuotesPlugin>
    {
        public QuoteCommand ()
        {
            Name = "quote";
            Description = "Words of wisdom.";
            Category = StandardCategories.Fun;
            RequiredPermissions.Add(GuildPermission.ManageMessages);
        }

        [Overload(typeof(Embed), "Get a random tidbit of endless wisdom.")]
        public Task<Result> Execute (CommandMetadata metadata)
        {
            return TaskResult(ParentPlugin.GetQuoteEmbed(ParentPlugin.GetRandomQuote()), string.Empty);
        }

        [Overload(typeof(Embed), "Quote a specific message using the message link.")]
        public async Task<Result> Execute(CommandMetadata metadata, string url) // Perhaps add URLs to the command framework?
        {
            (ulong channelId, ulong messageId) = url.ParseMessageUrl();
            SocketTextChannel channel = ParentPlugin.GuildHandler.FindTextChannel(channelId);

            if (channel == null)
            {
                throw new ArgumentException($"The message URL does not point to any channel on this server.");
            }

            var message = await channel.GetMessageAsync(messageId);
            if (message == null)
            {
                throw new ArgumentException($"The message URL does not point to any message in channel '{channel.Name}'. Perhaps it has been deleted.");
            }
            if (message is IUserMessage userMessage)
            {
                if (!string.IsNullOrEmpty(userMessage.Content))
                {
                    Quote quote = new Quote(userMessage.Content, userMessage.Author.GetShownName(), userMessage.Timestamp.Year);
                    ParentPlugin.AddQuote(quote);
                    return new Result(ParentPlugin.GetQuoteEmbed(quote), $"Succesfully quoted {userMessage.Author.GetShownName()} in their infinite wisdom.");
                }
                else
                {
                    throw new ArgumentException($"The message URL points to a message without any text content, perhaps an embed-only message. There must be some raw text to quote.");
                }
            }
            else
            {
                throw new ArgumentException($"The message URL points to a non-user message. Only messages sent by users or bots can be used.");
            }
        }

        [Overload(typeof(Embed), "Quote someone in current year.")]
        public Task<Result> Execute(CommandMetadata metadata, string content, string author)
        {
            Quote quote = new Quote(content, author, DateTime.Now.Year);
            ParentPlugin.AddQuote(quote);
            return TaskResult(ParentPlugin.GetQuoteEmbed(quote), $"Succesfully quoted {author} in their endless genius.");
        }

        [Overload(typeof(Embed), "Quote someone in a specific year.")]
        public Task<Result> Execute(CommandMetadata metadata, string content, string author, int year)
        {
            Quote quote = new Quote(content, author, year);
            ParentPlugin.AddQuote(quote);
            return TaskResult(ParentPlugin.GetQuoteEmbed(quote), $"Succesfully quoted {author} in their eternal brilliancy.");
        }
    }

    public class UnquoteCommand : PluginCommand<QuotesPlugin>
    {
        public UnquoteCommand ()
        {
            Name = "unquote";
            Description = "Words of dumbness";
            Category = StandardCategories.Fun;
            RequiredPermissions.Add(GuildPermission.ManageMessages);
        }

        [Overload(typeof (void), "Remove a command by a given index.")]
        public Task<Result> Execute (CommandMetadata metadata, int id)
        {
            Quote quote = ParentPlugin.RemoveQuote(id);
            return TaskResult(null, $"Succesfully removed quote with id {id} and content '{quote.Content} - {quote.Author}, {quote.Year}'.");
        }

        [Overload(typeof(void), "Remove a command by its content.")]
        public Task<Result> Execute(CommandMetadata metadata, string content)
        {
            Quote quote = ParentPlugin.RemoveQuote(content);
            return TaskResult(null, $"Succesfully removed quote with content '{quote.Content} - {quote.Author}, {quote.Year}'.");
        }
    }
}
