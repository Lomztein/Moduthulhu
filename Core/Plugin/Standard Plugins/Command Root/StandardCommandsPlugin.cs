using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.AdvDiscordCommands.ExampleCommands;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Extensions;

namespace Lomztein.Moduthulhu.Plugins.Standard

{
    [Descriptor("Lomztein", "Standard Commands", "Implements all the default commands from the command framework.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    public class StandardCommandsPlugin : PluginBase {

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
            SendMessage ("CommandRoot.AddCommands", commands);
        }

        public override void Shutdown() {
            SendMessage ("CommandRoot.RemoveCommands", commands);
        }
    }
}
