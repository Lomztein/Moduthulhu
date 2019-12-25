using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginInitializationException : Exception
    {
        public PluginInitializationException(string message) : base(message)
        {
        }

        public PluginInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PluginInitializationException()
        {
        }

        protected PluginInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
