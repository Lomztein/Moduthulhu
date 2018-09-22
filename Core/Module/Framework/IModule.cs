using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Module.Framework
{
    public interface IModule {

        /// <summary>
        /// Your module name, used to easily identify it.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Your module description, should give a short summery of what it does.
        /// </summary>
        string Description { get; } 

        /// <summary>
        /// Your name or alias here, so people know who to love and/or blame.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Should be true if your module can be used by multiserver bots, otherwise false.
        /// </summary>
        bool Multiserver { get; }   

        /// <summary>
        /// Add a link to your personal website or portfolio or github page or whatever, so that people know where to find it.
        /// </summary>
        string AuthorURL { get; }

        /// <summary>
        /// If you have a server that new module patches can be downloaded from, put the URL to the module file here. CURRENTLY NOT USED.
        /// </summary>
        string PatchURL { get; }

        /// <summary>
        /// Does your module have any hard dependancies that it just needs to run? Add them here in format [MODULEAUTHOR]_[MODULENAME]
        /// </summary>
        string [ ] RequiredModules { get; }

        /// <summary>
        /// The module handler that loaded and contains this module. Is set by the module handler.
        /// </summary>
        ModuleLoader ParentModuleHandler { get; set; }

        /// <summary>
        /// The bot client that parents the module handler that loaded this module. Is set by the module handler.
        /// </summary>
        Core ParentBotClient { get; set; }

        /// <summary>
        /// This runs before the bot has been fully connected and config automatically loaded. Use this for anything that can modify your configuration needs, or if you're providing framework for other modules.
        /// </summary>
        void PreInitialize();

        /// <summary>
        /// This runs after the bot has been fully connected and config automatically loaded. It is recommended you use this as your primary initialization.
        /// </summary>
        void Initialize();

        /// <summary>
        /// This runs lastly after everything is set up. As a general rule of thumb, modifying other modules should be done here.
        /// </summary>
        void PostInitialize();

        /// <summary>
        /// This is called if the module is shut down for whatever reason. Use this to undo changes you might have done to the BotClient or other modules.
        /// </summary>
        void Shutdown();
    }
}
