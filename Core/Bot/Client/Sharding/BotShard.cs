using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using System.Globalization;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class BotShard {

        public DateTime BootDate { get; private set; }
        public DateTime ConnectDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }
        public TimeSpan ConnectionUptime { get => DateTime.Now - ConnectDate; }

        public BotClient BotClient { get; private set; }

        public DiscordSocketClient Client { get; private set; }
        private readonly List<GuildHandler> _guildHandlers = new List<GuildHandler>();
        public IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }
        public bool IsConnected => Client != null && (Client.ConnectionState == ConnectionState.Connected || Client.ConnectionState == ConnectionState.Connecting);

        private Thread _thread;

        private readonly string _token;
        public readonly int ShardId;
        public readonly int TotalShards;
        public readonly ulong UniqueId;

        public event Func<Exception, Task> ExceptionCaught;

        private static ulong _uniqueShardIdCounter;

        internal BotShard(BotClient parentManager, string token, int shardId, int totalShards) {
            BotClient = parentManager;
            _token = token;
            ShardId = shardId;
            TotalShards = totalShards;
            UniqueId = _uniqueShardIdCounter++;
        }

        internal void Run () {
            void init() => Initialize().GetAwaiter().GetResult();
            _thread = new Thread (init) {
                Name = $"S{ShardId + 1}/{TotalShards}",
            };
            _thread.Start ();
        }

        internal Task Initialize () {
            Log.Bot($"Initializing shard {ShardId}.");
            BootDate = DateTime.Now;
            return Task.CompletedTask;
        }

        private DiscordSocketClient CreateClient ()
        {
            Log.Bot($"Initializing Discord Client for shard {ShardId}.");

            DiscordSocketConfig config = new DiscordSocketConfig
            {
                ShardId = ShardId,
                TotalShards = TotalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            };

            var client = new DiscordSocketClient(config);

            client.Ready += Client_Ready;
            client.JoinedGuild += Client_JoinedGuild;
            client.LeftGuild += Client_LeftGuild;
            client.Disconnected += Client_Disconnected;
            client.Connected += Client_Connected;
            client.Log += Client_Log;

            client.GuildMembersDownloaded += Client_GuildMembersDownloaded;

            return client;
        }

        private Task Client_Log(LogMessage arg)
        {
            if (arg.Exception == null)
            {
                Log.Client($"[{arg.Severity}] {arg.Message} from {arg.Source}");
            }
            else
            {
                Log.Exception(arg.Exception);
            }
            return Task.CompletedTask;
        }

        private Task Client_Connected()
        {
            Log.Write(Log.Type.BOT, $"Shard {ShardId} connected.");
            return Task.CompletedTask;
        }

        private Task Client_GuildMembersDownloaded(SocketGuild arg)
        {
            Log.Server(arg + " member downloaded.");
            return Task.CompletedTask;
        }

        private async Task Client_LeftGuild(SocketGuild arg)
        {
            await _guildHandlers.Find(x => x.GuildId == arg.Id).OnLeftGuild();
            KillGuildHandler(arg);
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            var handler = InitGuildHandler(arg);
            await handler.OnJoinedGuild();
        }

        private async Task Client_Disconnected(Exception arg)
        {
            Log.Warning($"Shard {ShardId} disconnected.");
            Log.Exception(arg);
            OnExceptionCaught(arg);

            await Task.CompletedTask;
        }

        internal async Task<bool> Connect ()
        {
            ConnectDate = DateTime.Now;
            Client = CreateClient();
            
            Log.Bot($"Connecting shard {ShardId}..");
            if (await TryLogin(6, 10000)) // Try logging in six times every ten seconds
            {
                await Start();
                await AwaitConnected();

                InitHandlers();
                RouteEvents();
                return true;
            }
            else
            {
                Log.Critical($"Shard {ShardId} has failed to login after several attempts. Something is wrong, perhaps Discord cannot be reached.");
                return false;
            }
        }

        internal GuildHandler[] GetGuildHandlers () => _guildHandlers.ToArray();

        private void InitHandlers ()
        {
            foreach (SocketGuild guild in Client.Guilds)
            {
                if (!_guildHandlers.Any (x => x.GuildId == guild.Id))
                {
                    InitGuildHandler(guild);
                }
            }
        }

        private GuildHandler InitGuildHandler (SocketGuild guild)
        {
            GuildHandler handler = new GuildHandler(this, guild.Id);
            handler.Initialize();
            _guildHandlers.Add(handler);
            return handler;
        }

        private void KillGuildHandler (SocketGuild guild)
        {
            GuildHandler handler = _guildHandlers.Find(x => x.GuildId == guild.Id);
            handler.Kill();
            _guildHandlers.Remove(handler);
        }

        private Task Client_Ready() {
            Log.Write (Log.Type.BOT, $"Shard {ShardId} is ready!");
            return Task.CompletedTask;
        }

        private async Task Start () {
            await Client.StartAsync ();
            Log.Write (Log.Type.BOT, $"Shard {ShardId} started.");
        }

        private async Task Stop() {
            await Client.StopAsync ();
            Log.Write (Log.Type.BOT, $"Shard {ShardId} stopped.");
        }

        private async Task<bool> TryLogin (int attempts, int delayMillis)
        {
            while (attempts > 0)
            {
                try
                {
                    await Login();
                    return true;
                } catch (Exception)
                {
                    Log.Bot($"Reattempting login of Shard {ShardId} in 10 seconds.");
                    await Task.Delay(delayMillis);
                    attempts--;
                }
            }
            return false;
        }

        private async Task Login () {
            Log.Bot($"Attempting login of shard {ShardId}..");
            try
            {
                await Client.LoginAsync(TokenType.Bot, _token);
                Log.Write (Log.Type.BOT, $"Shard {ShardId} logged in.");
            } catch (Exception exc)
            {
                Log.Bot($"Shard {ShardId} failed to login:");
                Log.Exception(exc);
                throw;
            }
        }

        private async Task Logout () {
            await Client.LogoutAsync ();
            Log.Write (Log.Type.BOT, $"Shard {ShardId} logged out.");
        }

        internal async Task Kill () {
            Log.Write (Log.Type.CRITICAL, $"KILLING SHARD {ShardId}!");
            await Logout ();
            await Stop ();
            Client.Dispose ();
            _thread.Abort();
        }

        private void RouteEvents () {  // It'd almost be worth writing a script to type this shiznat out automatically.
            Client.MessageReceived          += async (x) =>         { try { await ForGuild ((x.Channel as SocketTextChannel)?.Guild, g => g.OnMessageRecieved (x));     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelCreated           += async (x) =>         { try { await ForGuild ((x as SocketGuildChannel)?.Guild, g => g.OnChannelCreated(x));              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelDestroyed         += async (x) =>         { try { await ForGuild ((x as SocketGuildChannel)?.Guild, g => g.OnChannelDestroyed(x));            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelUpdated           += async (x, y) =>      { try { await ForGuild ((x as SocketGuildChannel)?.Guild, g => g.OnChannelUpdated(x, y));           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageDeleted           += async (x, y) =>      { try { await ForGuild ((await y.GetOrDownloadAsync() as SocketTextChannel)?.Guild, async g => await g.OnMessageDeleted(x, y));              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageUpdated           += async (x, y, z) =>   { try { await ForGuild ((z as SocketTextChannel)?.Guild, g => g.OnMessageUpdated (x, y, z));        } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionAdded            += async (x, y, z) =>   { try { await ForGuild ((await y.GetOrDownloadAsync() as SocketTextChannel)?.Guild, async g => await g.OnReactionAdded (x, y, z));          } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionRemoved          += async (x, y, z) =>   { try { await ForGuild ((await y.GetOrDownloadAsync() as SocketTextChannel)?.Guild, async g => await g.OnReactionRemoved (x, y, z));        } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionsCleared         += async (x, y) =>      { try { await ForGuild ((await y.GetOrDownloadAsync() as SocketTextChannel)?.Guild, async g => await g.OnReactionsCleared (x,y));            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserIsTyping             += async (x, y) =>      { try { await ForGuild ((await y.GetOrDownloadAsync() as SocketTextChannel)?.Guild, async g => await g.OnUserIsTyping (x, y));               } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserVoiceStateUpdated    += async (x, y, z) =>   { try { await ForGuild ((x as SocketGuildUser)?.Guild, g => g.OnUserVoiceStateUpdated (x, y, z));   } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Connected                += async () =>          { try { await ForEachGuild (g => g.OnConnected ());                                                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.CurrentUserUpdated       += async (x, y) =>      { try { await ForEachGuild (g => g.OnCurrentUserUpdated(x, y));                                     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Disconnected             += async (x) =>         { try { await ForEachGuild (g => g.OnDisconnected(x));                                              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LatencyUpdated           += async (x, y) =>      { try { await ForEachGuild (g => g.OnLatencyUpdated (x, y));                                        } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Log                      += async (x) =>         { try { await ForEachGuild (g => g.OnLog (x));                                                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LoggedIn                 += async () =>          { try { await ForEachGuild (g => g.OnLoggedIn ());                                                  } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LoggedOut                += async () =>          { try { await ForEachGuild (g => g.OnLoggedOut ());                                                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Ready                    += async () =>          { try { await ForEachGuild (g => g.OnReady ());                                                     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildMemberUpdated       += async (x, y) =>      { try { await ForGuild ((await x.GetOrDownloadAsync()).Guild, async g => await g.OnGuildMemberUpdated (x, y));   } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleCreated              += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnRoleCreated (x));                                         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleDeleted              += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnRoleDeleted (x));                                         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleUpdated              += async (x, y) =>      { try { await ForGuild (x.Guild, g => g.OnRoleUpdated (x, y));                                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserJoined               += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnUserJoined (x));                                          } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserLeft                 += async (x, y) =>      { try { await ForGuild (x, g => g.OnUserLeft (y));                                                  } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildAvailable           += async (x) =>         { try { await ForGuild (x, g => g.OnGuildAvailable());                                              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildMembersDownloaded   += async (x) =>         { try { await ForGuild (x, g => g.OnGuildMembersDownloaded ());                                     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildUnavailable         += async (x) =>         { try { await ForGuild (x, g => g.OnGuildUnavailable ());                                           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildUpdated             += async (x, y) =>      { try { await ForGuild (x, g => g.OnGuildUpdated (x, y));                                           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserBanned               += async (x, y) =>      { try { await ForGuild (y, g => g.OnUserBanned (x));                                                } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserUnbanned             += async (x, y) =>      { try { await ForGuild (y, g => g.OnUserUnbanned (x));                                              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ApplicationCommandCreated += async (x) =>        { try { await (x.Guild == null ? ForEachGuild(y => y.OnApplicationCommandCreated(x)) : ForGuild(x.Guild, y => y.OnApplicationCommandCreated(x))); } catch (Exception exc) { OnExceptionCaught(exc); } };
            Client.ApplicationCommandDeleted += async (x) =>        { try { await (x.Guild == null ? ForEachGuild(y => y.OnApplicationCommandDeleted(x)) : ForGuild(x.Guild, y => y.OnApplicationCommandDeleted(x))); } catch (Exception exc) { OnExceptionCaught(exc); } };
            Client.ApplicationCommandUpdated += async (x) =>        { try { await (x.Guild == null ? ForEachGuild(y => y.OnApplicationCommandUpdated(x)) : ForGuild(x.Guild, y => y.OnApplicationCommandUpdated(x))); } catch (Exception exc) { OnExceptionCaught(exc); } };
        }

        private Task ForGuild (ulong? guildId, Func<GuildHandler, Task> func)
        {
            if (!guildId.HasValue)
            {
                return Task.CompletedTask;
            }

            GuildHandler handler = _guildHandlers.Find(x => x.GuildId == guildId.Value);
            if (handler == null) // GuildHandler is missing for whatever reason. Perhaps the guild was added while the bot was temporarily offline.
            {
                InitGuildHandler(GetGuild(guildId.Value));
            }

            func(handler);
            return Task.CompletedTask;
        }

        private async Task ForGuild(SocketGuild guild, Func<GuildHandler, Task> func) => await ForGuild(guild?.Id, func);

        private Task ForEachGuild (Func<GuildHandler, Task> func)
        {
            Task[] tasks = new Task[_guildHandlers.Count];
            for (int i = 0; i < _guildHandlers.Count; i++)
            {
                tasks[i] = func(_guildHandlers[i]);
            }
            Task.WhenAll(tasks);
            return Task.CompletedTask;
        }

        private void OnExceptionCaught(Exception exception) {
            _ = ExceptionCaught?.Invoke (exception.InnerException ?? exception).ConfigureAwait (false);
        }

        // Status related stuff
        public async Task AwaitConnected () {
            while (!IsConnected) {
                await Task.Delay (1000);
            }
        }

        public string GetIndexString ()
            => $"Shard {ShardId + 1}/{TotalShards}";

        public override string ToString () {
            return $"GuildHandlers: {_guildHandlers.Count}\nPlugin Instances: {_guildHandlers.Sum (x => x.Plugins.ActiveCount)}\nUsers: {Client.Guilds.Sum (x => x.MemberCount)}\nLogin state: {Client.LoginState}\nConnection: {Client.ConnectionState}\nConnection Uptime: {ConnectionUptime.ToString("%d\\:%h", CultureInfo.InvariantCulture)}\nLatency: {Client.Latency}\nUptime: {Uptime.ToString ("%d\\:%h", CultureInfo.InvariantCulture)}\n";
        }

        // Guild item getters.
        internal SocketGuild              GetGuild(ulong id)                                  => Client.GetGuild (id);
        internal SocketGuildUser          GetUser(ulong guildId, ulong userId)                => GetGuild (guildId)?.GetUser (userId);
        internal SocketGuildChannel       GetChannel(ulong guildId, ulong channelId)          => GetGuild (guildId)?.GetChannel (channelId);
        internal SocketTextChannel        GetTextChannel(ulong guildId, ulong channelId)      => GetChannel (guildId, channelId) as SocketTextChannel;
        internal SocketVoiceChannel       GetVoiceChannel (ulong guildId, ulong channelId)    => GetChannel (guildId, channelId) as SocketVoiceChannel;
        internal SocketCategoryChannel    GetCategoryChannel(ulong guildId, ulong channelId)  => GetChannel (guildId, channelId) as SocketCategoryChannel;
        internal SocketRole               GetRole(ulong guildId, ulong roleId)                => GetGuild (guildId)?.GetRole (roleId);

    }
}
