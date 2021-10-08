using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.WikiMediaFetcher
{
    [Dependency("Moduthulhu-Command Root")]
    [Descriptor("Lomztein", "Universal Wiki Fetcher", "Simple universal wiki fetcher for any WikiMedia based source wiki. Uses the WikiMedia API.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/WikiMedia%20Fetcher/WikiMediaFetcherPlugin.cs")]
    public class WikiMediaFetcherPlugin : PluginBase
    {
        private const string QUERY_BASE = "api.php?action=query&format=json&prop=extracts%7Cinfo%7Cpageimages&list=&titles=[QUERY]&exchars=1200&exintro=1&explaintext=1&inprop=url&piprop=thumbnail%7Cname&pithumbsize=512&pilimit=1";
        private const string QUERY_REPLACE = "[QUERY]";

        private List<UniversalFetchCommand> _commands = new List<UniversalFetchCommand>();
        private CachedValue<List<Tuple<string, string, string>>> _sourceUrls;

        public override void Initialize()
        {
            _sourceUrls = GetConfigCache("SourceUrls", (x) => new List<Tuple<string, string, string>>() { new Tuple<string, string, string>("https://en.wikipedia.org/w/", "wiki", "Good ol' Wikipedia" )});

            AddConfigInfo<string, string, string>("addwiki", "Add a new wiki", ValidateAndAddSource, (success, source, name, description) => success ? "Succesfully added wiki at " + source : "Failed to add wiki at " + source, "Source URL", "Command Name", "Command Description");
            AddConfigInfo<string>("removewiki", "Remove an existing wiki", RemoveSouce, (success, source) => success ? "Succesfully removed " + source : "Failed to remove " + source + ", please ensure that your request matches the URL perfectly.", "Source URL");
            AddConfigInfo("listwikies", "List current wikies", ListSources);

            foreach (var cmd in _sourceUrls.GetValue())
            {
                AddCommand(CreateCommand(cmd.Item1, cmd.Item2, cmd.Item3));
            }
        }

        private bool ValidateAndAddSource (string source, string cmdName, string cmdDescription)
        {
            if (ValidateSource(source).GetAwaiter().GetResult())
            {
                _sourceUrls.MutateValue(x => x.Add(new Tuple<string, string, string>(source, cmdName, cmdDescription)));
                AddCommand(CreateCommand(source, cmdName, cmdDescription));
                return true;
            }
            return false;
        }

        private bool RemoveSouce(string source)
        {
            bool removed = false;
            _sourceUrls.MutateValue(x => removed = x.RemoveAll(y => y.Item1 == source) > 0);
            if (removed)
            {
                RemoveCommand(GetCommandBySource(source));
            }
            return removed;
        }

        private string ListSources() => string.Join("\n", _sourceUrls.GetValue());

        public override void Shutdown()
        {
            RemoveAllCommands();
        }

        private void AddCommand (UniversalFetchCommand command)
        {
            _commands.Add(command);
            SendMessage("Moduthulhu-Command Root", "AddCommand", command);
        }

        private void RemoveCommand(UniversalFetchCommand command)
        {
            _commands.Remove(command);
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", command);
        }

        private UniversalFetchCommand GetCommandBySource(string source) => _commands.FirstOrDefault(x => x.SourceUrl == source);

        private void RemoveAllCommands ()
        {
            var copy = _commands.ToArray();
            foreach (var cmd in copy)
            {
                RemoveCommand(cmd);
            }
        }

        private async Task<JObject> QueryAsync (string wiki, string query)
        {
            string url = wiki + QUERY_BASE.Replace(QUERY_REPLACE, query);
            HttpWebRequest request = WebRequest.CreateHttp(url);
            WebResponse response = await request.GetResponseAsync();
            JObject result = null;

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string streamContents = reader.ReadToEnd();
                    result = JObject.Parse(streamContents);
                }
            }

            return result;
        }

        private Embed BuildPageEmbed (JObject queryResult)
        {
            EmbedBuilder builder = new EmbedBuilder();

            var query = queryResult["query"];
            var page = query["pages"].First.First as JObject; // idk

            if (page["title"] != null)
            {
                builder.WithTitle(page["title"].ToString());
            }
            if (page["extract"] != null)
            {
                builder.WithDescription(page["extract"].ToString());
            }
            if (page["fullurl"] != null)
            {
                builder.WithUrl(page["fullurl"].ToString());
            }
            if (page["thumbnail"]?["source"] != null)
            {
                builder.WithImageUrl(page["thumbnail"]["source"].ToString());
            }

            return builder.Build();
        }

        private UniversalFetchCommand CreateCommand (string sourceUrl, string name, string description)
        {
            return new UniversalFetchCommand(sourceUrl, name, description) { ParentPlugin = this };
        }

        private async Task<bool> ValidateSource (string sourceUrl)
        {
            HttpWebRequest request = WebRequest.CreateHttp(sourceUrl);
            WebResponse response = await request.GetResponseAsync();
            return true; // TODO: Implement lol
        }

        public class UniversalFetchCommand : PluginCommand<WikiMediaFetcherPlugin>
        {
            public string SourceUrl { get; private set; } 

            public UniversalFetchCommand (string sourceUrl, string name, string description)
            {
                Name = name;
                Description = description;
                SourceUrl = sourceUrl;
                Category = new Category("Wiki", "Commands for querying a varity of wikies. Support for adding custom wikies planned.");
            }

            [Overload(typeof(Embed), "Query this wiki for a particular page.")]
            public async Task<Result> Execute (CommandMetadata metadata, string query)
            {
                JObject result = await ParentPlugin.QueryAsync(SourceUrl, query);
                Embed embed = ParentPlugin.BuildPageEmbed(result);
                if (embed == null)
                {
                    return new Result(null, "Sorry fam, the wiki fetch failed.");
                }
                return new Result(embed, string.Empty);
            }
        }
    }
}
