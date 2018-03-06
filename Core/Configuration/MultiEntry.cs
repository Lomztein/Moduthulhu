using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration {
    public struct MultiEntry<T> {

        public Dictionary<ulong, T> values;

        public MultiEntry(Dictionary<ulong, T> _values) {
            values = _values;
        }

        public T GetEntry (IEntity<ulong> entity) {
            return values [ entity.Id ];
        }
    }
}
