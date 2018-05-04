using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.Voice.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice.Commands {
    public class VoiceLockingSet : ModuleCommandSet<VoiceLockingModule> {

        public VoiceLockingSet() {
            command = "voice"; // Name is temporary untill set merging is running.
            shortHelp = "A command set for locking voice channels.";
            catagory = Category.Utility;

            commandsInSet = new List<Command> () {
                new Lock (),
                new Unlock (),
                new Invite (),
                new Kick (),
            };
        }

        public class Lock : ModuleCommand<VoiceLockingModule> {

            public Lock() {
                command = "lock";
                shortHelp = "Lock your voicechannel.";
                catagory = Category.Utility;
            }

            [Overload (typeof (void), "Lock the voicechannel you're currently in.")]
            public Task<Result> Execute(CommandMetadata data) {
                if (data.message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (!parentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        parentModule.LockChannel (guildUser.VoiceChannel, guildUser.VoiceChannel.Users);
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
                command = "unlock";
                shortHelp = "Unlock your voicechannel.";
                catagory = Category.Utility;
            }

            [Overload (typeof (void), "Unlock the voicechannel you're currently in.")]
            public Task<Result> Execute(CommandMetadata data) {
                if (data.message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (!parentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        parentModule.UnlockChannel (guildUser.VoiceChannel);
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
                command = "invite";
                shortHelp = "Invite someone to your locked channel.";
                catagory = Category.Utility;

            }

            [Overload (typeof (void), "Invite someone to your currently locked voice channel.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                if (data.message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (parentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        parentModule.GetLock (guildUser.VoiceChannel).AddMember (user);
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
                command = "kick";
                shortHelp = "Kick someone from your locked channel.";
                catagory = Category.Utility;
            }

            [Overload (typeof (void), "Kick someone from your currently locked voice channel.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                if (data.message.Author.IsInVoiceChannel (out Task<Result> result, out SocketGuildUser guildUser)) {
                    if (parentModule.IsChannelLocked (guildUser.VoiceChannel)) {
                        parentModule.GetLock (guildUser.VoiceChannel).KickMember (user);
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
