using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class MissingPermissionException : Exception
    {
        public MissingPermissionException(string message) : base(message)
        {
        }

        public MissingPermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MissingPermissionException()
        {
        }

        protected MissingPermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
