using Lomztein.ModularDiscordBot.Core.Bot;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Module.Framework
{
    public interface IModule {

        string Name { get; }
        string Description { get; }
        string Author { get; }

        string [ ] RequiredModules { get; }
        string [ ] RecommnendedModules { get; }

        ModuleHandler ParentModuleHandler { get; set; }
        BotClient ParentBotClient { get; set; }

        void PreInitialize();
        void Initialize();
        void PostInitialize();

        void Shutdown();

    }
}
