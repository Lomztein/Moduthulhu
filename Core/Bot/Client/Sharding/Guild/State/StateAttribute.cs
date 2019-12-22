using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement
{
    public class StateAttribute
    {
        public readonly string Name;
        public readonly string Description;

        public StateAttribute(string name, string desc)
        {
            Name = name;
            Description = desc;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode(StringComparison.InvariantCultureIgnoreCase) + Description.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is StateAttribute other)
            {
                return Name == other.Name && Description == other.Description;
            }
            return false;
        }
    }
}
