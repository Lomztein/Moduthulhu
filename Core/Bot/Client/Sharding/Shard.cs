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
        public IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }
        public bool IsConnected { get => Guilds.Count > 0 && Guilds.First ().IsConnected; }

        private Thread _thread;

        private string _token;
        private int _shardId;
        private int _totalShards;

        // WRAPPED DISCORD EVENTS //
        public event Func<SocketChannel, Task> ChannelCreated;
        public event Func<SocketChannel, Task> ChannelDestroyed;
        public event Func<SocketChannel, SocketChannel, Task> ChannelUpdated;
        public event Func<Task> Connected; // Done
        public event Func<SocketSelfUser, SocketSelfUser, Task> CurrentUserUpdated; // Done
        public event Func<Exception, Task> Disconnected; // Done
        public event Func<SocketGuild, Task> GuildAvailable;
        public event Func<SocketGuild, Task> GuildMembersDownloaded;
        public event Func<SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated;
        public event Func<SocketGuild, Task> GuildUnavailable;
        public event Func<SocketGuild, SocketGuild, Task> GuildUpdated;
        public event Func<SocketGuild, Task> JoinedGuild;
        public event Func<int, int, Task> LatencyUpdated; // Done
        public event Func<SocketGuild, Task> LeftGuild;
        public event Func<LogMessage, Task> Log; // Done
        public event Func<Task> LoggedIn; // Done
        public event Func<Task> LoggedOut; // Done
        public event Func<Cacheable<IMessage, ulong>, ISocketMessageChannel, Task> MessageDeleted;
        public event Func<SocketMessage, Task> MessageReceived;
        public event Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task> MessageUpdated;
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, Task> ReactionsCleared;
        public event Func<Task> Ready; // New
        public event Func<SocketGroupUser, Task> RecipientAdded; // New
        public event Func<SocketGroupUser, Task> RecipientRemoved; // New
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

        public event Func<Exception, Task> ExceptionCaught;

        internal Shard(BotClient parentManager, int shardId, int totalShards) {
            BotClient = parentManager;
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
            WrapClientEvents (Client);

            Client.Ready += Client_Ready;

            await Start ();
            await Login ();
            await AwaitConnected ();
        }

        private Task Client_Ready() {
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} is ready and connected.");
            return Task.CompletedTask;
        }

        private async Task Start () {
            await Client.StartAsync ();
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} started.");
        }

        private async Task Stop() {
            await Client.StopAsync ();
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} stopped.");
        }

        private async Task Login () {
            await Client.LoginAsync (TokenType.Bot, _token);
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} logged in.");
        }

        private async Task Logout () {
            await Client.LogoutAsync ();
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {_shardId} logged out in.");
        }

        internal async Task Kill () {
            Cross.Log.Write (Cross.Log.Type.CRITICAL, $"KILLING CLIENT {BotClient.Name} SHARD {_shardId}!");
            await Logout ();
            await Stop ();
            Client.Dispose ();
        }

        public override string ToString() => $"{BotClient} - S{_shardId}/{_totalShards}";

        private void WrapClientEvents (DiscordSocketClient client) {  // It'd almost be worth writing a script to type this shiznat out automatically.
            client.ChannelCreated           += async (x) =>         { try { await HackyInvoke (ChannelCreated, x);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.ChannelDestroyed         += async (x) =>         { try { await HackyInvoke (ChannelDestroyed, x);            } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.ChannelUpdated           += async (x, y) =>      { try { await HackyInvoke (ChannelUpdated, x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.Connected                += async () =>          { try { await HackyInvoke (Connected);                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.CurrentUserUpdated       += async (x, y) =>      { try { await HackyInvoke (CurrentUserUpdated, x, y);       } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.Disconnected             += async (x) =>         { try { await HackyInvoke (Disconnected, x);                } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.GuildAvailable           += async (x) =>         { try { await HackyInvoke (GuildAvailable, x);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.GuildMembersDownloaded   += async (x) =>         { try { await HackyInvoke (GuildMembersDownloaded, x);      } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.GuildMemberUpdated       += async (x, y) =>      { try { await HackyInvoke (GuildMemberUpdated, x, y);       } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.GuildUnavailable         += async (x) =>         { try { await HackyInvoke (GuildUnavailable, x);            } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.GuildUpdated             += async (x, y) =>      { try { await HackyInvoke (GuildUpdated, x, y);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.JoinedGuild              += async (x) =>         { try { await HackyInvoke (JoinedGuild, x);                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.LatencyUpdated           += async (x, y) =>      { try { await HackyInvoke (LatencyUpdated, x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.LeftGuild                += async (x) =>         { try { await HackyInvoke (LeftGuild, x);                   } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.Log                      += async (x) =>         { try { await HackyInvoke (Log, x);                         } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.LoggedIn                 += async () =>          { try { await HackyInvoke (LoggedIn);                       } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.LoggedOut                += async () =>          { try { await HackyInvoke (LoggedOut);                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.MessageDeleted           += async (x, y) =>      { try { await HackyInvoke (MessageDeleted, x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.MessageReceived          += async (x) =>         { try { await HackyInvoke (MessageReceived, x);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.MessageUpdated           += async (x, y, z) =>   { try { await HackyInvoke (MessageUpdated, x, y, z);        } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.ReactionAdded            += async (x, y, z) =>   { try { await HackyInvoke (ReactionAdded, x, y, z);         } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.ReactionRemoved          += async (x, y, z) =>   { try { await HackyInvoke (ReactionRemoved, x, y, z);       } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.Ready                    += async () =>          { try { await HackyInvoke (Ready);                          } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.RecipientAdded           += async (x) =>         { try { await HackyInvoke (RecipientAdded, x);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.ReactionsCleared         += async (x, y) =>      { try { await HackyInvoke (ReactionsCleared, x, y);         } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.RecipientRemoved         += async (x) =>         { try { await HackyInvoke (RecipientRemoved, x);            } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.RoleCreated              += async (x) =>         { try { await HackyInvoke (RoleCreated, x);                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.RoleDeleted              += async (x) =>         { try { await HackyInvoke (RoleDeleted, x);                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.RoleUpdated              += async (x, y) =>      { try { await HackyInvoke (RoleUpdated, x, y);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserBanned               += async (x, y) =>      { try { await HackyInvoke (UserBanned, x, y);               } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserIsTyping             += async (x, y) =>      { try { await HackyInvoke (UserIsTyping, x, y);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserJoined               += async (x) =>         { try { await HackyInvoke (UserJoined, x);                  } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserLeft                 += async (x) =>         { try { await HackyInvoke (UserLeft, x);                    } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserUnbanned             += async (x, y) =>      { try { await HackyInvoke (UserUnbanned, x, y);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserUpdated              += async (x, y) =>      { try { await HackyInvoke (UserUpdated,x, y);               } catch (Exception exc) { OnExceptionCaught (exc); } };
            client.UserVoiceStateUpdated    += async (x, y, z) =>   { try { await HackyInvoke (UserVoiceStateUpdated, x, y, z); } catch (Exception exc) { OnExceptionCaught (exc); } };
         }

        // May the gods of code have mercy on my soul.
        private async Task HackyInvoke (dynamic function, params object[] parameters) {
            if (function == null)
                return;
            Type type = function.GetType ();
            await type.GetMethod ("Invoke").Invoke (function, parameters);
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
        public SocketGuild              GetGuild(ulong id)                                  => Client.GetGuild (id);
        public SocketGuildUser          GetUser(ulong guildId, ulong userId)                => GetGuild (guildId)?.GetUser (userId);
        public SocketGuildChannel       GetChannel(ulong guildId, ulong channelId)          => GetGuild (guildId)?.GetChannel (channelId);
        public SocketTextChannel        GetTextChannel(ulong guildId, ulong channelId)      => GetChannel (guildId, channelId) as SocketTextChannel;
        public SocketVoiceChannel       GetVoiceChannel (ulong guildId, ulong channelId)    => GetChannel (guildId, channelId) as SocketVoiceChannel;
        public SocketCategoryChannel    GetCategoryChannel(ulong guildId, ulong channelId)  => GetChannel (guildId, channelId) as SocketCategoryChannel;
        public SocketRole               GetRole(ulong guildId, ulong roleId)                => GetGuild (guildId)?.GetRole (roleId);

    }
}
