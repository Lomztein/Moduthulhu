using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.InsultGenerators
{
    public class AdjectiveVerbNounInsultGenerator : IInsultGenerator
    {
        private string[] _adjectives = new string[] {
            "such a massive", "an extremely dumb", "a super ugly", "a so handsome", "such a fat"
        };

        private string[] _verbs = new string[]
        {
            "fucking", "barfing", "emancipating", "arousing", "crying", "masturbating", "reeing", "jerking"
        };

        private string[] _nouns = new string[]
        {
            "cat", "dog", "penis", "Trump supporter", "hunk", "gastropod", "traitor", "TikTok fan", "jerkoff", "9Gag user", "Agesome1"
        };

        public string Insult(string target)
        {
            Random random = new Random();
            return $"{target} is {_adjectives[random.Next(_adjectives.Length)]} {_verbs[random.Next(_verbs.Length)]} {_nouns[random.Next (_nouns.Length)]}!";
        }
    }
}
