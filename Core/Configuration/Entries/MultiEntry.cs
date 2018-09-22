using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration {

    public class MultiEntry<TValue, TSouce> : EntryBase<TValue, TSouce> where TSouce : IEntity<ulong> {

        public Dictionary<ulong, TValue> Values { internal set; get; } = new Dictionary<ulong, TValue> ();

        public MultiEntry(Func<TSouce, TValue> defaultValueExpression, string key, bool isCritical = false) : base (defaultValueExpression, key, isCritical) { }

        public TValue GetEntry(IEntity<ulong> entity) {
            return Values[entity.Id];
        }

        public override void SetEntry(ulong id, object newObject) {
            if (!Values.ContainsKey (id))
                Values.Add (id, default (TValue));
            Values[id] = (TValue)newObject;
        }

        public override void UpdateEntry(ulong id) {
            SetEntry (id, ParentConfig.GetEntry<TValue> (id, Key));
        }
    }

    /// <summary>
    /// The Source generic argument in this is "SocketGuild".
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class MultiEntry<TValue> : MultiEntry<TValue, SocketGuild> {
        public MultiEntry (Func<SocketGuild, TValue> defaultValueExpression, string key, bool isCritical = false) : base (defaultValueExpression, key, isCritical) { }
    }
}
