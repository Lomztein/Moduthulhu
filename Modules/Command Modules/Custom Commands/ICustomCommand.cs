using Lomztein.AdvDiscordCommands.Framework.Interfaces;
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

    // Implementing ICommand into ICustomCommand shouldn't change anything in practice, but might avoid issues later down the line.
    public interface ICustomCommand : ICommand
    {
        CommandAccessability Accessability { get; set; }

        ulong OwnerID { get; set; }

        CustomCommandData SaveToData();
    }
}
