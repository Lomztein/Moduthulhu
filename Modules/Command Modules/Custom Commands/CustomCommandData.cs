using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CustomCommands.Data
{
    public class CustomCommandData
    {
        public string name;
        public string description;

        public ulong ownerID;
        public CommandAccessability accessability;

        public void ApplyTo (ICustomCommand command) {
            command.OwnerID = ownerID;
            command.Accessability = accessability;

            if (command is AdvDiscordCommands.Framework.Command cmd) {
                cmd.Name = name;
                cmd.Description = description;
            }
        }

        public virtual ICustomCommand CreateFrom() => throw new NotImplementedException ();

    }

    public class CustomChainData : CustomCommandData {

        public string commandChain;

        public override ICustomCommand CreateFrom() {
            CustomCommand newCommand = new CustomCommand ();
            ApplyTo (newCommand);
            newCommand.commandChain = commandChain;
            return newCommand;
        }

    }

    public class CustomSetData : CustomCommandData {

        public override ICustomCommand CreateFrom() {
            CustomCommandSet newSet = new CustomCommandSet ();
            ApplyTo (newSet);
            return newSet;
        }

    }
}
