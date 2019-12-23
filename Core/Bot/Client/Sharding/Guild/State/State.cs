using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement
{
    public class State
    {
        public readonly string Identifier;

        private List<StateAttribute> _attributes = new List<StateAttribute>();
        public StateAttribute[] GetAttributes() => _attributes.ToArray ();

        public State (string identifier)
        {
            Identifier = identifier;
        }

        public void Add(string name, string desc) => _attributes.Add(new StateAttribute (name, desc));
    }
}
