using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.Config
{
    public struct ConfigFunctionParam : IEquatable<ConfigFunctionParam>
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }

        public ConfigFunctionParam (Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj is ConfigFunctionParam other)
            {
                return Type == other.Type && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + Name.GetHashCode(StringComparison.Ordinal);
        }

        public static bool operator ==(ConfigFunctionParam left, ConfigFunctionParam right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConfigFunctionParam left, ConfigFunctionParam right)
        {
            return !(left == right);
        }

        public bool Equals(ConfigFunctionParam other)
        {
            return this == other;
        }
    }
}
