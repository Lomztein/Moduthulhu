using Discord;
using Lomztein.Moduthulhu.Core.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public class UserList : IEnumerable<ulong>
    {
        private IList<ulong> _users;
        private readonly string _filePath;

        public UserList (string filePath) {
            _filePath = filePath;
            Load ();
        }

        public bool Contains(ulong id) => _users.Contains (id);

        public void AddUser(ulong id) {
            _users.Add (id);
            Save ();
        }

        public void RemoveUser(ulong id) {
            _users.Remove (id);
            Save ();
        }

        public void SetUsers (IEnumerable<ulong> newAdministrators) {
            _users = newAdministrators.ToList ();
            Save ();
        }

        private void Load () {
            _users = JSONSerialization.DeserializeFile<List<ulong>> (_filePath);
            if (_users == null) {
                _users = new List<ulong> { 0 };
                Save ();
            }
        }

        private void Save () {
            JSONSerialization.SerializeObject (_users, _filePath);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_users).GetEnumerator();
        }

        IEnumerator<ulong> IEnumerable<ulong>.GetEnumerator()
        {
            return _users.GetEnumerator();
        }
    }
}
