using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    public class BookMessage : ISendable<IMessage>, IDeletable, IAttachable {

        private GuildHandler _attachedHandler;
        private Book _book;

        public IMessage Result { get; private set; }

        public BookMessage (string sorrounder, string contents) {
            _book = new Book(contents.SplitMessage(sorrounder));
        }

        public async Task DeleteAsync(RequestOptions options = null) {
            await Result.DeleteAsync();
            Detach(_attachedHandler);
        }

        public async Task SendAsync(IMessageChannel channel) {
            await _book.SendAsync(channel);
        }

        public void Attach(GuildHandler guildHandler)
        {
            _attachedHandler = guildHandler;
            guildHandler.ReactionAdded += GuildHandler_ReactionAdded;
        }

        private async Task GuildHandler_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, Discord.WebSocket.SocketReaction arg3)
        {
            if (arg1.Id == _book.Message.Id)
            {
                if (arg3.Emote.Name == Book.LeftArrow)
                {
                    await _book.Flip(-1);
                }
                if (arg3.Emote.Name == Book.RightArrow)
                {
                    await _book.Flip(1);
                }
            }
        }

        public void Detach(GuildHandler guildHandler)
        {
            guildHandler.ReactionAdded -= GuildHandler_ReactionAdded;
        }

        private class Book {

            public uint Index { get; set; }
            public string[] Pages { get; private set; }
            public string CurrentPage { get => Pages[Index]; }

            public const string LeftArrow = "⬅";
            public const string RightArrow = "➡";

            public IUserMessage Message { get; private set; }

            public Book (string[] pages) {
                Pages = pages;
            }

            public async Task Flip (int amount) {
                Index = (uint)(Index + amount) % (uint)Pages.Length - 1;
                await UpdateMessageAsync ();
            }

            private async Task UpdateMessageAsync () {
                await Message.ModifyAsync (x => x.Content = CurrentPage);
            }

            public async Task SendAsync (IMessageChannel channel) {
                var message = await channel.SendMessageAsync (CurrentPage);
                await message.AddReactionAsync (new Emoji (LeftArrow));
                await message.AddReactionAsync (new Emoji (RightArrow));

            }
        }
    }
}
