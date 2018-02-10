using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration
{
    public interface IConfigurable
    {
        void Configure();

        Config Configuration { get; set; }
    }
}
