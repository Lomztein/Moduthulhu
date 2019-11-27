using Lomztein.Moduthulhu.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class ClientConfiguration
    {
        public struct IntRange : IEquatable<IntRange>
        {
            [JsonProperty]
            public int Min { get; private set; }
            [JsonProperty]
            public int Max { get; private set; }

            public bool IsValid() => Min < Max;

            public override bool Equals(object obj)
            {
                if (obj is IntRange other)
                {
                    return Max == other.Max && Min == other.Min;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public bool Equals(IntRange other)
            {
                return Equals((object)other);
            }

            public static bool operator ==(IntRange left, IntRange right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(IntRange left, IntRange right)
            {
                return !(left == right);
            }
        }

            [JsonProperty]
        public IntRange ShardRange { get; private set; }
            [JsonProperty]
        public int TotalShards { get; private set; }
            [JsonProperty]
        public string Token { get; private set; }

        public void CheckValidity ()
        {
            if (!ShardRange.IsValid())
            {
                throw new InvalidConfigurationException("Shard range is invalid.");
            }

            if (ShardRange.Min < 0)
            {
                throw new InvalidConfigurationException("Shard range minimum is below zero.");
            }

            if (ShardRange.Max > TotalShards)
            {
                throw new InvalidConfigurationException("Shard range max is above total shard count.");
            }

            if (string.IsNullOrWhiteSpace(Token))
            {
                throw new InvalidConfigurationException("Configuration contains no token.");
            }
        }

        public static ClientConfiguration Load (string path)
        {
            return JSONSerialization.DeserializeFile<ClientConfiguration>(path);
        }

        public void Save (string path)
        {
            JSONSerialization.SerializeObject(this, path);
        }
    }

    public class InvalidConfigurationException : Exception, ISerializable
    {
        public InvalidConfigurationException(string message) : base(message)
        {

        }

        public InvalidConfigurationException()
        {
        }

        public InvalidConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
