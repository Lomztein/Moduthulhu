using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Core.Module.Framework
{
    public abstract class ModuleBase : IModule {

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract bool Multiserver { get; }

        public virtual string AuthorURL { get; } = "";
        public virtual string PatchURL { get; } = "";

        public ModuleContainer ParentContainer { get; set; }
        public Shard ParentShard { get; set; }

        public abstract void Initialize ();

        public virtual void PreInitialize () { }
        public virtual void PostInitialize () { }

        public abstract void Shutdown();

        private void Log(string contents) => ModuleExtensions.Log (this, contents);

    }
}
