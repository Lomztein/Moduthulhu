using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.WikiMediaFetcher
{
    [Descriptor("Lomztein", "Universal Wiki Fetcher", "Eternally WIP simple fetcher for any WikiMedia based source wiki. Except for the part where other wikis don't work for whatever reason.")]
    public class WikiMediaFetcherPlugin : PluginBase
    {
        private const string QUERY_BASE = "api.php?action=query&format=json&prop=extracts%7Cinfo%7Cpageimages&list=&titles=[QUERY]&exchars=1200&exintro=1&explaintext=1&inprop=url&piprop=thumbnail%7Cname&pithumbsize=512&pilimit=1";
        private const string QUERY_REPLACE = "[QUERY]";

        private List<UniversalFetchCommand> _commands = new List<UniversalFetchCommand>();

        public override void Initialize()
        {
            AddCommand (CreateCommand("https://en.wikipedia.org/w/", "wiki", "Good ol' Wikipedia"));
        }

        public override void Shutdown()
        {
            RemoveAllCommands();
        }

        private void AddCommand (UniversalFetchCommand command)
        {
            _commands.Add(command);
            SendMessage("Moduthulhu-Command Root", "AddCommand", command);
        }

        private void RemoveAllCommands ()
        {
            SendMessage("Moduthulhu-Command Root", "RemoveCommands", _commands.ToArray());
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

            try
            {
                builder.WithTitle(page["title"].ToString())
                .WithDescription(page["extract"].ToString())
                .WithUrl(page["fullurl"].ToString())
                    .WithImageUrl(page["thumbnail"]["source"].ToString());

                return builder.Build();
            }
            catch (Exception) { }
            return null;
        }

        private UniversalFetchCommand CreateCommand (string sourceUrl, string name, string description)
        {
            return new UniversalFetchCommand(sourceUrl, name, description) { ParentPlugin = this };
        }

        private async Task AssertSourceValidity (string sourceUrl)
        {
            HttpWebRequest request = WebRequest.CreateHttp(sourceUrl);
            WebResponse response = await request.GetResponseAsync();
            throw new ArgumentException("Source URL is invalid. Try and go to a page of your desired wiki, and copy the URL that prefixes the page. Example: https://en.wikipedia.org/wiki/Death -> https://en.wikipedia.org/wiki/");
        }

        public class UniversalFetchCommand : PluginCommand<WikiMediaFetcherPlugin>
        {
            private string _sourceUrl;

            public UniversalFetchCommand (string sourceUrl, string name, string description)
            {
                Name = name;
                Description = description;
                _sourceUrl = sourceUrl;
                Category = StandardCategories.Utility;
            }

            [Overload(typeof(Embed), "Query this wiki for a particular page.")]
            public async Task<Result> Execute (CommandMetadata metadata, string query)
            {
                JObject result = await ParentPlugin.QueryAsync(_sourceUrl, query);
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
