using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Voice.Commands {
    public class VoiceLockingSet : CommandSet {

        public VoiceLockingModule parentModule;

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

        public override void Initialize() {
            base.Initialize ();
            foreach (Command cmd in commandsInSet) {
                (cmd as LockCommandBase).parentModule = parentModule;
            }
        }

        public abstract class LockCommandBase : Command {
            public VoiceLockingModule parentModule;
        }

        public class Lock : LockCommandBase {

            public Lock() {
                command = "lock";
                shortHelp = "Lock your voicechannel.";
            }

            [Overload (typeof (void), "Lock the voicechannel you're currently in.")]
            public Task<Result> Execute(CommandMetadata data) {
                SocketGuildUser author = data.message.Author as SocketGuildUser;
                if (author.VoiceChannel != null) {

                    parentModule.LockChannel (author.VoiceChannel, author.VoiceChannel.Users);
                    return TaskResult (null, $"Voice channel **{author.VoiceChannel.Name}** succesfully locked!");
                } else
                    return TaskResult (null, $"You're gonna need to be in a channel to do that.");
            }
        }

        public class Unlock : LockCommandBase {

            public Unlock() {
                command = "unlock";
                shortHelp = "Unlock your voicechannel.";
            }

            [Overload (typeof (void), "Unlock the voicechannel you're currently in.")]
            public Task<Result> Execute(CommandMetadata data) {
                SocketGuildUser author = data.message.Author as SocketGuildUser;
                if (author.VoiceChannel != null) {

                    parentModule.UnlockChannel (author.VoiceChannel);
                    return TaskResult (null, $"Voice channel **{author.VoiceChannel.Name}** succesfully unlocked!");
                } else
                    return TaskResult (null, $"You're gonna need to be in a channel to do that.");
            }
        }

        public class Invite : LockCommandBase {

            public Invite() {
                command = "invite";
                shortHelp = "Invite someone to your locked channel.";
            }

            [Overload (typeof (void), "Invite someone to your currently locked voice channel.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                SocketGuildUser author = data.message.Author as SocketGuildUser;
                if (author.VoiceChannel != null && parentModule.IsChannelLocked (author.VoiceChannel)) {
                    parentModule.GetLock (author.VoiceChannel)?.AddMember (user);
                    return TaskResult (null, $"Succesfully invited **{user.GetShownName ()}** to **{author.VoiceChannel.Name}!**");
                } else
                    return TaskResult (null, $"You're gonna need to be in a locked channel to do that.");
            }
        }

        public class Kick : LockCommandBase {

            public Kick() {
                command = "kick";
                shortHelp = "Kick someone from your locked channel.";
            }

            [Overload (typeof (void), "Kick someone from your currently locked voice channel.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                SocketGuildUser author = data.message.Author as SocketGuildUser;
                if (author.VoiceChannel != null && parentModule.IsChannelLocked (author.VoiceChannel)) {
                    parentModule.GetLock (author.VoiceChannel)?.KickMember (user);
                    return TaskResult (null, $"Succesfully kicked **{user.GetShownName ()}** from **{author.VoiceChannel.Name}.**");
                } else
                    return TaskResult (null, $"You're gonna need to be in a locked channel to do that.");
            }
        }
    }
}
