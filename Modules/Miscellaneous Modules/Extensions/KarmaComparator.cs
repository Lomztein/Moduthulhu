using Discord.WebSocket;
using Lomztein.Moduthulhu.Modules.Misc.Karma;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Misc.Karma.Extensions
{
    public class KarmaComparator : IComparer<SocketUser> {

        public KarmaModule sourceModule;

        public KarmaComparator(KarmaModule _sourceModule) => sourceModule = _sourceModule;

        public int Compare(SocketUser x, SocketUser y) {
            return sourceModule.GetKarma (y.Id).Total - sourceModule.GetKarma (x.Id).Total;
        }

    }
}
