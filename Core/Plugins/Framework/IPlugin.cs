using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Newtonsoft.Json.Linq;
using System;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public interface IPlugin {

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

        /// <summary>
        /// Calling this should return a JObject containing all data related to the end-user who's ID is given.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        JToken RequestUserData(ulong userId);

        /// <summary>
        /// Calling this should delete any data related to the specific end-user who's ID is given.
        /// </summary>
        /// <param name="userId"></param>
        void DeleteUserData(ulong userId);
    }
}
