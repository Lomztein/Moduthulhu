using Lomztein.ModularDiscordBot.Core.Bot;
using Lomztein.ModularDiscordBot.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Module.Framework
{
    public interface IModule {

        string Name { get; }
        string Description { get; }
        string Author { get; }
        bool Multiserver { get; }

        string [ ] RequiredModules { get; }
        string [ ] RecommendedModules { get; }
        string [ ] ConflictingModules { get; }

        ModuleHandler ParentModuleHandler { get; set; }
        BotClient ParentBotClient { get; set; }

        void PreInitialize();
        void Initialize();
        void PostInitialize();

        void Shutdown();

    }
}
