using Discord;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    class LargeTextMessage : ICustomMessage<string, string[], IMessage[]> {

        public IMessage[] Message { get; set; }
        public string[] Intermediate { get; set; }

        public string Sorrounder { get; private set; }

        public LargeTextMessage (string _sorrounder) {
            Sorrounder = _sorrounder;
        }

        public void CreateFrom(string source) {
            Intermediate = source.SplitMessage (Sorrounder);
        }

        public async Task DeleteAsync(RequestOptions options = null) {
            foreach (var message in Message)
                await message.DeleteAsync ();
        }

        public async Task SendAsync(IMessageChannel channel) {
            List<IMessage> messages = new List<IMessage> ();
            foreach (string text in Intermediate) {
                messages.Add (await channel.SendMessageAsync (text));
            }
            Message = messages.ToArray ();
        }
    }
}
