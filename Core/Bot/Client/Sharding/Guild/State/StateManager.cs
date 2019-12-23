using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement
{
    public class StateManager
    {
        private Dictionary<string, State> _currentStates = new Dictionary<string, State>();
        private Dictionary<string, State> _previousStates = new Dictionary<string, State>();

        private Dictionary<string, string> _additionsHeaders = new Dictionary<string, string>();
        private Dictionary<string, string> _removingsHeaders = new Dictionary<string, string>();
        private Dictionary<string, string> _mutationsHeaders = new Dictionary<string, string>();

        public StateManager ()
        {
            Reset();
        }

        public void Reset ()
        {
            _previousStates = new Dictionary<string, State>(_currentStates);
            _currentStates.Clear();
            ClearHeaders();
        }

        public void SetChangeHeaders (string identifier, string addition, string removing, string mutation)
        {
            if (!_additionsHeaders.ContainsKey (identifier))
            {
                _additionsHeaders.Add(identifier, addition);
                _removingsHeaders.Add(identifier, removing);
                _mutationsHeaders.Add(identifier, mutation);
            }

            AssertHeadersEqualCount();
        }

        private void ClearHeaders ()
        {
            _additionsHeaders.Clear();
            _removingsHeaders.Clear();
            _mutationsHeaders.Clear();
        }

        private void AssertHeadersEqualCount ()
        {
            if (_additionsHeaders.Count != _removingsHeaders.Count || 
                _additionsHeaders.Count != _mutationsHeaders.Count)
            {
                throw new InvalidOperationException("State headers have unequal lengths.");
            }
        }

        private void AssertHeadersForAllStates ()
        {
            string missingHeader = string.Empty;
            if (!_currentStates.All (x => {
                if (_additionsHeaders.ContainsKey(x.Key))
                {
                    return true;
                }
                else
                {
                    missingHeader = x.Key;
                    return false;
                }
                }))
            {
                throw new InvalidOperationException($"State {missingHeader} is missing a header. Remember to call the SetHeader method to assign headers.");
            }
        }

        public void AddAttribute (string identifier, string name, string desc)
        {
            if (!_currentStates.ContainsKey (identifier))
            {
                _currentStates.Add(identifier, new State(identifier));
            }
            _currentStates[identifier].Add(name, desc);
        }

        public StateChanges Compare (State prev, State curr)
        {
            AssertHeadersForAllStates();

            StateAttribute[] additions = prev.GetAttributes().Except(curr.GetAttributes()).ToArray ();
            StateAttribute[] removings = curr.GetAttributes().Except(prev.GetAttributes()).ToArray ();
            StateAttribute[] mutationsPrev = prev.GetAttributes().Where (x => curr.GetAttributes ().Any (y => y.Name == x.Name && y.Description != x.Description)).ToArray ();
            StateAttribute[] mutationsCurr = curr.GetAttributes().Where (x => prev.GetAttributes ().Any (y => y.Name == x.Name && y.Description != x.Description)).ToArray ();

            int index = 0;
            string[] mutations = mutationsPrev.Select(x => $"{x.Description} => {mutationsCurr[index++].Description}").ToArray ();

            return new StateChanges (_additionsHeaders[curr.Identifier], additions.Select (x => x.Description).ToArray (),
                _removingsHeaders[curr.Identifier], removings.Select (x => x.Description).ToArray (),
                _mutationsHeaders[curr.Identifier], mutations);
        }

        public IEnumerable<StateChanges> GetChanges ()
        {
            List<StateChanges> changes = new List<StateChanges>();
            foreach (var pair in _currentStates)
            {
                var prev = _previousStates.GetValueOrDefault(pair.Key);
                if (prev == null)
                {
                    prev = new State(pair.Value.Identifier);
                }

                changes.Add (Compare(pair.Value, prev));
            }
            return changes;
        }

        public Embed ChangesToEmbed (string title)
        {
            IEnumerable<StateChanges> changes = GetChanges();
            EmbedBuilder result = new EmbedBuilder();

            foreach (var change in changes)
            {
                if (change.GetAdditions().Length > 0)
                {
                    result.AddField(change.AddedHeader,
                    $"```> {string.Join("\n> ", change.GetAdditions())}```");
                }

                if (change.GetRemovals().Length > 0)
                {
                    result.AddField(change.RemovedHeader,
                    $"```> {string.Join("\n> ", change.GetRemovals())}```");
                }

                if (change.GetMutations().Length > 0)
                {
                    result.AddField(change.MutatedHeader,
                    $"```> {string.Join("\n> ", change.GetMutations())}```");
                }
            }

            if (result.Fields.Count > 0)
            {
                result.WithTitle(title).WithDescription("The following tracked changes have occured.");
            }
            else
            {
                result.WithTitle(title).WithDescription("No tracked changes have occured.");
            }

            return result.Build ();
        }
    }
}
