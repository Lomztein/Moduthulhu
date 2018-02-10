using Lomztein.ModularDiscordBot.Core.Bot;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestModule
{
    public class ReferenceTest : ModuleBase {

        public override string Name => "Reference Test";
        public override string Description => "A module for testing the limits of required dependencies of C#.";
        public override string Author => "Lomztein";

        public override void Initialize() {
            throw new NotImplementedException ();
        }

        public override void Shutdown() {
            throw new NotImplementedException ();
        }

        public class NestedClass {
            
            public float Method (float n1, float n2) {
                return n1 + n2;
            }

        } 
    }
}
