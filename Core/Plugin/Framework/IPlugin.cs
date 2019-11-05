using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using System;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    public interface IPlugin {

        /// <summary>
        /// Your module name, used to easily identify it. Required.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Your module description, should give a short summery of what it does. Not required but highly recommended.
        /// </summary>
        string Description { get; } 

        /// <summary>
        /// Your name or alias here, so people know who to love and/or blame. Required.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// The version of the plugin. Required if you make breaking changes in a plugin with dependant plugins.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Add a link to your personal website or portfolio or github page or whatever, so that people know where to find it. Not required.
        /// </summary>
        Uri AuthorURI { get; }

        /// <summary>
        /// If you have a server that new module patches can be downloaded from, put the URL to the module .dll file here. Only required if you wish for it to automatically be patched.
        /// </summary>
        Uri PatchURI { get; }

        /// <summary>
        /// A URL to a public repository with this module, so that someone may inspect the source if they so desire. Not required.
        /// </summary>
        Uri ProjectURI { get; }

        /// <summary>
        /// The bot client that parents the module handler that loaded this module. Is set when the module is created.
        /// </summary>
        GuildHandler GuildHandler { get; }

        /// <summary>
        /// This runs before the bot has been fully connected and config automatically loaded. Use this for anything that can modify your configuration needs, or if you're providing framework for other modules.
        /// </summary>
        void PreInitialize(GuildHandler guildHandler);

        /// <summary>
        /// This runs after the bot has been fully connected and config automatically loaded. It is recommended you use this as your primary initialization.
        /// </summary>
        void Initialize();

        /// <summary>
        /// This runs lastly after everything is set up. As a general rule of thumb, modifying other modules should be done here.
        /// </summary>
        void PostInitialize();

        /// <summary>
        /// This is called if the module is shut down for whatever reason. Use this to undo changes you might have done to the Core or other modules.
        /// </summary>
        void Shutdown();
    }
}
