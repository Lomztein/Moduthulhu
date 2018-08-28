using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.CustomCommands.Categories;
using Lomztein.Moduthulhu.Modules.Voice.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice.Commands {
    public class VoiceLockingSet : ModuleCommandSet<VoiceLockingModule> {

        private static readonly Category LockingCategory = new Category ("Locking", "Commands allowing you to create a lock on voice channels, allowing only certain people into it.");

        public VoiceLockingSet() {
            Name = "voice";
            Description = "A command set for locking voice channels.";
            Category = AdditionalCategories.Voice;

            commandsInSet = new List<ICommand> () {
                new Lock (),
                new Unlock (),
                new Invite (),
                new Kick (),
            };
        }

        public class Lock : ModuleCommand<VoiceLockingModule> {

            public Lock() {
                Name = "lock";
                Description = "Lock your voicechannel.";
                Category = LockingCategory;
            }

            [Overload (typeof (void), "Lock the voicechannel you're currently in.")]
            public Task<Result> Execute(CommandMetadata data) {
                if (data.Message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (!ParentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        ParentModule.LockChannel (guildUser.VoiceChannel, guildUser.VoiceChannel.Users);
                        return TaskResult (null, $"Channel **{guildUser.VoiceChannel.Name}** succesfully locked!");
                    } else {
                        return TaskResult (null, $"Error - Channel **{guildUser.VoiceChannel.Name}** is already locked.");
                    }
                }
                return result;
            }
        }

        public class Unlock : ModuleCommand<VoiceLockingModule> {

            public Unlock() {
                Name = "unlock";
                Description = "Unlock your voicechannel.";
                Category = LockingCategory;
            }

            [Overload (typeof (void), "Unlock the voicechannel you're currently in.")]
            public Task<Result> Execute(CommandMetadata data) {
                if (data.Message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (ParentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        ParentModule.UnlockChannel (guildUser.VoiceChannel);
                        return TaskResult (null, $"Channel **{guildUser.VoiceChannel.Name}** succesfully unlocked!");
                    } else {
                        return TaskResult (null, $"Error - Channel **{guildUser.VoiceChannel.Name}** isn't locked.");
                    }
                }
                return result;
            }
        }

        public class Invite : ModuleCommand<VoiceLockingModule> {

            public Invite() {
                Name = "invite";
                Description = "Invite someone to your locked channel.";
                Category = LockingCategory;

            }

            [Overload (typeof (void), "Invite someone to your currently locked voice channel.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                if (data.Message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (ParentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        ParentModule.GetLock (guildUser.VoiceChannel).AddMember (user);
                        return TaskResult (null, $"Channel **{user.GetShownName ()}** succesfully invited!");
                    } else {
                        return TaskResult (null, $"Error - Channel **{guildUser.VoiceChannel.Name}** isn't locked.");
                    }
                }
                return result;
            }
        }

        public class Kick : ModuleCommand<VoiceLockingModule> {

            public Kick() {
                Name = "kick";
                Description = "Kick someone from your locked channel.";
                Category = LockingCategory;
            }

            [Overload (typeof (void), "Kick someone from your currently locked voice channel.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                if (data.Message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (ParentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        ParentModule.GetLock (guildUser.VoiceChannel).KickMember (user);
                        return TaskResult (null, $"Channel **{user.GetShownName ()}** succesfully kicked!");
                    } else {
                        return TaskResult (null, $"Error - Channel **{guildUser.VoiceChannel.Name}** isn't locked.");
                    }
                }
                return result;
            }
        }
    }
}
