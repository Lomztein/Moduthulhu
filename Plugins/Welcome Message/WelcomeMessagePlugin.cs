using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.WelcomeMessage
{
    [Dependency("Moduthulhu-Command Root")]
    [Descriptor("Lomztein", "Welcome Message", "Sends new joinees a heavily customizable welcome message.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Welcome%20Message/WelcomeMessagePlugin.cs")]
    public class WelcomeMessagePlugin : PluginBase
    {
        private CachedValue<string> _title;
        private CachedValue<string> _description;
        private CachedValue<string> _url;
        private CachedValue<string> _colourHex;
        private CachedValue<string> _iconUrl;

        private CachedValue<EmbedAuthor> _author;
        private CachedValue<List<FieldData>> _fields;

        private CachedValue<string> _footerText;

        private Dictionary<string, Func<SocketGuildUser, string>> _referenceFunctions;
        private ICommand _command;

        public override void Initialize()
        {
            GuildHandler.UserJoined += GuildHandler_UserJoined;
            PopulateReferenceFunctions();

            _title = GetConfigCache("Title", x => "Welcome to [ServerName]!");
            _description = GetConfigCache("Description", x => "Hello [Joinee]! You have been invited and thus completely voluntarily joined [ServerName], and we are absolutely thrilled to have you here! There is absolutely no reason to believe any memetic hazards to have been at play!");
            _url = GetConfigCache<string>("Url", x => null);
            _colourHex = GetConfigCache("Colour", x => "08F26E");
            _iconUrl = GetConfigCache("IconUrl", x => "[ServerIconUrl]");
            _author = GetConfigCache("Author", x => new EmbedAuthor (GuildHandler.BotUser.GetShownName (), null, GuildHandler.BotUser.GetAvatarUrl ()));
            _fields = GetConfigCache("Fields", x => new List<FieldData>());
            _footerText = GetConfigCache("Footer", x => "[DateNow]");

            _command = new DemoWelcomeMessageCommand { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _command);

            AddConfigInfo<string>("Set Welcome Title", "Set title", x => _title.SetValue(x), x => $"Welcome message title has been set to '{x}'.", "Title");
            AddConfigInfo<string>("Set Welcome Description", "Set description", x => _description.SetValue(x), x => $"Welcome message description has been set to '{x}'.", "Description");
            AddConfigInfo<string>("Set Welcome Url", "Set url", x => _url.SetValue(x), x => $"Welcome message url has been set to '{x}'.", "Url");
            AddConfigInfo<string>("Set Welcome Colour", "Set color (hex)", x => _colourHex.SetValue(x), x => $"Welcome message colour has been set to '#{x}'.", "Colour (hex)");
            AddConfigInfo<string>("Set Welcome Icon Url", "Set icon url", x => _iconUrl.SetValue(x), x => $"Welcome message icon url has been set to '{x}'.", "Icon Url");
            AddConfigInfo<string>("Set Welcome Footer", "Set footer", x => _footerText.SetValue(x), x => $"Welcome message footer has been set to '{x}'.", "Footer");

            AddConfigInfo<string, string>("Add Welcome Field", "Add field", (x, y) => _fields.MutateValue(z => z.Add(new FieldData(x, y, false))), (x, y) => $"Added field with title '{x}' and value '{y}'", "Title", "Value");
            AddConfigInfo<string, string, bool>("Add Welcome Field", "Add field", (x, y, z) => _fields.MutateValue(w => w.Add(new FieldData(x, y, z))), (x, y, z) => $"Added field with title '{x}' and value '{y}' that is inline: {z}", "Title", "Value", "Inline");
            AddConfigInfo("Add Welcome Field", "Add field", () => $"Current fields in the welcome message embed:{string.Join ("\n", _fields.GetValue ().Select (x => $"**{x.Name}**\n{x.Value}"))}");

            AddConfigInfo<string>("Remove Welcome Field", "Remove field", x => _fields.MutateValue(y => y.RemoveAll(z => z.Name == x)), x => $"Removed any fields with the title '{x}'.", "Title");

            AddConfigInfo<IUser>("Set Welcome Author", "Set Author", x => _author.SetValue(new EmbedAuthor(x.GetShownName (), null, x.GetAvatarUrl ())), x => $"Set welcome message author to {x.GetShownName ()}.", "User");
            AddConfigInfo<string>("Set Welcome Author", "Set Author", x => _author.SetValue(new EmbedAuthor(x, null, null)), x => $"Set welcome message author to '{x}'.", "Author Name");
            AddConfigInfo<string, string>("Set Welcome Author", "Set Author", (x, y) => _author.SetValue(new EmbedAuthor(x, null, y)), (x, y) => $"Set welcome message author to '{x}' with icon url '{y}'.", "Author Name", "Author Icon Url");
            AddConfigInfo<string, string, string>("Set Welcome Author", "Set Author", (x, y, z) => _author.SetValue(new EmbedAuthor(x, z, y)), (x, y, z) => $"Set welcome message author to '{x}' with icon url '{y}' and url to {z}.", "Author Name", "Author Icon Url", "Author Url");
        }

        public override void Shutdown()
        {
            GuildHandler.UserJoined -= GuildHandler_UserJoined;
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _command);
        }

        private async Task GuildHandler_UserJoined(SocketGuildUser arg)
        {
            Embed embed = GenerateWelcomeMessage(arg);
            await arg.SendMessageAsync(null, false, embed);
        }

        private void PopulateReferenceFunctions ()
        {
            _referenceFunctions = new Dictionary<string, Func<SocketGuildUser, string>>
            {
                { "ServerName", (x) => GuildHandler.GetGuild ().Name },
                { "ServerIconUrl", (x) => GuildHandler.GetGuild ().IconUrl },
                { "ServerOwner", (x) => $"[{GuildHandler.GetGuild ().OwnerId}]" },
                { "DateNow", (x) => DateTime.Now.ToString ("dd/MM/yyyy H:mm") },
                { "Joinee", (x) => x.GetShownName () },
            };
        }

        public Embed GenerateWelcomeMessage(SocketGuildUser user)
        {
            EmbedBuilder builder = new EmbedBuilder();
            IfNotNull(_title, x => builder.WithTitle(ParseReferences (x, user)));
            IfNotNull(_description, x => builder.WithDescription(ParseReferences(x, user)));
            IfNotNull(_url, x => builder.WithUrl(ParseReferences(x, user)));
            IfNotNull(_colourHex, x => builder.WithColor(new Color (Convert.ToUInt32 (ParseReferences(x, user), 16))));
            IfNotNull(_iconUrl, x => builder.WithThumbnailUrl(ParseReferences(x, user)));

            if (_author.GetValue () != null)
            {
                builder.WithAuthor(ParseReferences(_author.GetValue().Name, user), ParseReferences(_author.GetValue().IconUrl, user), ParseReferences(_author.GetValue().AuthorUrl, user));
            }

            if (_fields.GetValue ().Count != 0)
            {
                List<FieldData> data = _fields.GetValue();
                foreach (FieldData field in data)
                {
                    builder.AddField(ParseReferences(field.Name, user), ParseReferences(field.Value, user), field.Inline);
                }
            }

            IfNotNull(_footerText, x => builder.WithFooter(ParseReferences(x, user)));
            return builder.Build();
        }

        private void IfNotNull (CachedValue<string> value, Action<string> ifNot)
        {
            if (!string.IsNullOrEmpty (value.GetValue ()))
            {
                ifNot(value.GetValue());
            }
        }

        private string ParseReferences (string text, SocketGuildUser user)
        {
            if (text == null)
            {
                return null;
            }

            Regex referenceRegex = new Regex(@"\[(.*?)\]");
            string newText = referenceRegex.Replace (text, (x) => GetReference (x, user));

            return newText == text ? newText : ParseReferences(newText, user);
        }

        private string GetReference(Match input, SocketGuildUser user)
        {
            string reference = input.Value.Substring(1, input.Length - 2);
            if (_referenceFunctions.ContainsKey(reference)) 
            {
                return _referenceFunctions[reference](user);
            }
            else if (ulong.TryParse (reference, out ulong id))
            {
                return GetSnowflakeName(id);
            }
            return input.Value;
        }

        private string GetSnowflakeName (ulong id)
        {
            IUser user = GuildHandler.FindUser(id);
            if (user != null)
            {
                return $"@{user.GetShownName ()}";
            }

            IChannel channel = GuildHandler.FindChannel(id);
            if (channel != null)
            {
                return $"#{channel.Name}";
            }

            IRole role = GuildHandler.FindRole(id);
            if (role != null)
            {
                return $"@{role.Name}";
            }
            return id.ToString();
        }

        private class FieldData
        {
            [JsonProperty ("Name")]
            public string Name { get; private set; }
            [JsonProperty ("Value")]
            public string Value { get; private set; }
            [JsonProperty ("Inline")]
            public bool Inline { get; private set; }

            public FieldData (string name, string value, bool inline)
            {
                Name = name;
                Value = value;
                Inline = inline;
            }
        }

        private class EmbedAuthor
        {
            [JsonProperty ("Name")]
            public string Name { get; private set; }
            [JsonProperty ("AuthorUrl")]
            public string AuthorUrl { get; private set; }
            [JsonProperty ("IconUrl")]
            public string IconUrl { get; private set; }

            public EmbedAuthor(string name, string authorUrl, string iconUrl)
            {
                Name = name;
                AuthorUrl = authorUrl;
                IconUrl = iconUrl;
            }
        }
    }

    public class DemoWelcomeMessageCommand : PluginCommand<WelcomeMessagePlugin>
    {
        public DemoWelcomeMessageCommand ()
        {
            Name = "demowelcomemessage";
            Description = "Show welcome message";
            Category = AdditionalCategories.Management;
            RequiredPermissions.Add(GuildPermission.ManageGuild);
        }

        [Overload (typeof (Embed), "Display what you'd see if you joined this server.")]
        public Task<Result> Execute (CommandMetadata data)
            => TaskResult(ParentPlugin.GenerateWelcomeMessage(data.Author as SocketGuildUser), string.Empty);
    }
}
