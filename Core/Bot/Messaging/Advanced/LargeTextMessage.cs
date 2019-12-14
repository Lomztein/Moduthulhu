using Discord;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    public class LargeTextMessage : ISendable<IMessage[]>, IDeletable {

        private readonly string _sorrounder;
        private readonly string _content;

        public IMessage[] Result { get; private set; }

        public LargeTextMessage (string sorrounder, string content) {
            _sorrounder = sorrounder;
            _content = content;
        }

        public async Task DeleteAsync(RequestOptions options = null) {
            foreach (var message in Result)
            {
                await message.DeleteAsync();
            }
        }

        public async Task SendAsync(IMessageChannel channel) {
            List<IMessage> messages = new List<IMessage> ();
            foreach (string text in _content.SplitMessage (_sorrounder)) {
                messages.Add (await channel.SendMessageAsync (text));
            }
            Result = messages.ToArray ();
        }
    }
}
