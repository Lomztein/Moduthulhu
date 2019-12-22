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

        public StateManager ()
        {
            Reset();
        }

        public void Reset ()
        {
            _previousStates = new Dictionary<string, State>(_currentStates);
            _currentStates.Clear();
        }

        public void AddAttribute (string target, string addedHeader, string removedHeader, string mutatedHeader, string name, string desc)
        {
            if (!_currentStates.ContainsKey (target))
            {
                _currentStates.Add(target, new State(addedHeader, removedHeader, mutatedHeader));
            }
            _currentStates[target].Add(name, desc);
        }

        public static StateChanges Compare (State prev, State curr)
        {
            StateAttribute[] additions = prev.GetAttributes().Except(curr.GetAttributes()).ToArray ();
            StateAttribute[] removings = curr.GetAttributes().Except(prev.GetAttributes()).ToArray ();
            StateAttribute[] mutationsPrev = prev.GetAttributes().Where (x => curr.GetAttributes ().Any (y => y.Name == x.Name && y.Description != x.Description)).ToArray ();
            StateAttribute[] mutationsCurr = curr.GetAttributes().Where (x => prev.GetAttributes ().Any (y => y.Name == x.Name && y.Description != x.Description)).ToArray ();

            int index = 0;
            string[] mutations = mutationsPrev.Select(x => $"{x.Description} => {mutationsCurr[index++].Description}").ToArray ();

            return new StateChanges (curr.AddedHeader, additions.Select (x => x.Description).ToArray (),
                curr.RemovedHeader, removings.Select (x => x.Description).ToArray (),
                curr.MutatedHeader, mutations);
        }

        public IEnumerable<StateChanges> GetChanges ()
        {
            List<StateChanges> changes = new List<StateChanges>();
            foreach (var pair in _currentStates)
            {
                var prev = _previousStates.GetValueOrDefault(pair.Key);
                if (prev == null)
                {
                    prev = new State(pair.Value.AddedHeader, pair.Value.RemovedHeader, pair.Value.MutatedHeader);
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
                    $"```{string.Join("\n", change.GetAdditions())}```");
                }

                if (change.GetRemovals().Length > 0)
                {
                    result.AddField(change.RemovedHeader,
                    $"```{string.Join("\n", change.GetRemovals())}```");
                }

                if (change.GetMutations().Length > 0)
                {
                    result.AddField(change.MutatedHeader,
                    $"```{string.Join("\n", change.GetMutations())}```");
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
