using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    [AttributeUsage (AttributeTargets.Class)]
    public class DependencyAttribute : Attribute
    {
        public string DependencyName { get; private set; }
        public string DesiredVersion { get; private set; }

        public DependencyAttribute (string dependency, string desiredVersion) {
            DependencyName = dependency;
            DesiredVersion = desiredVersion;
        }

        public DependencyAttribute (string dependency) : this (dependency, "1.0.0") { }
    }
}
