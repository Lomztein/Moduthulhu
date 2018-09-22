using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Sharding
{
    public class ModuleContainer
    {
        public List<IModule> Modules { get; private set; }

        public T GetModule<T>() {
            IModule module = Modules.Find (x => x is T);
            if (module == null)
                return default (T);
            else
                return (T)module;
        }
    }
}
