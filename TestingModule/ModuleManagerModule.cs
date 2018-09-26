using Lomztein.Moduthulhu.Core.Module.Framework;
using System;

namespace Lomztein.Moduthulhu.Modules.Meta
{
    public class ModuleManagerModule : ModuleBase {

        public override string Name => "Module Manager";
        public override string Description => "Manually keep track of and manage modules.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override void Initialize() {
        }

        public override void Shutdown() {
        }
    }
}
