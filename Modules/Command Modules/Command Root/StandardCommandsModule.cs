using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.AdvDiscordCommands.ExampleCommands;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Extensions;

namespace Lomztein.Moduthulhu.Modules.Command
{
    [Dependency ("CommandRootModule")]
    public class StandardCommandsModule : PluginBase {

        public override string Name => "Standard Commands";
        public override string Description => "A module that adds all standard commands from the Advanced Discord Commands library.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private AdvDiscordCommands.Framework.Interfaces.ICommand [ ] commands = new AdvDiscordCommands.Framework.Interfaces.ICommand[ ] {
                new HelpCommand (),
                new DiscordCommandSet (),
                new FlowCommandSet (),
                new MathCommandSet (),
                new VariableCommandSet (),
                new CallstackCommand (),
                new PrintCommand (),
        };

        public override void Initialize() {
            var root = ParentContainer.GetCommandRoot ();
            root.AddCommands (commands);
        }

        public override void Shutdown() {
            var root = ParentContainer.GetCommandRoot ();
            root.RemoveCommands (commands);
        }
    }
}
