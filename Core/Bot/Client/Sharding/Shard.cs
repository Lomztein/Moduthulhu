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
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class Shard {

        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        public BotClient BotClient { get; private set; }
        internal ClientManager ClientManager { get => BotClient.ClientManager; }
        public Core Core { get => BotClient.Core; }

        internal ModuleLoader ModuleLoader { get => BotClient.Core.ModuleLoader; }
        public ModuleContainer ModuleContainer { get; private set; }

        public DiscordSocketClient Client { get; private set; }
        public IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }
        public bool IsConnected { get => Guilds.Count > 0 && Guilds.First ().IsConnected; }

        private Thread Thread { get; set; }

        public int ShardId { get; private set; }

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

        internal Shard(BotClient parentManager, int shardId) {
            BotClient = parentManager;
            ShardId = shardId;
        }

        internal void Begin () {
            ThreadStart init = new ThreadStart (Initialize);
            Thread = new Thread (init) {
                Name = ToString (),
            };
            Thread.Start ();
        }

        internal async void Initialize () {

            BootDate = DateTime.Now;
            DiscordSocketConfig config = new DiscordSocketConfig () {
                ShardId = ShardId,
                TotalShards = BotClient.TotalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            };
            Client = new DiscordSocketClient (config);
            InitializeErrorHandling ();
            Client.Ready += Client_Ready;

            await Start ();
            await Login ();
            await AwaitConnected ();

            ModuleContainer = new ModuleContainer (this);
            ModuleContainer.InstantiateModules ();
            ModuleContainer.InitializeModules ();
        }

        private Task Client_Ready() {
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} is ready and connected.");
            return Task.CompletedTask;
        }

        private async Task Start () {
            await Client.StartAsync ();
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} started.");
        }

        private async Task Stop() {
            await Client.StopAsync ();
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} stopped.");
        }

        private async Task Login () {
            await Client.LoginAsync (TokenType.Bot, BotClient.Token);
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} logged in.");
        }

        private async Task Logout () {
            await Client.LogoutAsync ();
            Cross.Log.Write (Cross.Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} logged out in.");
        }

        internal async Task Kill () {
            Cross.Log.Write (Cross.Log.Type.CRITICAL, $"KILLING CLIENT {BotClient.Name} SHARD {ShardId}!");
            await Logout ();
            await Stop ();
            Client.Dispose ();
        }

        public override string ToString() => $"{BotClient} - S{ShardId}/{BotClient.TotalShards}";

        private void InitializeErrorHandling() {  // It'd almost be worth writing a script to type this shiznat out automatically.
            Client.ChannelCreated           += async (x) =>         { try { await HackyInvoke (ChannelCreated, x);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelDestroyed         += async (x) =>         { try { await HackyInvoke (ChannelDestroyed, x);            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ChannelUpdated           += async (x, y) =>      { try { await HackyInvoke (ChannelUpdated, x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Connected                += async () =>          { try { await HackyInvoke (Connected);                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.CurrentUserUpdated       += async (x, y) =>      { try { await HackyInvoke (CurrentUserUpdated, x, y);       } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Disconnected             += async (x) =>         { try { await HackyInvoke (Disconnected, x);                } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildAvailable           += async (x) =>         { try { await HackyInvoke (GuildAvailable, x);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildMembersDownloaded   += async (x) =>         { try { await HackyInvoke (GuildMembersDownloaded, x);      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildMemberUpdated       += async (x, y) =>      { try { await HackyInvoke (GuildMemberUpdated, x, y);       } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildUnavailable         += async (x) =>         { try { await HackyInvoke (GuildUnavailable, x);            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.GuildUpdated             += async (x, y) =>      { try { await HackyInvoke (GuildUpdated, x, y);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.JoinedGuild              += async (x) =>         { try { await HackyInvoke (JoinedGuild, x);                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LatencyUpdated           += async (x, y) =>      { try { await HackyInvoke (LatencyUpdated, x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LeftGuild                += async (x) =>         { try { await HackyInvoke (LeftGuild, x);                   } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Log                      += async (x) =>         { try { await HackyInvoke (Log, x);                         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LoggedIn                 += async () =>          { try { await HackyInvoke (LoggedIn);                       } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.LoggedOut                += async () =>          { try { await HackyInvoke (LoggedOut);                      } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageDeleted           += async (x, y) =>      { try { await HackyInvoke (MessageDeleted, x, y);           } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageReceived          += async (x) =>         { try { await HackyInvoke (MessageReceived, x);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.MessageUpdated           += async (x, y, z) =>   { try { await HackyInvoke (MessageUpdated, x, y, z);        } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionAdded            += async (x, y, z) =>   { try { await HackyInvoke (ReactionAdded, x, y, z);         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionRemoved          += async (x, y, z) =>   { try { await HackyInvoke (ReactionRemoved, x, y, z);       } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.ReactionsCleared         += async (x, y) =>      { try { await HackyInvoke (ReactionsCleared, x, y);         } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.Ready                    += async () =>          { try { await HackyInvoke (Ready);                          } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RecipientAdded           += async (x) =>         { try { await HackyInvoke (RecipientAdded, x);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RecipientRemoved         += async (x) =>         { try { await HackyInvoke (RecipientRemoved, x);            } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleCreated              += async (x) =>         { try { await HackyInvoke (RoleCreated, x);                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleDeleted              += async (x) =>         { try { await HackyInvoke (RoleDeleted, x);                 } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.RoleUpdated              += async (x, y) =>      { try { await HackyInvoke (RoleUpdated, x, y);              } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserBanned               += async (x, y) =>      { try { await HackyInvoke (UserBanned, x, y);               } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserIsTyping             += async (x, y) =>      { try { await HackyInvoke (UserIsTyping, x, y);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserJoined               += async (x) =>         { try { await HackyInvoke (UserJoined, x);                  } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserLeft                 += async (x) =>         { try { await HackyInvoke (UserLeft, x);                    } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserUnbanned             += async (x, y) =>      { try { await HackyInvoke (UserUnbanned, x, y);             } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserUpdated              += async (x, y) =>      { try { await HackyInvoke (UserUpdated,x, y);               } catch (Exception exc) { OnExceptionCaught (exc); } };
            Client.UserVoiceStateUpdated    += async (x, y, z) =>   { try { await HackyInvoke (UserVoiceStateUpdated, x, y, z); } catch (Exception exc) { OnExceptionCaught (exc); } };
         }

        // May the gods of code have mercy on my soul.
        private async Task HackyInvoke (dynamic function, params object[] parameters) {
            if (function == null)
                return;
            Type type = function.GetType ();
            await type.GetMethod ("Invoke").Invoke (function, parameters);
        }

        private async void OnExceptionCaught(Exception inner) {
            await ExceptionCaught?.Invoke (inner);
        }

        // Status related stuff
        public async Task AwaitConnected () {
            while (!IsConnected) {
                await Task.Delay (100);
            }
        }

        // TODO: Change all these to extension methods since that'll better support null-cases.
        public string GetStatusString () {
            return $" -- SHARD {ShardId} -- \nGuilds: {Client.Guilds.Count}\nUsers: {Client.Guilds.Sum (x => x.MemberCount)}\nLogin state: {Client.LoginState}\nConnection: {Client.ConnectionState}\nLatency: {Client.Latency}\nUptime: {Uptime}\n";
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
