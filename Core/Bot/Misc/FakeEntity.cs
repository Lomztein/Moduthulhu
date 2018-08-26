using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Misc
{
    public class FakeEntity<T> : IEntity<T> where T : IEquatable<T> {
        private ulong x;

        public T Id { get; set; }

        public FakeEntity(T _id) {
            Id = _id;
        }
    }
}
