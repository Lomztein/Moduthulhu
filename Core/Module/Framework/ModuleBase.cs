using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Configuration;

namespace Lomztein.Moduthulhu.Core.Module.Framework
{
    public abstract class ModuleBase : IModule {

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract bool Multiserver { get; }

        public virtual string AuthorURL { get; } = "";
        public virtual string PatchURL { get; } = "";

        public virtual string [ ] RequiredModules { get; } = new string [ 0 ];

        public ModuleHandler ParentModuleHandler { get; set; }
        public BotClient ParentBotClient { get; set; }

        public abstract void Initialize ();

        public virtual void PreInitialize () { }
        public virtual void PostInitialize () { }

        public abstract void Shutdown();

    }
}
