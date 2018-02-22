using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration
{
    public interface IConfigurable {

        void Configure();

    }

    public interface IConfigurable<T> : IConfigurable where T : Config {

        T Configuration { get; set; }

    }
}
