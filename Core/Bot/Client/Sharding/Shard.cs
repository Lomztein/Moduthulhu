using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot.Client;
using Lomztein.Moduthulhu.Core.Module;
using Lomztein.Moduthulhu.Cross;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class Shard {

        internal BotClient BotClient { get; private set; }
        internal ClientManager ClientManager { get => BotClient.ClientManager; }
        internal Core Core { get => BotClient.Core; }

        internal ModuleLoader ModuleLoader { get => BotClient.Core.ModuleLoader; }
        internal ModuleContainer ModuleContainer { get; private set; }

        public DiscordSocketClient Client { get; private set; }
        public IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }
        public bool IsConnected { get => Guilds.Count > 0; }

        private Thread Thread { get; set; }

        public int ShardId { get; private set; }

        // WRAPPED DISCORD EVENTS //
        public event Func<SocketChannel, Task> ChannelCreated;
        public event Func<SocketChannel, Task> ChannelDestroyed;
        public event Func<SocketChannel, SocketChannel, Task> ChannelUpdated;
        public event Func<SocketGuild, Task> GuildAvailable;
        public event Func<SocketGuild, Task> GuildMembersDownloaded;
        public event Func<SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated;
        public event Func<SocketGuild, Task> GuildUnavailable;
        public event Func<SocketGuild, SocketGuild, Task> GuildUpdated;
        public event Func<SocketGuild, Task> JoinedGuild;
        public event Func<SocketGuild, Task> LeftGuild;
        public event Func<Cacheable<IMessage, ulong>, ISocketMessageChannel, Task> MessageDeleted;
        public event Func<SocketMessage, Task> MessageReceived;
        public event Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task> MessageUpdated;
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, Task> ReactionsCleared;
        public event Func<SocketRole, Task> RoleCreated;
        public event Func<SocketRole, Task> RoleDeleted;
        public event Func<SocketRole, SocketRole, Task> RoleUpdated;
        public event Func<SocketUser, SocketGuild, Task> UserBanned;
        public event Func<SocketUser, ISocketMessageChannel, Task> UserIsTyping;
        public event Func<SocketGuildUser, Task> UserJoined;
        public event Func<SocketGuildUser, Task> UserLeft;
        public event Func<SocketUser, SocketGuild, Task> UserUnbanned;
        public event Func<SocketUser, SocketUser, Task> UserUpdated;
        public event Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated;
        // WRAPPED DISCORD EVENTS //

        public event Action<Exception> OnExceptionCaught;

        internal Shard(BotClient parentManager, int shardId) {
            BotClient = parentManager;
            ShardId = shardId;
        }

        internal void Begin () {
            ThreadStart init = new ThreadStart (Initialize);
            Thread = new Thread (init);
            Thread.Name = ToString ();
            Thread.Priority = ThreadPriority.AboveNormal;
        }

        internal async void Initialize () {

            DiscordSocketConfig config = new DiscordSocketConfig () {
                ShardId = ShardId,
                TotalShards = BotClient.TotalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            };
            Client = new DiscordSocketClient (config);

            await Start ();
            await Login ();
            await AwaitConnected ();

            ModuleContainer = new ModuleContainer (this);
            ModuleContainer.InstantiateModules ();
            ModuleContainer.InitializeModules ();
            InitializeErrorHandling ();

            Client.Ready += Client_Ready;
        }

        private Task Client_Ready() {
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} is ready and connected.");
            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage arg) {
            Log.Write (Log.Type.CHAT, arg.Content);
            return Task.CompletedTask;
        }

        private async Task Start () {
            await Client.StartAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} started.");
        }

        private async Task Stop() {
            await Client.StopAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} stopped.");
        }

        private async Task Login () {
            await Client.LoginAsync (TokenType.Bot, BotClient.Token);
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} logged in.");
        }

        private async Task Logout () {
            await Client.LogoutAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} logged out in.");
        }

        internal async Task Kill () {
            Log.Write (Log.Type.CRITICAL, $"KILLING CLIENT {BotClient.Name} SHARD {ShardId}!");
            await Logout ();
            await Stop ();
            Client.Dispose ();
        }

        public override string ToString() => $"{BotClient} - S{ShardId}/{BotClient.TotalShards}";

        private void InitializeErrorHandling() {
            Client.ChannelCreated           += async (x) =>         { try { await ChannelCreated?.Invoke (x);              } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.ChannelDestroyed         += async (x) =>         { try { await ChannelDestroyed?.Invoke (x);            } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.ChannelUpdated           += async (x, y) =>      { try { await ChannelUpdated?.Invoke (x, y);           } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.GuildAvailable           += async (x) =>         { try { await GuildAvailable?.Invoke (x);              } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.GuildMembersDownloaded   += async (x) =>         { try { await GuildMembersDownloaded?.Invoke (x);      } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.GuildMemberUpdated       += async (x, y) =>      { try { await GuildMemberUpdated?.Invoke (x, y);       } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.GuildUnavailable         += async (x) =>         { try { await GuildUnavailable?.Invoke (x);            } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.GuildUpdated             += async (x, y) =>      { try { await GuildUpdated?.Invoke (x, y);             } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.JoinedGuild              += async (x) =>         { try { await JoinedGuild?.Invoke (x);                 } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.LeftGuild                += async (x) =>         { try { await LeftGuild?.Invoke (x);                   } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.MessageDeleted           += async (x, y) =>      { try { await MessageDeleted?.Invoke (x, y);           } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.MessageReceived          += async (x) =>         { try { await MessageReceived?.Invoke (x);             } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.MessageUpdated           += async (x, y, z) =>   { try { await MessageUpdated?.Invoke (x, y, z);        } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.ReactionAdded            += async (x, y, z) =>   { try { await ReactionAdded?.Invoke (x, y, z);         } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.ReactionRemoved          += async (x, y, z) =>   { try { await ReactionRemoved?.Invoke (x, y, z);       } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.ReactionsCleared         += async (x, y) =>      { try { await ReactionsCleared?.Invoke (x, y);         } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.RoleCreated              += async (x) =>         { try { await RoleCreated?.Invoke (x);                 } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.RoleDeleted              += async (x) =>         { try { await RoleDeleted?.Invoke (x);                 } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.RoleUpdated              += async (x, y) =>      { try { await RoleUpdated?.Invoke (x, y);              } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserBanned               += async (x, y) =>      { try { await UserBanned?.Invoke (x, y);               } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserIsTyping             += async (x, y) =>      { try { await UserIsTyping?.Invoke (x, y);             } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserJoined               += async (x) =>         { try { await UserJoined?.Invoke (x);                  } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserLeft                 += async (x) =>         { try { await UserLeft?.Invoke (x);                    } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserUnbanned             += async (x, y) =>      { try { await UserUnbanned?.Invoke (x, y);             } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserUpdated              += async (x, y) =>      { try { await UserUpdated?.Invoke (x, y);              } catch (Exception exc) { ExceptionCaught (exc); } };
            Client.UserVoiceStateUpdated    += async (x, y, z) =>   { try { await UserVoiceStateUpdated?.Invoke (x, y, z); } catch (Exception exc) { ExceptionCaught (exc); } };
        }

        private async void ExceptionCaught (Exception exception) {
            Log.Write (exception);
            await Client.SetGameAsync (exception.Message + " - " + exception.GetType ().Name + " at " + exception.TargetSite.Name + " in " + exception.Source);
            OnExceptionCaught?.Invoke (exception);
        }

        // Status related stuff
        public async Task AwaitConnected () {
            while (!IsConnected) {
                await Task.Delay (100);
            }
        }

        // Guild item getters.
        public SocketGuild              GetGuild(ulong id)                                  => Client.GetGuild (id);
        public SocketGuildUser          GetUser(ulong guildId, ulong userId)                => GetGuild (guildId)?.GetUser (userId);
        public SocketGuildChannel       GetChannel(ulong guildId, ulong channelId)          => GetGuild (guildId)?.GetChannel (channelId);
        public SocketTextChannel        GetTextChannel(ulong guildId, ulong channelId)      => GetChannel (guildId, channelId) as SocketTextChannel;
        public SocketVoiceChannel       GetVoiceChannel (ulong guildId, ulong channelId)    => GetChannel (guildId, channelId) as SocketVoiceChannel;
        public SocketCategoryChannel    GetCategoryChannel(ulong guildId, ulong channelId)  => GetChannel (guildId, channelId) as SocketCategoryChannel;
        public SocketRole               GetRole(ulong guildId, ulong roleId)                => GetGuild (guildId)?.GetRole (roleId);

    }
}
