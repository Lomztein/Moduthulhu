using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CustomCommands.IO
{
    public static class CustomCommandIO
    {
        private const string SET_METAFILE_NAME = "SET_METAFILE";
        public static string DataPath => DataSerialization.DataPath + "CustomCommands\\";

        /* Custom commands are saved into individual files in the current version.
         * Custom command sets are saved as a folder with a metadata file within.
         */


        public static CustomCommand LoadCommand (string path) {
            CustomChainData data = JSONSerialization.DeserializeFile<CustomChainData> (path);
            return data.CreateFrom () as CustomCommand;
        }

        public static void SaveCommand(CustomCommand cmd, string path) {
            JSONSerialization.SerializeObject (cmd.SaveToData (), path + cmd.Name);
        }

        // Also loads the commands inside the set recursively.
        public static CustomCommandSet LoadSet (string path) {
            CustomSetData data = JSONSerialization.DeserializeFile<CustomSetData> (path + "\\" + SET_METAFILE_NAME);
            CustomCommandSet set = data.CreateFrom () as CustomCommandSet;
            set.AddCommands (LoadAll (path + "\\"));
            return set;
        }

        public static void SaveSet (CustomCommandSet set, string path) {
            CustomSetData data = set.SaveToData () as CustomSetData;
            path += set.Name;

            DirectoryInfo info = Directory.CreateDirectory (path);
            JSONSerialization.SerializeObject (data, info.FullName + "\\" + SET_METAFILE_NAME, true);
            SaveAll (set.GetCommands ().Cast<ICustomCommand>().ToArray (), info.FullName + "\\");
        }

        public static ICustomCommand[] LoadAll (string sourcePath) {
            List<ICustomCommand> commands = new List<ICustomCommand> ();

            string[] files = Directory.GetFiles (sourcePath);
            string[] directories = Directory.GetDirectories (sourcePath);

            foreach (string file in files) {
                commands.Add (LoadCommand (file));
            }

            foreach (string directory in directories) {
                commands.Add (LoadSet (directory));
            }

            return commands.ToArray ();
        }

        public static void SaveAll (ICustomCommand[] commands, string targetPath) {
            foreach (ICustomCommand command in commands) {

                if (command is CustomCommand cmd) {
                    SaveCommand (cmd, targetPath);
                }

                if (command is CustomCommandSet set) {
                    SaveSet (set, targetPath);
                }
            }
        }
    }
}
