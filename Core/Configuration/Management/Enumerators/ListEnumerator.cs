using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration.Management.Adders
{
    public class ListEnumerator : IConfigEnumerator {

        public Type TargetType => typeof (List<>);

        public void AddTo(dynamic enumerable, List<dynamic> objects) {
            enumerable.Add (objects[0]);
        }

        public void Change(dynamic enumerable, dynamic newObject, int index) {
            enumerable[index] = newObject;
        }

        public string ListToString(dynamic enumerable) {
            string result = "";
            for (int i = 0; i < enumerable.Count; i++) {
                result += i + " - " + enumerable[i].ToString () + "\n";
            }
            return result;
        }

        public void RemoveFrom(dynamic enumerable, int index) {
            enumerable.RemoveAt (index);
        }
    }
}
