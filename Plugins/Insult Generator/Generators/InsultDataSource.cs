using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.InsultGenerators.Lomz
{
    public class InsultDataSource
    {
        private string[] _formats;
        public string[] Formats => _formats.Clone() as string[];

        private Dictionary<string, string[]> _categories;
        private Dictionary<string, Func<string, string>> _variables;

        public InsultDataSource (string[] formats, Dictionary<string, string[]> categories, Dictionary<string, Func<string, string>> variables)
        {
            _formats = formats;
            _categories = categories;
            _variables = variables;
        }

        public string[] GetCategory(string category) => _categories[category];
        public string GetVariable(string variable, string target) => _variables[variable](target);

        public string Get (string value, string target)
        {
            if (_variables.ContainsKey (value))
            {
                return GetVariable(value, target);
            }

            if (_categories.ContainsKey (value))
            {
                Random random = new Random();
                string[] cat = GetCategory(value);
                return cat[random.Next(cat.Length)];
            }

            return value;
        }
    }
}
