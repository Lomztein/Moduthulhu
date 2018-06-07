using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration
{
    /// <summary>
    /// Purely to be used as a common ancestor for all generic entry classes. It is not to be inhereted from
    /// </summary>
    public abstract class EntryBase {

        /// <summary>
        /// If all IsCritical marked entries hasn't been manually configured for a specific id, then IConfigurable.IsConfigured (ulong id) will return false.
        /// </summary>
        public bool IsCritical { protected set; get; }
        public string Key { protected set; get; }

        internal Config ParentConfig { set; get; }

        public abstract void SetEntry(ulong id, object newObject);

        public abstract void UpdateEntry(ulong id);

        public EntryBase(string key, bool isCritical = false) {
            Key = key;
            isCritical = IsCritical;
        }

    }

    public abstract class EntryBase<TValue, TSource> : EntryBase where TSource : IEntity<ulong> {

        public Func<TSource, TValue> DefaultValueExpression { get; private set; }

        public TValue GetDefault(TSource source) => DefaultValueExpression (source);

        public EntryBase (Func<TSource, TValue> defaultValueExpression, string key, bool isCritical = false) : base (key, isCritical) {
            DefaultValueExpression = defaultValueExpression;
        }

    }
}
