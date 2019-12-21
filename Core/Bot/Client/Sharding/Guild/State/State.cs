using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement
{
    public class State
    {
        public string AddedHeader { get; }
        public string RemovedHeader { get; }
        public string MutatedHeader { get; }

        private List<StateAttribute> _attributes = new List<StateAttribute>();
        public StateAttribute[] GetAttributes() => _attributes.ToArray ();

        public State (string addedHeader, string removedHeader, string mutatedHeader)
        {
            AddedHeader = addedHeader;
            RemovedHeader = removedHeader;
            MutatedHeader = mutatedHeader;
        }

        public void Add(string name, string desc) => _attributes.Add(new StateAttribute (name, desc));
    }
}
