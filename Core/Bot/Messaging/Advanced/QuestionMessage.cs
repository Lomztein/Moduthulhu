using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced
{
    public class QuestionMessage : ICustomMessage<string, string, IMessage> {

        public IMessage Message { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
        public string Intermediate { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

        public void CreateFrom(string source) {
            throw new NotImplementedException ();
        }

        public Task DeleteAsync(RequestOptions options = null) {
            throw new NotImplementedException ();
        }

        public Task SendAsync(IMessageChannel channel) {
            throw new NotImplementedException ();
        }
    }
}
