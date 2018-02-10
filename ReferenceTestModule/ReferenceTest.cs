using Lomztein.ModularDiscordBot.Core.Module.Framework;
using TestModule;
using System;
using Lomztein.ModularDiscordBot.Core.Bot;

namespace ReferenceTestModule
{
    public class ReferenceTest : ModuleBase {

        public override string Name => "Module Reference Test";
        public override string Description => "The other side of the Reference Test module. Confusing, no?";
        public override string Author => "Lomztein";

        public override void Initialize() {
            
        }

        public override void PostInitialize() {
            Log.Write (Log.Type.WARNING, AppDomain.CurrentDomain.FriendlyName);
            var obj = new TestModule.ReferenceTest.NestedClass ();
            Log.Write (Log.Type.WARNING, obj.Method (2f, 2f).ToString ());
        }

        public override void Shutdown() {
            
        }
    }
}
