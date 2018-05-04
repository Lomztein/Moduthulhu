using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Meta
{
    public class ConfigurationManagerModule : ModuleBase {

        public override string Name => "Configuration Manager";
        public override string Description => "Used for interacting with and changing configuration.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override void Initialize() {
        }

        public override void Shutdown() {
        }
    }
}
