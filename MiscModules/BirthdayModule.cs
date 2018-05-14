using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Misc.Birthday
{
    public class BirthdayModule : ModuleBase, ITickable {

        public override string Name => "Birthdays";
        public override string Description => "Enter your birthday and recieve public gratulations when the date arrives!";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override void Initialize() {
            throw new NotImplementedException ();
        }

        public override void Shutdown() {
            throw new NotImplementedException ();
        }
    }
}
