using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;

namespace Lomztein.ModularDiscordBot.Modules.Voice
{
    public class VoiceNameModule : ModuleBase, IConfigurable {

        public override string Name => "Voice Name Changer";
        public override string Description => "Changes voice channel names to reflect the games played within.";
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
