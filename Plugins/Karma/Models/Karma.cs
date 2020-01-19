using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    public class Karma
    {
        [JsonProperty("UserId")]
        public ulong UserId { get; private set; }
        [JsonProperty("Messages")]
        private List<Message> _messages;

        public Message[] GetMessages() => _messages.ToArray ();
        public void AddMessage(Message message) => _messages.Add(message);

        public ulong[] GetUpvotes () => _messages.SelectMany(x => x.GetUpvotes()).ToArray ();
        public ulong[] GetDownvotes () => _messages.SelectMany(x => x.GetDownvotes()).ToArray ();

        [JsonIgnore]
        public int Upvotes => _messages.Sum (x => x.Upvotes);
        [JsonIgnore]
        public int Downvotes => _messages.Sum (x => x.Downvotes);
        [JsonIgnore]
        public int Total { get => Upvotes - Downvotes; }

        public override string ToString() => $"{Total} (+{Upvotes} / -{Downvotes})";

        public Karma ()
        {
            _messages = new List<Message>();
        }

        public Karma (ulong userId)
        {
            UserId = userId;
            _messages = new List<Message>();
        }

        public Karma (ulong userId, IEnumerable<Message> messages)
        {
            UserId = userId;
            _messages = messages.ToList ();
        }
    }
}
