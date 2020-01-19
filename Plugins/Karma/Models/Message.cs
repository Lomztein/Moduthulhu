using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    public class Message
    {
        [JsonProperty ("MessageId")]
        public ulong Id { get; private set; }
        [JsonProperty ("ChannelId")]
        public ulong ChannelId { get; private set; }
        [JsonProperty ("AuthorId")]
        public ulong AuthorId { get; private set; }

        [JsonProperty ("Upvotes")]
        private List<ulong> _upvotes;
        [JsonProperty ("Downvotes")]
        private List<ulong> _downvotes;

        [JsonIgnore]
        public int Upvotes => _upvotes.Count;
        [JsonIgnore]
        public int Downvotes => _downvotes.Count;
        [JsonIgnore]
        public int Total { get => Upvotes - Downvotes; }

        public ulong[] GetUpvotes() => _upvotes.ToArray ();
        public ulong[] GetDownvotes() => _downvotes.ToArray ();

        public void Vote(ulong sender, int value)
        {
            int sign = Math.Sign(value);
            if (sign == 1)
            {
                _upvotes.Add(sender);
            }
            else if (sign == -1)
            {
                _downvotes.Add(sender);
            }
        }

        public override string ToString()
        {
            return $"{Total} (+{Upvotes} / -{Downvotes})";
        }

        public Message ()
        {
        }

        public Message (ulong id, ulong channel, ulong author, IEnumerable<ulong> upvotes, IEnumerable<ulong> downvotes)
        {
            Id = id;
            ChannelId = channel;
            AuthorId = author;
            _upvotes = upvotes.ToList ();
            _downvotes = downvotes.ToList();
        }
    }
}
