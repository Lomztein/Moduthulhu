using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    [AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DescriptorAttribute : Attribute
    {
        public string Author { get; private set; }
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }

        public DescriptorAttribute (string author, string name, string description = "", string version = "1.0.0")
        {
            Author = author;
            Name = name;
            Version = version;
            Description = description;
        }
    }
}
