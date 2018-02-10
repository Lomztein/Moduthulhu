using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration
{
    class SingleConfig : Config {

        public SingleConfig(string _name) : base (_name) { }

        public T GetEntry<T> (string key, T fallback) {
            return GetEntry (0, key, fallback);
        }
    }
}
