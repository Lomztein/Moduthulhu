using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Karma
{
    public interface IKarmaRepository
    {
        void Init();

        Karma GetKarma(ulong guildId, ulong userId);

        Karma[] GetLeaderboard(ulong guildId);

        void DeleteUserData(ulong guildId, ulong userId);

        void ChangeKarma(ulong guildId, ulong senderId, ulong recieverId, ulong channelId, ulong messageId, VoteAction action);
    }
}
