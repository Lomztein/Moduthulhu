using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class StringExtensions
    {
        private const int MaxCharactersPerMessage = 2000;
        public static string[] SplitMessage(this string message, string sorrounder) {
            List<string> splitted = new List<string> ();

            int counted = 0;
            while (message.Length > 0) {

                // Give some wiggle room, to avoid any shenanagens.
                int margin = 10 + sorrounder.Length * 2;
                if (counted > MaxCharactersPerMessage - margin) {

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
                        spaceSearch = counted;

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
    }
}
