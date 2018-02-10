using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;

namespace VoiceModules
{
    public class ChannelNameModule : ModuleBase {

        public override string Name => "Channel Names";
        public override string Description => "This module handles renaming voice channels based on the game played within.";
        public override string Author => "Lomztein";
        public override bool Multiserver => false;

        public override void Initialize() {
            throw new NotImplementedException ();
        }

        public override void Shutdown() {
            throw new NotImplementedException ();
        }
    }
}
