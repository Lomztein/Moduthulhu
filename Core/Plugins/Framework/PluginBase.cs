using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public abstract class PluginBase : IPlugin {

        /// <summary>
        /// The name of this plugin as defined by a [Descriptor] attribute.
        /// </summary>
        public string Name => Plugin.GetName(GetType());

        /// <summary>
        /// The description of this plugin as defined by a [Descriptor] attribute.
        /// </summary>
        public string Description => Plugin.GetDescription(GetType());

        /// <summary>
        /// The author of this plugin as defined by a [Descriptor] attribute.
        /// </summary>
        public string Author => Plugin.GetAuthor(GetType());

        /// <summary>
        /// The version of the plugin as defined by a [Descriptor] attribute.
        /// </summary>
        public string Version => Plugin.GetVersion(GetType());

        /// <summary>
        /// The authors portfolio URI as defined by a [Source] attribute.
        /// </summary>
        public Uri AuthorURI => Plugin.GetAuthorURI(GetType());

        /// <summary>
        /// The plugins patch URI where new versions can be downloaded from, as defined by a [Source] attribute. Not currently in use.
        /// </summary>
        public Uri PatchURI => Plugin.GetPatchURI(GetType());

        /// <summary>
        /// The plugins project URI such as a repository URI, as defined by a [Source] attribute.
        /// </summary>
        public Uri ProjectURI => Plugin.GetProjectURI(GetType());

        /// <summary>
        /// The GuildHandler this plugin is assigned to, which represents a specific discord server. Contains a wealth of tools.
        /// </summary>
        public GuildHandler GuildHandler { get; private set; }

        /// <summary>
        /// Called after all plugins PreInitialize and after all plugins PostInitialize. Use for general purpose initialization.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called before any other init methods. Use for setting up services that other plugins might need.
        /// </summary>
        /// <param name="handler">The GuildHandler this plugin is to be assigned to.</param>
        public virtual void PreInitialize(GuildHandler handler) => GuildHandler = handler;

        /// <summary>
        /// Called lastly, after any other init methods. Use for handling possible information given by other plugins.
        /// </summary>
        public virtual void PostInitialize() { }

        /// <summary>
        /// Called when the plugin is being shut down. Use this to revert any changes this plugin has done to the core, or to other plugins.
        /// </summary>
        public abstract void Shutdown();

        /// <summary>
        /// Called for each plugin when a user requests their personal user data.
        /// </summary>
        /// <param name="id">Requester user id</param>
        /// <returns>A JToken containing all data relavant to the given user id.</returns>
        public virtual JToken RequestUserData(ulong id) => null;

        /// <summary>
        /// Called for each plugin when a user requests their personal user data deleted.
        /// </summary>
        /// <param name="id">Requester user id</param>
        public virtual void DeleteUserData(ulong id) { }

        /// <summary>
        /// Log something.
        /// </summary>
        /// <param name="contents">The contents to be logged.</param>
        protected void Log(string contents) => Plugin.Log(this, contents);

        /// <summary>
        /// Get a <see cref="CachedValue{T}"/> linked to this plugins guild within the "plugindata" database table.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Value key</param>
        /// <param name="defaultValue">Default value getter function</param>
        /// <returns>CachedValue containing a value of type <typeparamref name="T"/>.</returns>
        protected CachedValue<T> GetDataCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new DoubleKeyJsonRepository("plugindata"), GuildHandler.GuildId, Plugin.GetFullName(GetType()) + "." + key, () => defaultValue(GuildHandler));

        /// <summary>
        /// Get a <see cref="CachedValue{T}"/> linked to this plugins guild and the given key, within the "pluginconfig" database table.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Value key</param>
        /// <param name="defaultValue">Default value getter function</param>
        /// <returns>CachedValue containing a value of type <typeparamref name="T"/>.</returns>
        protected CachedValue<T> GetConfigCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new DoubleKeyJsonRepository("pluginconfig"), GuildHandler.GuildId, Plugin.GetFullName(GetType()) + "." + key, () => defaultValue(GuildHandler));

        /// <summary>
        /// Register a message function that other plugins may call with <see cref="SendMessage(string, string, object)"/>. <seealso cref="SendMessage(string, string)"/>
        /// </summary>
        /// <param name="identifier">Message identifier</param>
        /// <param name="function">Message function delegate</param>
        protected void RegisterMessageFunction(string identifier, Func<object, object> function) => GuildHandler.Messenger.Register(Plugin.GetFullName(GetType()), identifier, function);
     
        /// <summary>
        /// Register a message action that other plugins may call with <see cref="SendMessage(string, string, object)"/>. <seealso cref="SendMessage(string, string)"/>
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="action"></param>
        protected void RegisterMessageAction(string identifier, Action<object> action) => GuildHandler.Messenger.Register(Plugin.GetFullName(GetType()), identifier, action);

        /// <summary>
        /// Unregister a message action or function previously registered using <see cref="RegisterMessageAction(string, Action{object})"/> or <see cref="RegisterMessageFunction(string, Func{object, object})"/>.
        /// </summary>
        /// <param name="identifier"></param>
        protected void UnregisterMessageDelegate(string identifier) => GuildHandler.Messenger.Unregister(Plugin.GetFullName(GetType()), identifier);
        
        /// <summary>
        /// Clear all registered message actions or functions previously registered using <see cref="RegisterMessageAction(string, Action{object})"/> or <see cref="RegisterMessageFunction(string, Func{object, object})"/>
        /// </summary>
        protected void ClearMessageDelegates() => GuildHandler.Messenger.Clear(Plugin.GetFullName(GetType()));

        /// <summary>
        /// Overload of <see cref="AddConfigInfo(string, string, Delegate, Delegate, string[])"/>. Has a host of variants with generic arguments for inputs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="action"></param>
        /// <param name="message"></param>
        protected void AddConfigInfo(string name, string description, Action action, Func<string> message) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message);

        /// <summary>
        /// Add a new configuration info that may be used for end-users to configure the bot.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="action">Action delegate</param>
        /// <param name="message">Function delegate, must return string</param>
        protected void AddConfigInfo(string name, string description, Delegate action, Delegate message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);

        /// <summary>
        /// Overload of <see cref="AddConfigInfo(string, string, Delegate, Delegate, string[])"/> that contains an empty delegate.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="message"></param>
        protected void AddConfigInfo(string name, string description, Func<string> message) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), new Action(() => { }), message);

        // Generic AddConfigInfo methods.
        #region
        protected void AddConfigInfo<T1>(string name, string description, Action<T1> action, Func<T1, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2>(string name, string description, Action<T1, T2> action, Func<T1, T2, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3>(string name, string description, Action<T1, T2, T3> action, Func<T1, T2, T3, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4>(string name, string description, Action<T1, T2, T3, T4> action, Func<T1, T2, T3, T4, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5>(string name, string description, Action<T1, T2, T3, T4, T5> action, Func<T1, T2, T3, T4, T5, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5, T6>(string name, string description, Action<T1, T2, T3, T4, T5, T6> action, Func<T1, T2, T3, T4, T5, T6, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5, T6, T7>(string name, string description, Action<T1, T2, T3, T4, T5, T6, T7> action, Func<T1, T2, T3, T4, T5, T6, T7, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5, T6, T7, T8>(string name, string description, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        #endregion

        /// <summary>
        /// Clear all configuration information previously registered using <see cref="AddConfigInfo(string, string, Delegate, Delegate, string[])"/>.
        /// </summary>
        protected void ClearConfigInfos() => GuildHandler.Config.Clear(Plugin.GetFullName(GetType()));

        /// <summary>
        /// Send a message to a registered action/function on another plugin using <see cref="RegisterMessageAction(string, Action{object})"/> or <see cref="RegisterMessageFunction(string, Func{object, object})"/>.
        /// </summary>
        /// <param name="target">Target plugin</param>
        /// <param name="identifier">Target function/action</param>
        protected void SendMessage(string target, string identifier) => GuildHandler.Messenger.SendMessage(target, identifier);

        /// <summary>
        /// Send a message to a registered action/function on another plugin using <see cref="RegisterMessageAction(string, Action{object})"/> or <see cref="RegisterMessageFunction(string, Func{object, object})"/>.
        /// </summary>
        /// <param name="target">Target plugin</param>
        /// <param name="identifier">Target function/action</param>
        /// <param name="value">Value to pass to action/function</param>
        protected void SendMessage(string target, string identifier, object value) => GuildHandler.Messenger.SendMessage(target, identifier, value);

        /// <summary>
        /// Send a message to a registered function on another plugin using <see cref="RegisterMessageFunction(string, Func{object, object})"/>.
        /// </summary>
        /// <typeparam name="T">The type of value expected to be returned.</typeparam>
        /// <param name="target">Target plugin</param>
        /// <param name="identifier">Target function</param>
        /// <returns>An value returned by the target funcion.</returns>
        protected T SendMessage<T>(string target, string identifier) => GuildHandler.Messenger.SendMessage<T>(target, identifier);

        /// <summary>
        /// Send a message to a registered function on another plugin using <see cref="RegisterMessageFunction(string, Func{object, object})"/>.
        /// </summary>
        /// <typeparam name="T">The type of value expected to be returned.</typeparam>
        /// <param name="target">Target plugin</param>
        /// <param name="identifier">Target function</param>
        /// <param name="value">Value to pass to function</param>
        /// <returns>An value returned by the target funcion.</returns>
        protected T SendMessage<T>(string target, string identifier, object value) => GuildHandler.Messenger.SendMessage<T>(target, identifier, value);

        /// <summary>
        /// Throws a <see cref="MissingPermissionException"/> if the bot is missing permission <paramref name="perm"/>.
        /// </summary>
        /// <param name="perm">The permission to asserted</param>
        protected void AssertPermission(GuildPermission perm) => GuildHandler.AssertPermission(perm);

        /// <summary>
        /// Throws a <see cref="MissingPermissionException"/> if the bot is missing permission <paramref name="perm"/> in a channel with id <paramref name="channelId"/>.
        /// </summary>
        /// <param name="perm">The permission to asserted</param>
        /// <param name="channelId">The channel to be checked</param>
        protected void AssertChannelPermission(ChannelPermission perm, ulong channelId) => GuildHandler.AssertChannelPermission(perm, channelId);
      
        /// <summary>
        /// Check if the bot has permission <paramref name="perm"/> in this plugins assigned server.
        /// </summary>
        /// <param name="perm">The permission to check</param>
        /// <returns>True if bot has permission, otherwise false.</returns>
        protected bool HasPermission(GuildPermission perm) => GuildHandler.HasPermission(perm);

        /// <summary>
        /// Check if the bot has permission <paramref name="perm"/> in this plugins assigned server in a channel with id <paramref name="channelId"/>.
        /// </summary>
        /// <param name="perm">The permission to check</param>
        /// <returns>True if bot has permission, otherwise false.</returns>
        protected void HasChannelPermission(ChannelPermission perm, ulong channelId) => GuildHandler.HasChannelPermission(perm, channelId);

        /// <summary>
        /// Add a state attribute to let the system know of a certain modification this plugin has made. Differences in state attributes between reloads are notified to the Discord server.
        /// </summary>
        /// <param name="identifier">State identifier</param>
        /// <param name="name">Name of attribute</param>
        /// <param name="description">Description of attribute</param>
        /// <seealso cref="SetStateChangeHeaders(string, string, string, string)"/>
        protected void AddStateAttribute(string identifier, string name, string description) => GuildHandler.AddStateAttribute(Plugin.GetVersionedFullName(GetType())+identifier, name, description);

        /// <summary>
        /// Add a state attribute to let the system know of a modification to general features. General-purpose version of <see cref="AddStateAttribute(string, string, string)"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <seealso cref="SetStateChangeHeaders(string, string, string, string)"/>
        protected void AddGeneralFeaturesStateAttribute(string name, string description) => GuildHandler.AddStateAttribute("GeneralFeatures", name, description);
      
        /// <summary>
        /// Set a change header for states that may be modified using <see cref="AddStateAttribute(string, string, string)"/>.
        /// </summary>
        /// <param name="identifier">State identifier</param>
        /// <param name="addition">Addition header</param>
        /// <param name="removals">Removal header</param>
        /// <param name="mutations">Modification header</param>
        /// <seealso cref="SetStateChangeHeaders(string, string, string, string)"/>
        protected void SetStateChangeHeaders(string identifier, string addition, string removals, string mutations) => GuildHandler.SetStateChangeHeaders(Plugin.GetVersionedFullName(GetType())+identifier, addition, removals, mutations);
       
        /// <summary>
        /// Set a change header for states that may be modified using <see cref="AddStateAttribute(string, string, string)"/>.
        /// </summary>
        /// <param name="identifier">State identifier</param>
        /// <param name="addition">Addition header</param>
        /// <param name="removals">Removal header</param>
        /// <seealso cref="SetStateChangeHeaders(string, string, string)"/>
        protected void SetStateChangeHeaders(string identifier, string addition, string removals) => GuildHandler.SetStateChangeHeaders(Plugin.GetVersionedFullName(GetType())+identifier, addition, removals, string.Empty);

        /// <summary>
        /// Notify the assigned Discord server of something in their chosen notification channel.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="embed">Embed to be sent</param>
        /// <returns>Task for sending message</returns>
        protected Task NotifyGuild(string message, Embed embed) => GuildHandler.Notifier.Notify(message, embed);

        /// <summary>
        /// Notify the assigned Discord server of something in their chosen notification channel.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <returns>Task for sending message</returns>
        protected Task NotifyGuild(string message) => GuildHandler.Notifier.Notify(message);

        /// <summary>
        /// Notify the assigned Discord server of something in their chosen notification channel.
        /// </summary>
        /// <param name="embed">Embed to be sent</param>
        /// <returns>Task for sending message</returns>
        protected Task NotifyGuild(Embed embed) => GuildHandler.Notifier.Notify(embed);

        /// <summary>
        /// Instantly disable this plugin for <paramref name="reason"/> reason.
        /// </summary>
        /// <param name="reason">Reason</param>
        protected void DisablePlugin(string reason)
        {
            GuildHandler.Plugins.RemovePlugin(Plugin.GetName(GetType()));
            GuildHandler.Plugins.ReloadPlugins();
            throw new PluginDisabledException(reason);
        }

        /// <summary>
        /// Check if a certain permission is missing, and call <see cref="DisablePlugin(string)"/> if so. Call <see cref="NotifyGuild(string)"/> if <paramref name="permission"/> is true.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="notifyGuild">Notify guild of disable</param>
        protected void DisablePluginIfPermissionMissing(GuildPermission permission, bool notifyGuild)
        {
            if (!HasPermission(permission))
            {
                if (notifyGuild)
                {
                    NotifyGuild($"Plugin {Plugin.GetName(GetType())} requires permission '{permission}' to function, which has been revoked. The plugin has automatically been disabled.");
                }
                DisablePlugin($"Disabled plugin due to revoked permission '{permission}'.");
            }
        }
    }
}
