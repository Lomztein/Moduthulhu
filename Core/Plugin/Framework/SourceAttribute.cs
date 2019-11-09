using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    [AttributeUsage (AttributeTargets.Class)]
    public class SourceAttribute : Attribute
    {
        public string AuthorURI { get; private set; }
        public string ProjectURI { get; private set; }
        public string PatchURI { get; private set; }

        public SourceAttribute (string author = null, string project = null, string patch = null)
        {
            AuthorURI = author;
            ProjectURI = project;
            PatchURI = patch;
        }
    }
}
