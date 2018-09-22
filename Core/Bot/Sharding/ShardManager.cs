using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Sharding
{
    public class ShardManager
    {
        public Core Core { get; private set; }

        public string Token { get; private set; }

        public List<Shard> ActiveShards { get; private set; }
        public int TotalShards { get; private set; }

        public ShardManager (Core core, string token) {
            Core = core;
            Token = token;
        }

        public Shard Spawn (int shardId) {
            Shard shard = new Shard (this, shardId);
            ActiveShards.Add (shard);
            return shard;
        }

    }
}
