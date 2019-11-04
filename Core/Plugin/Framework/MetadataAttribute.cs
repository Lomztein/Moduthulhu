﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    [AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MetadataAttribute : Attribute
    {
        public string Author { get; private set; }
        public string Name { get; private set; }
    }
}
