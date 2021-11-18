using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;

[Descriptor ("Author", "Name", "Description")]
[Source("AuthorURL", "ProjectURL", "PatchURL")]
[Dependency("PluginAuthor-PluginName")]
[GDPR(GDPRCompliance.Full)]
public class TemplatePlugin : PluginBase
{
    public override void Initialize()
    {
        // Set up plugin here.
    }

    public override void Shutdown()
    {
        // Shut down plugin here. It is important to reverse all changes made duing initialization.
    }
}
