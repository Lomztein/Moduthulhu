using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration.Management.Adders
{
    public class DictionaryEnumerator : IConfigEnumerator {

        public Type TargetType => typeof (Dictionary<,>);

        public void AddTo(dynamic enumerable, List<dynamic> objects) {
            enumerable.Add (objects[0], objects[1]);
        }

        public string ListToString (dynamic enumerable) {
            string result = "";
            for (int i = 0; i < enumerable.Count; i++) {
                dynamic pair = Enumerable.ElementAt (enumerable, i);
                result += i + " - " + pair.Key + ", " + pair.Value + "\n";
            }
            return result;
        }

        public void RemoveFrom(dynamic enumerable, int index) {
            enumerable.Remove (index);
        }

        public void Change (dynamic enumerable, dynamic newObject, int index) {
            dynamic pair = Enumerable.ElementAt (enumerable, index);
            enumerable.Remove [pair.Key] = newObject;
        }
    }
}
