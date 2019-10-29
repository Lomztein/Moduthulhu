using Lomztein.Moduthulhu.Core.Clock;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class ClockableExtensions
    {
        public static Clock.Clock GetClock(this Plugin.Framework.IPlugin module) => module.ParentShard.Core.Clock;
    }
}
