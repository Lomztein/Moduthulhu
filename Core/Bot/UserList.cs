using Discord;
using Lomztein.Moduthulhu.Cross;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public class UserList
    {
        public List<ulong> Users { get; private set; }
        private string FilePath { get; set; }

        public UserList (string filePath) {
            FilePath = filePath;
            Load ();
        }

        public bool Contains(ulong id) => Users.Contains (id);

        public void AddUser(ulong id) {
            Users.Add (id);
            Save ();
        }

        public void RemoveUser(ulong id) {
            Users.Remove (id);
            Save ();
        }

        public void SetUsers (IEnumerable<ulong> newAdministrators) {
            Users = newAdministrators.ToList ();
            Save ();
        }

        private void Load () {
            Users = JSONSerialization.DeserializeFile<List<ulong>> (FilePath);
            if (Users == null) {
                Users = new List<ulong> () { 0 };
                Save ();
            }
        }

        private void Save () {
            JSONSerialization.SerializeObject (Users, FilePath);
        }
    }
}
