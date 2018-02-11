using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Lomztein.AdvDiscordCommands.ExampleCommands;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.AdvDiscordCommands.Framework;

namespace Lomztein.ModularDiscordBot.Modules.CommandRoot
{
    public class StandardCommandsModule : ModuleBase {

        public override string Name => "Standard Commands";
        public override string Description => "A module that adds all standard commands from the Advanced Discord Commands library.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override string [ ] RequiredModules { get => new string [ ] { "Lomztein.Command Root" }; }

        private Command [ ] commands = new Command [ ] {
                new HelpCommand (),
                new DiscordCommandSet (),
                new FlowCommandSet (),
                new MathCommandSet (),
                new VariableCommandSet (),
                new MiscCommandSet (),
                new CallstackCommand (),
                new PrintCommand (),
        };

        public override void Initialize() { }

        public override void PostInitialize() {
            var root = ParentModuleHandler.GetCommandRoot ();
            root.AddCommands (commands);
        }

        public override void Shutdown() {
            var root = ParentModuleHandler.GetCommandRoot ();
            root.RemoveCommands (commands);
        }
    }
}
