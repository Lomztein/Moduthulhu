using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    class BookMessage : ICustomMessage<string, BookMessage.Book, IMessage> {

        public IMessage Message { get; set; }
        public Book Intermediate { get; set; }

        public string Sorrounder { get; private set; }

        public BookMessage (string sorrounder) {
            Sorrounder = sorrounder;
        }

        public void CreateFrom(string source) {
            Intermediate = new Book (source.SplitMessage (Sorrounder));
        }

        public Task DeleteAsync(RequestOptions options = null) {
            throw new NotImplementedException ();
        }

        public Task SendAsync(IMessageChannel channel) {
            throw new NotImplementedException ();
        }

        public class Book {

            public uint Index { get; set; }
            public string[] Pages { get; private set; }
            public string CurrentPage { get => Pages[Index]; }

            public const string LeftArrow = "⬅";
            public const string RightArrow = "➡";

            public IUserMessage Message { get; private set; }

            public Book (string[] pages) {
                Pages = pages;
            }

            public void Flip (int amount) {
                Index = (uint)(Index + amount) % (uint)Pages.Length - 1;
                UpdateMessageAsync ();
            }

            public async void UpdateMessageAsync () {
                await Message.ModifyAsync (x => x.Content = CurrentPage);
            }

            public async void SendAsync (IMessageChannel channel) {
                var message = await channel.SendMessageAsync (CurrentPage);
                await message.AddReactionAsync (new Emoji (LeftArrow));
                await message.AddReactionAsync (new Emoji (RightArrow));

            }
        }
    }
}
