using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class StringExtensions
    {
        public static string[] SplitMessage(this string message, string sorrounder) => SplitMessage(message, sorrounder, 2000);

        public static string[] SplitMessage(this string message, string sorrounder, int maxChars) {
            List<string> splitted = new List<string> ();

            int counted = 0;
            while (message.Length > 0) {

                // Give some wiggle room, to avoid any shenanagens.
                int margin = 10 + sorrounder.Length * 2;
                if (counted > maxChars - margin) {

                    int spaceSearch = counted; // First, try newlines.
                    while (message[spaceSearch] != '\n' && spaceSearch > 0) {
                        spaceSearch--;
                    }

                    if (spaceSearch == 0) { // No newlines were found, try spaces instead.
                        spaceSearch = counted;
                        while (message[spaceSearch] != ' ' && spaceSearch > 0) {
                            spaceSearch--;
                        }
                    }

                    if (spaceSearch == 0) // No spaces found? Jeez, just cut of as late as possible then.
                    { 
                        spaceSearch = counted;
                    }

                    string substring = message.Substring (0, spaceSearch);
                    splitted.Add (sorrounder + substring + sorrounder);
                    message = message.Substring (spaceSearch);

                    counted = 0;
                } else if (counted >= message.Length) {

                    splitted.Add (sorrounder + message + sorrounder);
                    message = "";
                }

                counted++;
            }

            return splitted.ToArray ();
        }
        public static (ulong channelId, ulong messageId) ParseMessageUrl (this string messageUrl)
        {
            string[] split = messageUrl.Split('/');
            if (split.Length != 7)
            {
                throw GetInvalidUrlMessage();
            }

            string channel = split[5];
            string message = split[6];

            try
            {
                ulong channelId = ulong.Parse(channel, CultureInfo.InvariantCulture);
                ulong messageId = ulong.Parse(message, CultureInfo.InvariantCulture);
                return (channelId, messageId);
            }
            catch (FormatException)
            {
                throw GetInvalidUrlMessage();
            }

            ArgumentException GetInvalidUrlMessage() => new ArgumentException("Message URL is not valid. You can get message URLs by clicking the three vertical dots to the right side of a message, and selecting 'Copy Link'.");
        }
    }
}
