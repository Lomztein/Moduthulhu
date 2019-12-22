using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    public class LargeEmbed : ISendable<IMessage[]>, IDeletable {

        public const int FieldsPerEmbed = 25;
        private EmbedBuilder[] _builders;

        public IMessage[] Result { get; private set; }

        public LargeEmbed(EmbedBuilder source, IEnumerable<EmbedFieldBuilder> fieldBuilders) {

            List<EmbedFieldBuilder> fieldBuildersList = fieldBuilders.ToList();

            EmbedBuilder header = source;
            EmbedBuilder footer = source;

            List<EmbedBuilder> fields = new List<EmbedBuilder> { source };

            List<List<EmbedFieldBuilder>> embedFields = new List<List<EmbedFieldBuilder>> ();
            int index = 0;
            while (fieldBuildersList.Count != 0)
            {
                int amount = Math.Min(FieldsPerEmbed, fieldBuildersList.Count);
                if (index > amount)
                { // This goes a bit against the typical for-loop conventions, but it should work rather simply. Please don't scream at me. // nvm i changed it after a website screamed at me.
                    embedFields.Add(new List<EmbedFieldBuilder>(fieldBuildersList.GetRange(0, amount)));
                    fieldBuildersList.RemoveRange(0, amount);
                    index = -1;
                }
                index++;
            }

            for (int i = 0; i < embedFields.Count; i++) {

                EmbedBuilder field = source;

                if (i > 0)
                {
                    field = new EmbedBuilder();
                    fields.Add (field);
                }

                for (int j = 0; j < embedFields[i].Count; j++) {
                    field.AddField (embedFields[i][j]);
                }

                if (i == 0)
                {
                    header = field;
                }

                if (i == embedFields.Count - 1)
                {
                    footer = field;
                }

            }

            header.Author = source.Author;
            header.Color = source.Color;
            header.Description = source.Description;
            header.ImageUrl = source.ImageUrl;
            header.ThumbnailUrl = source.ThumbnailUrl;
            header.Title = source.Title;
            header.Url = source.Url;

            footer.Footer = source.Footer;
            footer.Timestamp = source.Timestamp;

            _builders = fields.ToArray ();
        }

        public async Task DeleteAsync(RequestOptions options) {
            foreach (var message in Result) {
                await message.DeleteAsync (options);
            }
        }

        public async Task DeleteAsync() => await DeleteAsync(null);

        public async Task SendAsync(IMessageChannel channel) {
            List<IMessage> messages = new List<IMessage> ();

            foreach (EmbedBuilder builder in _builders) {
                var message = await channel.SendMessageAsync ("", false, builder.Build ());
                messages.Add (message);
            }

            Result = messages.ToArray ();
        }
    }
}
