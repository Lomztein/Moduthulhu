using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration.Management.Converters
{
    public class KeyValuePairConverter : IConfigConverter {

        public Type TargetType => typeof (KeyValuePair<,>);

        public object Convert(Type targetType, params string[] input) {
            Type[] genericTypes = targetType.GetGenericArguments ();
            dynamic dictionary = Activator.CreateInstance (targetType);

            List<object> generics = new List<object> ();
            for (int i = 0; i < genericTypes.Length; i++) {
                object obj = ConfigurationManager.ConvertTo (genericTypes[i], input[i]);
            }

            dictionary.Add (generics[0], generics[1]);
            return dictionary;
        }
    }
}
