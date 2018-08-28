using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public enum CommandAccessability {
        Private, // Only creator can user it.
        Public, // Everyone on creators server can use it.
        Global // Litteraly global for the entire bot.
    }

    public interface ICustomCommand
    {

        CommandAccessability Accessability { get; set; }

        ulong OwnerID { get; set; }

        string Name { get; }

        CustomCommandData SaveToData();

    }
}
