using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Modules.Voice
{
    public class AutoVoiceNameModule : ModuleBase {

        public override string Name => "Auto Voice Names";
        public override string Description => "Automatically renames voice channels based on games played within.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override void Initialize() {
            
        }

        public override void Shutdown() {
            throw new NotImplementedException ();
        }
    }
}
