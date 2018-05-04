using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Misc.Logging.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToStringOrNull (this object obj) {
            if (obj == null)
                return "null";
            return obj.ToString ();
        }
    }
}
