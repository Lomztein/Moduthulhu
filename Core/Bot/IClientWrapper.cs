using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public interface IClientWrapper
    {
        BaseSocketClient Client { get; set; }

        Task StartAsync();

        Task LoginAsync();

        Task DisconnectAsync();

    }
}
