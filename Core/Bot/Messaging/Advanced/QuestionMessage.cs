using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced
{
    public class QuestionMessage : ISendable<IMessage>, IAttachable, IDeletable {

        private readonly string _contents;
        private ulong? _recipient;
        public IMessage Result { get; set; }

        private QuestionOption[] _options;
        private GuildHandler _handler;

        public QuestionMessage (string contents, params QuestionOption[] options)
        {
            _contents = contents;
            _options = options;
        }

        public QuestionMessage(string contents, Func<Task> ifYes, Func<Task> ifNo) : this(contents, new QuestionOption("👍", ifYes), new QuestionOption("👎", ifNo)) { }
        public QuestionMessage(string contents, Func<Task> ifYes) : this(contents, new QuestionOption("👍", ifYes), new QuestionOption("👎", () => Task.CompletedTask)) { }

        public QuestionMessage SetRecipient(ulong recipient)
        {
            _recipient = recipient;
            return this;
        }

        public void Attach(GuildHandler guildHandler)
        {
            _handler = guildHandler;
            guildHandler.ReactionAdded += GuildHandler_ReactionAdded;
        }

        private async Task GuildHandler_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Discord.WebSocket.ISocketMessageChannel arg2, Discord.WebSocket.SocketReaction arg3)
        {
            var message = await arg1.GetOrDownloadAsync();

            if (arg1.Id == Result.Id && message.Author.Id != arg3.UserId)
            {
                if (!_recipient.HasValue || _recipient.Value == arg3.UserId)
                {
                    QuestionOption option = _options.First(x => x.Emoji == arg3.Emote.Name);
                    if (option != null)
                    {
                        await option.OnOption();
                        Detach(_handler);
                    }
                }
            }
        }

        public Task DeleteAsync(RequestOptions options = null) {
            Detach(_handler);
            return Result.DeleteAsync(options);
        }

        public void Detach(GuildHandler guildHandler)
        {
            guildHandler.ReactionAdded -= GuildHandler_ReactionAdded;
        }

        public async Task SendAsync(IMessageChannel channel) {
            var message = await channel.SendMessageAsync(_contents);
            foreach (QuestionOption option in _options)
            {
                await message.AddReactionAsync(new Emoji(option.Emoji));
            }
            Result = message;
        }
    }

    public class QuestionOption
    {
        public readonly string Emoji;
        public readonly Func<Task> OnOption;
        public QuestionOption(string emoji, Func<Task> onOption)
        {
            Emoji = emoji;
            OnOption = onOption;
        }
    }
}
