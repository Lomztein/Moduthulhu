using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    [AttributeUsage (AttributeTargets.Class)]
    public class SourceAttribute : Attribute
    {
        public Uri AuthorURI { get; private set; }
        public Uri ProjectURI { get; private set; }
        public Uri PatchURI { get; private set; }

        public SourceAttribute (Uri author, Uri project, Uri patch)
        {
            AuthorURI = author;
            ProjectURI = project;
            PatchURI = patch;
        }
    }
}
