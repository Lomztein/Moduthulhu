﻿using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    public class CachedValueKarmaRepository : IKarmaRepository
    {
        private Dictionary<ulong, CachedValue<Karma>> _karma = new Dictionary<ulong, CachedValue<Karma>>();
        private DoubleKeyJsonRepository _sourceRepo = new DoubleKeyJsonRepository("lomzkarma_cachedvalue");

        public void ChangeKarma(ulong guildId, ulong senderId, ulong recieverId, ulong channelId, ulong messageId, VoteAction action)
        {
            AddIfMissing(guildId, recieverId);
            _karma[recieverId].MutateValue (x => x.Vote(senderId, channelId, messageId, action));
        }

        public void DeleteUserData(ulong guildId, ulong userId)
        {
        }

        public Karma GetKarma(ulong guildId, ulong userId)
        {
            AddIfMissing(guildId, userId);
            return _karma[userId].GetValue();
        }

        private void AddIfMissing (ulong guildId, ulong userId)
        {
            if (!_karma.ContainsKey(userId))
            {
                _karma.Add(userId, GetKarmaCache(guildId, userId));
            }
        }

        public Karma[] GetLeaderboard(ulong guildId)
        {
            CachedArray<Karma> karma = new CachedArray<Karma>(_sourceRepo, guildId, "lomzkarma_");
            return karma.GetValue ();
        }

        public void Init()
        {
        }

        private CachedValue<Karma> GetKarmaCache(ulong guildId, ulong userId)
            => new CachedValue<Karma>(_sourceRepo, guildId, $"lomzkarma_{userId.ToString()}", () => new Karma(userId));
    }
}
