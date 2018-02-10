using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.ModularDiscordBot.Core.Bot;

namespace Lomztein.ModularDiscordBot.Core.Module.Framework
{
    public abstract class ModuleBase : IModule {

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public string [ ] RequiredModules { get; } = new string [ 0 ];
        public string [ ] RecommnendedModules { get; } = new string [ 0 ];

        public ModuleHandler ParentModuleHandler { get; set; }
        public BotClient ParentBotClient { get; set; }

        public abstract void Initialize ();

        public virtual void PreInitialize () { }
        public virtual void PostInitialize () { }

        public abstract void Shutdown();

    }
}
