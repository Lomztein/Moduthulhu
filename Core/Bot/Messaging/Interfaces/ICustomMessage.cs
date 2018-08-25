using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced
{
    public interface ICustomMessage<TSource, TIntermediate, TResult> : IDeletable, ISendable<TResult, TIntermediate>
    {
        void CreateFrom(TSource source);
    }
}
