using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    public class LargeEmbed : ISendable<IMessage[]>, IDeletable {

        public const int FieldsPerEmbed = 25;
        private EmbedBuilder[] _builders;

        public IMessage[] Result { get; private set; }

        public LargeEmbed(EmbedBuilder source) {

            EmbedBuilder header = null;
            EmbedBuilder footer = null;

            List<EmbedBuilder> fields = new List<EmbedBuilder> ();

            List<List<EmbedFieldBuilder>> embedFields = new List<List<EmbedFieldBuilder>> ();
            int index = 0;
            while (source.Fields.Count != 0)
            {
                int amount = Math.Min(FieldsPerEmbed, embedFields.Count);
                if (index > amount)
                { // This goes a bit against the typical for-loop conventions, but it should work rather simply. Please don't scream at me. // nvm i changed it after a website screamed at me.
                    embedFields.Add(new List<EmbedFieldBuilder>(source.Fields.GetRange(0, amount)));
                    source.Fields.RemoveRange(0, amount);
                    index = -1;
                }
                index++;
            }

            embedFields.Add (new List<EmbedFieldBuilder> (source.Fields));

            if (embedFields.Count == 0)
            {
                header = new EmbedBuilder ();
                footer = new EmbedBuilder ();
            }

            for (int i = 0; i < embedFields.Count; i++) {
                EmbedBuilder field = new EmbedBuilder ();

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

                fields.Add (field);
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

            List<EmbedBuilder> result = new List<EmbedBuilder> ();
            result.AddRange (fields);

            _builders = result.ToArray ();
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
