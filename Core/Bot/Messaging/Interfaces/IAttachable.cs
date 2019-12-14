using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced
{
    public interface IAttachable
    {
        void Attach(GuildHandler guildHandler);

        void Detach(GuildHandler guildHandler);
    }
}
