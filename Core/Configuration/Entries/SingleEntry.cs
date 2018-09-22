using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration {

    public class SingleEntry<TValue, TSource> : EntryBase<TValue, TSource> where TSource : IEntity<ulong> {

        public TValue Value { get; internal set; } = default (TValue);

        public SingleEntry(Func<TSource, TValue> defaultValueExpression, string key, bool isCritical = false) : base (defaultValueExpression, key, isCritical) { }

        public TValue GetValue() => Value;

        public override void SetEntry(ulong id, object newValue) {
            Value = (TValue)newValue;
        }

        public override void UpdateEntry(ulong id) {
            SetEntry (SingleConfig.SINGLE_ID, ParentConfig.GetEntry<TValue> (SingleConfig.SINGLE_ID, Key));
        }
    }

    /// <summary>
    /// The Source generic argument in this is "SocketGuild".
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SingleEntry<TValue> : SingleEntry<TValue, SocketGuild> {
        public SingleEntry(Func<SocketGuild, TValue> defaultValueExpression, string key, bool isCritical = false) : base (defaultValueExpression, key, isCritical) { }
    }
}
