using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests
{
    public class GuildHandlerTests
    {
        private GuildHandler CreateGuildHandler ()
        {
            GuildHandler handler = new GuildHandler(null, 0);
            handler.Initialize();
            return handler;
        }
    }
}
