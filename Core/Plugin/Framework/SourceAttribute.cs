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

        public SourceAttribute (string author, string project, string patch)
        {
            AuthorURI = author;
            ProjectURI = project;
            PatchURI = patch;
        }

        public SourceAttribute (string author, string project) : this (author, project, null) { }
        public SourceAttribute (string author) : this (author, null, null) { }
        public SourceAttribute () : this (null, null, null) { }
    }
}
