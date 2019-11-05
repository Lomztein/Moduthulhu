using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Misc
{
    public class FakeEntity<T> : IEntity<T> where T : IEquatable<T> {

        public T Id { get; set; }

        public FakeEntity(T id) {
            Id = id;
        }
    }
}
