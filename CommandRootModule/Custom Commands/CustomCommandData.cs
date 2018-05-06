using Lomztein.AdvDiscordCommands.Framework;
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
        public Command.Category catagory;

        public void ApplyTo (ICustomCommand command) {
            command.OwnerID = ownerID;
            command.Accessability = accessability;

            if (command is Command cmd) {
                cmd.command = name;
                cmd.shortHelp = description;
                cmd.catagory = catagory;
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

        // The only limitation to nested sets currently, is deserialization of the data. I can't quite figure it out right now,
        // so I've constricted it to only allow a single layer sets. I doubt multiple-layer sets would be used much regardless, 
        // but I'd still like to figure it out at some point in the future. For now, this will do.
        public List<CustomChainData> nestedCommands = new List<CustomChainData> ();

        public override ICustomCommand CreateFrom() {
            CustomCommandSet newSet = new CustomCommandSet ();
            ApplyTo (newSet);

            List<ICustomCommand> newCommands = new List<ICustomCommand> ();
            foreach (var nested in nestedCommands) {
                ICustomCommand cmd = nested.CreateFrom ();
                newCommands.Add (cmd);
            }

            newSet.AddCommands (newCommands.Cast<Command>().ToArray ());
            return newSet;
        }

    }
}
