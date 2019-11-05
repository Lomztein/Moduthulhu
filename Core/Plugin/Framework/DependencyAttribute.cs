using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    [AttributeUsage (AttributeTargets.Class)]
    public class DependencyAttribute : Attribute
    {
        public string DependencyName { get; private set; }

        public DependencyAttribute (string dependency) {
            DependencyName = dependency;
        }
    }
}
