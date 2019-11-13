using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.Config
{
    public class ConfigFunctionInfo
    {
        private ConfigFunctionParam[] _parameters;
        public ConfigFunctionParam[] GetParameters () => _parameters;
        public Delegate Action { get; private set; }
        public Func<string> Message { get; private set; }

        public string Name { get; private set; }
        public string Desc { get; private set; }
        public string Identifier { get; private set; }

        public ConfigFunctionInfo(string name, string description, string identifier, Delegate action, Func<string> message, params string[] paramNames)
        {
            Action = action;

            if (Action.Method.ReturnType != typeof(void)) // I couldn't find any non-generic Action object to use, best I got is a Delegate type.
                throw new ArgumentException("Delegate must be without a return type, use of System.Action type required.");

            Type[] generics = Action.GetType().GetGenericArguments();
            if (generics.Length != paramNames.Length)
                throw new ArgumentException("A differing amount of parameter names was given in comparison to the actions generic arguments. Lengths need to be identical.");

            _parameters = new ConfigFunctionParam[generics.Length];
            for (int i = 0; i < generics.Length; i++)
            {
                _parameters[i] = new ConfigFunctionParam(generics[i], paramNames[i]);
            }

            Name = name;
            Desc = description;
            Identifier = identifier;
            Message = message;
        }

        public ConfigFunctionInfo(string name, string description, string identifier, Delegate @delegate, Func<string> message, params ConfigFunctionParam[] parameters)
        {
            Action = @delegate;
            Name = name;
            Desc = description;
            Identifier = identifier;
            Message = message;
            _parameters = parameters;
        }

        public bool Matches(string identifier) => Identifier == identifier;
        public bool Matches(string name, string identifier) => Name == name && Identifier == identifier;
    }
}
