using Lomztein.Moduthulhu.Cross;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class ClientConfiguration
    {
        public struct IntRange
        {
            public int Min, Max;

            public bool IsValid() => Min < Max;
        }
        public IntRange ShardRange;
        public int TotalShards;
        public string Token;

        public void CheckValidity () // Might be better just returning a simple string with a reason, but I wanted to try something more strongly typed, so to speak.
        {
            if (!ShardRange.IsValid())
                throw new InvalidConfigurationException("Shard range is invalid.");

            if (ShardRange.Min < 0)
                throw new InvalidConfigurationException("Shard range minimum is below zero.");

            if (ShardRange.Max > TotalShards)
                throw new InvalidConfigurationException("Shard range max is above total shard count.");

            if (string.IsNullOrWhiteSpace(Token))
                throw new InvalidConfigurationException("Configuration contains no token.");
        }

        public static ClientConfiguration Load (string path)
        {
            return JSONSerialization.DeserializeFile<ClientConfiguration>(path);
        }

        public void Save (string path)
        {
            JSONSerialization.SerializeObject(this, path);
        }

        public class InvalidConfigurationException : Exception
        {
            public InvalidConfigurationException(string message) : base(message)
            {

            }
        }
    }
}
