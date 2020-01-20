using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    public enum VoteAction { AddUpvote, AddDownvote, RemoveUpvote, RemoveDownvote }
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

        public void Vote(ulong sender, VoteAction action)
        {
            switch (action)
            {
                case VoteAction.AddUpvote:
                    _upvotes.Add(sender);
                    break;

                case VoteAction.AddDownvote:
                    _downvotes.Add(sender);
                    break;

                case VoteAction.RemoveUpvote:
                    _upvotes.Remove(sender);
                    break;

                case VoteAction.RemoveDownvote:
                    _downvotes.Remove(sender);
                    break;
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
