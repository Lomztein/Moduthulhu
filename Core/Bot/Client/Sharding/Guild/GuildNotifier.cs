using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    /// <summary>
    /// Class for sending messages to a user-defined channel on the assigned guild.
    /// </summary>
    public class GuildNotifier
    {
        private GuildHandler _parentGuild;
        private CachedValue<ulong> _notificationChannel;
        private CachedValue<bool> _allowNotifications;

        public GuildNotifier(GuildHandler parentGuild)
        {
            _parentGuild = parentGuild;
            _notificationChannel = new CachedValue<ulong>(
                new DoubleKeyJsonRepository("pluginconfig"), _parentGuild.GuildId, "NotificationChannel", () => (_parentGuild.GetGuild().TextChannels.FirstOrDefault()?.Id).GetValueOrDefault());
            _allowNotifications = new CachedValue<bool>(
                new DoubleKeyJsonRepository("pluginconfig"), _parentGuild.GuildId, "AllowNotifications", () => true);

            _parentGuild.Config.Add("Set Notification Channel", "Set channel", "Universal Settings",
                new Action<SocketTextChannel>(x => _notificationChannel.SetValue(x.Id)),
                new Func<SocketTextChannel, string>(x => $"Set notification channel to {x.Mention}"),
                "Channel");
            _parentGuild.Config.Add("Set Notification Channel", "Set channel", "Universal Settings", new Action(() => { }), new Func<string>(
                () => $"Current notification channel is {GetNotificationChannelName()}"));
            _parentGuild.Config.Add("Toggle Notifications", "Toggle notications", "Universal Settings", new Action(() => _allowNotifications.SetValue (!_allowNotifications.GetValue ())), new Func<string>(
                () => _allowNotifications.GetValue () ? "You have opted in to bot notifications." : "You have opted out of bot notifications."));
        }

        private string GetNotificationChannelName ()
        {
            var channel = _parentGuild.FindTextChannel(_notificationChannel.GetValue());
            return channel == null ? "Channel doesn't exist either due to missing configuration or deleted channel. Please reconfigure." : channel.Mention;
        }

        private SocketTextChannel GetNotificationChannel() => _parentGuild.FindTextChannel(_notificationChannel.GetValue());
        private void ResetNotificationChannel() => _notificationChannel.SetValue((_parentGuild.GetGuild().TextChannels.FirstOrDefault()?.Id).GetValueOrDefault());

        /// <summary>
        /// Send a message to the assigned guild containing both a <paramref name="message"/> and an <paramref name="embed"/>.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="embed">Embed to be sent</param>
        /// <returns>Send Message Task</returns>
        public async Task Notify (string message, Embed embed)
        {
            var channel = GetNotificationChannel();
            if (channel == null)
            {
                ResetNotificationChannel();
                channel = GetNotificationChannel();
                message = $"*Notification channel has automatically been reset to {channel.Mention}, due to the previous channel not being found.*\n\n" + message;
            }

            if (_allowNotifications.GetValue())
            {
                Log.Bot($"Notifying guild {_parentGuild.Name}: {message}");
                await channel.SendMessageAsync(message, false, embed);
            }
            else
            {
                Log.Bot($"Failed to notify guild {_parentGuild.Name}, they may have opted out of notifications, or are missing a notification channel: {message}");
            }
        }

        /// <summary>
        /// See <see cref="Notify(string, Embed)"/>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Notify(string message) => Notify(message, null);

        /// <summary>
        /// See <see cref="Notify(string, Embed)"/>
        /// </summary>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task Notify(Embed embed) => Notify(string.Empty, embed);
    }
}
