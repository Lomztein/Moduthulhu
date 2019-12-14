using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public enum GDPRCompliance { Full, Partial, None }

    [AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GDPRAttribute : Attribute
    {
        public GDPRCompliance Compliance { get; private set; }
        public string[] Notes { get; private set; }

        public GDPRAttribute(GDPRCompliance compliance, params string[] notes)
        {
            Compliance = compliance;
            Notes = notes;
        }

        public GDPRAttribute(GDPRCompliance compliance) : this(compliance, Array.Empty<string>()) { }
    }
}
