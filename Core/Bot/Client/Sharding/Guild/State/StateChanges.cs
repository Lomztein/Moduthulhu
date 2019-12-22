using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement
{
    public class StateChanges
    {
        public string AddedHeader { get; }
        public string RemovedHeader { get; }
        public string MutatedHeader { get; }

        private string[] _additions;
        private string[] _removals;
        private string[] _mutations;

        public StateChanges (string addedHeader, string[] additions, string removedHeader, string[] removals, string mutatedHeader, string[] mutations)
        {
            AddedHeader = addedHeader;
            RemovedHeader = removedHeader;
            MutatedHeader = mutatedHeader;

            _additions = additions;
            _removals = removals;
            _mutations = mutations;
        }

        public string[] GetAdditions() => _additions;
        public string[] GetRemovals() => _removals;
        public string[] GetMutations() => _mutations;
    }
}
