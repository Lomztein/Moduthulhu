using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public class PluginDisabledException : Exception
    {
        public PluginDisabledException(string message) : base(message)
        {
        }

        public PluginDisabledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PluginDisabledException()
        {
        }

        protected PluginDisabledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
