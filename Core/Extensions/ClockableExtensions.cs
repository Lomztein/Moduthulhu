using Lomztein.Moduthulhu.Core.Clock;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class ClockableExtensions
    {
        public static Clock.Clock GetClock(this IModule module) => module.ParentShard.Core.Clock;
    }
}
