using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced {

    public class LargeEmbed : ICustomMessage<EmbedBuilder, EmbedBuilder[], IMessage[]> {

        public const int fieldsPerEmbed = 25;

        public IMessage[] Message { get; set; }
        public EmbedBuilder[] Intermediate { get; set; }

        public void CreateFrom(EmbedBuilder source) {

            EmbedBuilder header = new EmbedBuilder ();
            EmbedBuilder footer = new EmbedBuilder ();

            List<EmbedBuilder> fields = new List<EmbedBuilder> ();

            header.Author = source.Author;
            header.Color = source.Color;
            header.Description = source.Description;
            header.ImageUrl = source.ImageUrl;
            header.ThumbnailUrl = source.ThumbnailUrl;
            header.Title = source.Title;
            header.Url = source.Url;

            footer.Footer = source.Footer;
            footer.Timestamp = source.Timestamp;

            List<List<EmbedFieldBuilder>> embedFields = new List<List<EmbedFieldBuilder>> ();
            for (int i = 0; i < source.Fields.Count; i++) {

                if (i > fieldsPerEmbed) { // This goes a bit against the typical for-loop conventions, but it should work rather simply. Please don't scream at me.
                    embedFields.Add (new List<EmbedFieldBuilder> (source.Fields.GetRange (0, fieldsPerEmbed)));
                    source.Fields.RemoveRange (0, fieldsPerEmbed);
                    i = 0;
                }
            }

            embedFields.Add (new List<EmbedFieldBuilder> (source.Fields));

            for (int i = 0; i < embedFields.Count; i++) {
                EmbedBuilder field = new EmbedBuilder ();

                for (int j = 0; j < embedFields[j].Count; j++) {
                    field.AddField (embedFields[i][j]);
                }

                fields.Add (field);
            }

            List<EmbedBuilder> result = new List<EmbedBuilder> ();
            result.Add (header);
            result.AddRange (fields);
            result.Add (footer);

            Intermediate = result.ToArray ();
        }

        public async Task DeleteAsync(RequestOptions options = null) {
            foreach (var message in Message) {
                await message.DeleteAsync (options);
            }
        }

        public async Task SendAsync(IMessageChannel channel) {
            List<IMessage> messages = new List<IMessage> ();

            foreach (EmbedBuilder builder in Intermediate) {
                var message = await channel.SendMessageAsync ("", false, builder.Build ());
                messages.Add (message);
            }

            Message = messages.ToArray ();
        }
    }
}
