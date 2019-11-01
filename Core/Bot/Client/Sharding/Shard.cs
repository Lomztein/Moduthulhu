using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Plugin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class Shard {

        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        public BotClient BotClient { get; private set; }
        internal ClientManager ClientManager { get => BotClient.ClientManager; }

        public DiscordSocketClient Client { get; private set; }
        private List<GuildHandler> _guildHandlers = new List<GuildHandler>();
        public IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }
        public bool IsConnected { get => Guilds.Count > 0 && Guilds.First ().IsConnected; }

        private Thread _thread;

        private string _token;
        private int _shardId;
        private int _totalShards;

        public event Func<Exception, Task> ExceptionCaught;

        internal Shard(BotClient parentManager, string token, int shardId, int totalShards) {
            BotClient = parentManager;
            _token = token;
            _shardId = shardId;
            _totalShards = totalShards;
        }

        internal void Run () {
            ThreadStart init = new ThreadStart (Initialize);
            _thread = new Thread (init) {
                Name = ToString (),
            };
            _thread.Start ();
        }

        internal async void Initialize () {

            BootDate = DateTime.Now;
            DiscordSocketConfig config = new DiscordSocketConfig () {
                ShardId = _shardId,
                TotalShards = _totalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            };

            Client = new DiscordSocketClient (config);
            RouteEvents ();

            Client.Ready += Client_Ready;
            Client.JoinedGuild += InitGuildHandler;
            Client.LeftGuild += KillGuildHandler;
            Client.Disconnected += Client_Disconnected;

            await Start ();
            await Login ();
            await AwaitConnected ();
        }

        private Task Client_Disconnected(Exception arg)
        {
            Log.Write(arg);
            return Task.CompletedTask;
        }

        private Task InitGuildHandler (SocketGuild guild)
        {
            GuildHandler handler = new GuildHandler(this, guild.Id);
            handler.Initialize();
            _guildHandlers.Add(handler);
            return Task.CompletedTask;
        }

        private Task KillGuildHandler (SocketGuild guild)
        {
            GuildHandler handler = _guildHandlers.Find(x => x.GuildId == guild.Id);
            handler.Kill();
            _guildHandlers.Remove(handler);
            return Task.CompletedTask;
        }

        private Task Client_Ready() {
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} is ready and connected.");
            return Task.CompletedTask;
        }

        private async Task Start () {
            await Client.StartAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} started.");
        }

        private async Task Stop() {
            await Client.StopAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} stopped.");
        }

        private async Task Login () {
            await Client.LoginAsync (TokenType.Bot, _token);
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} logged in.");
        }

        private async Task Logout () {
            await Client.LogoutAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} logged out in.");
        }

        internal async Task Kill () {
            Log.Write (Log.Type.CRITICAL, $"KILLING CLIENT {BotClient.Name} SHARD {_shardId}!");
            await Logout ();
            await Stop ();
            Client.Dispose ();
        }

        public override string ToString() => $"{BotClient} - S{_shardId}/{_totalShards}";

        private void RouteEvents () {  // It'd almost be worth writing a script to type this shiznat out automatically.
            Client.MessageReceived          += async (x) =>         { try { await ForGuild ((x.Channel as SocketTextChannel)?.Guild, g => g.OnMessageRecieved (x));     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelCreated           += async (x) =>         { try { await ForGuild ((x as SocketGuildChannel)?.Guild, g => g.OnChannelCreated(x));              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelDestroyed         += async (x) =>         { try { await ForGuild ((x as SocketGuildChannel)?.Guild, g => g.OnChannelDestroyed(x));            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelUpdated           += async (x, y) =>      { try { await ForGuild ((x as SocketGuildChannel)?.Guild, g => g.OnChannelUpdated(x, y));           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageDeleted           += async (x, y) =>      { try { await ForGuild ((y as SocketTextChannel)?.Guild, g => g.OnMessageDeleted (x, y));           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageUpdated           += async (x, y, z) =>   { try { await ForGuild ((z as SocketTextChannel)?.Guild, g => g.OnMessageUpdated (x, y, z));        } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionAdded            += async (x, y, z) =>   { try { await ForGuild ((y as SocketTextChannel)?.Guild, g => g.OnReactionAdded (x, y, z));         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionRemoved          += async (x, y, z) =>   { try { await ForGuild ((y as SocketTextChannel)?.Guild, g => g.OnReactionRemoved (x, y, z));       } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionsCleared         += async (x, y) =>      { try { await ForGuild ((y as SocketTextChannel)?.Guild, g => g.OnReactionsCleared (x, y));         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserIsTyping             += async (x, y) =>      { try { await ForGuild ((y as SocketTextChannel)?.Guild, g => g.OnUserIsTyping (x, y));             } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Connected                += async () =>          { try { await ForEachGuild (g => g.OnConnected ());                                                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.CurrentUserUpdated       += async (x, y) =>      { try { await ForEachGuild (g => g.OnCurrentUserUpdated(x, y));                                     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Disconnected             += async (x) =>         { try { await ForEachGuild (g => g.OnDisconnected(x));                                              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LatencyUpdated           += async (x, y) =>      { try { await ForEachGuild (g => g.OnLatencyUpdated (x, y));                                        } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Log                      += async (x) =>         { try { await ForEachGuild (g => g.OnLog (x));                                                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LoggedIn                 += async () =>          { try { await ForEachGuild (g => g.OnLoggedIn ());                                                  } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LoggedOut                += async () =>          { try { await ForEachGuild (g => g.OnLoggedOut ());                                                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Ready                    += async () =>          { try { await ForEachGuild (g => g.OnReady ());                                                     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildMemberUpdated       += async (x, y) =>      { try { await ForGuild (x.Guild, g => g.OnGuildMemberUpdated (x, y));                               } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleCreated              += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnRoleCreated (x));                                         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleDeleted              += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnRoleDeleted (x));                                         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleUpdated              += async (x, y) =>      { try { await ForGuild (x.Guild, g => g.OnRoleUpdated (x, y));                                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserJoined               += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnUserJoined (x));                                          } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserLeft                 += async (x) =>         { try { await ForGuild (x.Guild, g => g.OnUserLeft (x));                                            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildAvailable           += async (x) =>         { try { await ForGuild (x, g => g.OnGuildAvailable());                                              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildMembersDownloaded   += async (x) =>         { try { await ForGuild (x, g => g.OnGuildMembersDownloaded ());                                     } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildUnavailable         += async (x) =>         { try { await ForGuild (x, g => g.OnGuildUnavailable ());                                           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildUpdated             += async (x, y) =>      { try { await ForGuild (x, g => g.OnGuildUpdated (x, y));                                           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.JoinedGuild              += async (x) =>         { try { await ForGuild (x, g => g.OnJoinedGuild ());                                                } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserBanned               += async (x, y) =>      { try { await ForGuild (y, g => g.OnUserBanned (x));                                                } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserUnbanned             += async (x, y) =>      { try { await ForGuild (y, g => g.OnUserUnbanned (x));                                              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserVoiceStateUpdated    += async (x, y, z) =>   { try { await ForGuild ((x as SocketGuildUser)?.Guild, g => g.OnUserVoiceStateUpdated (x, y, z));   } catch (Exception exc) { OnExceptionCaught (exc); } };
            //Client.UserUpdated              += async (x, y) =>      { try { await ForGuild ((x as SocketGuildUser)?.Guild, g => g.OnMem (UserUpdated,x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
        }

        private async Task ForGuild (ulong? guildId, Func<GuildHandler, Task> func)
        {
            if (!guildId.HasValue)
                return;

            GuildHandler handler = _guildHandlers.Find(x => x.GuildId == guildId.Value);
            await func(handler);
        }

        private async Task ForGuild(SocketGuild guild, Func<GuildHandler, Task> func) => await ForGuild(guild?.Id, func);

        private async Task ForEachGuild (Func<GuildHandler, Task> func)
        {
            Task[] tasks = new Task[_guildHandlers.Count];
            for (int i = 0; i < _guildHandlers.Count; i++)
            {
                tasks[i] = func(_guildHandlers[i]);
            }
            await Task.WhenAll(tasks);
        }

        private async void OnExceptionCaught(Exception exception) {
            await ExceptionCaught?.Invoke (exception.InnerException);
        }

        // Status related stuff
        public async Task AwaitConnected () {
            while (!IsConnected) {
                await Task.Delay (100);
            }
        }

        // TODO: Change all these to extension methods since that'll better support null-cases.
        public string GetStatusString () {
            return $" -- SHARD {_shardId} -- \nGuilds: {Client.Guilds.Count}\nUsers: {Client.Guilds.Sum (x => x.MemberCount)}\nLogin state: {Client.LoginState}\nConnection: {Client.ConnectionState}\nLatency: {Client.Latency}\nUptime: {Uptime}\n";
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
