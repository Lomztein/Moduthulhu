using System;
using System.Collections.Generic;
using System.Linq;
using Lomztein.Moduthulhu.Core.Extensions;
using System.Collections;
using Lomztein.Moduthulhu.Core.Configuration.Management.Converters;
using Lomztein.Moduthulhu.Core.Configuration.Management.Adders;

namespace Lomztein.Moduthulhu.Core.Configuration.Management {

    // TODO: Figure out a way to use a "Source" system for easier configuration. As in instead of text input, a user can just type in a channel name or something, and a "SourceCoverter" can convert it automatically.

    /// <summary>
    /// Contains a bunch of methods for modifying and listing configuration during runtime. There are few compile-time checks in place due to a large amount of dynamics and objects, so be careful.
    /// </summary>
    public static class ConfigurationManager
    {
        private static List<IConfigConverter> converters = new List<IConfigConverter> () {
            new KeyValuePairConverter (),
        };

        private static List<IConfigEnumerator> enumerators = new List<IConfigEnumerator> () {
            new DictionaryEnumerator (),
            new ListEnumerator (),
        };

        public static object ConvertTo (Type targetType, params string[] input) {

            object result = null; // Not entirely sure why I do it like this, but I do.

            // First, try out in-build conversions.
            try {
                result = Convert.ChangeType (input.Single (), targetType);
                return result;
            } catch {
                // Try and go through custom converters.
                IConfigConverter converter = converters.Find (x => x.TargetType == targetType);
                if (converter != null) {
                    return converter.Convert (targetType, input);
                }
            }

            throw new InvalidOperationException ("Unable to convert \"" + input.Singlify (", ") + "\" to type \"" + targetType.Name + "\" - No suitable converter exists.");
        }

        public static void AddConverter (IConfigConverter converter) => converters.Add (converter);
        public static void AddAdderRemover (IConfigEnumerator adderRemover) => enumerators.Add (adderRemover);

        public static void ChangeEntry(this IConfigurable configurable, ulong id, string key, bool save, bool manual, params string[] input) {
            Type type = configurable.GetEntryType (id, key);
            object newValue = ConvertTo (type, input);
            configurable.GetConfig ().SetEntry (id, key, newValue, save, true);
            configurable.GetConfigEntry (key).UpdateEntry (id);
        }

        public static string ListToString(this IConfigurable configurable, ulong id, string key) {
            dynamic currentEnumerable = configurable.GetConfig ().GetEntry (id, key) as IEnumerable;
            if (currentEnumerable == null)
                throw new InvalidOperationException ($"\"{key}\" is not an enumerable.");

            currentEnumerable = ConvertToListIfArray (currentEnumerable, out bool isArray);

            IConfigEnumerator enumerator = FindConfigEnumerator (currentEnumerable);
            if (enumerator != null)
                return enumerator.ListToString (currentEnumerable);
            throw new InvalidOperationException ($"Unable to list \"{key}\" - No suitable lister exists.");

        }

        public static void AddToEntry(this IConfigurable configurable, ulong id, string key, bool save, bool manual, params string[] input) {
            dynamic currentEnumerable = configurable.GetConfig ().GetEntry (id, key) as IEnumerable;
            if (currentEnumerable == null)
                throw new InvalidOperationException ($"\"{key}\" is not an enumerable.");

            currentEnumerable = ConvertToListIfArray (currentEnumerable, out bool isArray);
            Type enumerableType = currentEnumerable.GetType ();

            Type[] genericTypes = enumerableType.GetGenericArguments ();
            List<dynamic> genericValues = new List<dynamic> ();
            for (int i = 0; i < genericTypes.Length; i++) {
                object result = ConvertTo (genericTypes[i], input[i]);
                genericValues.Add (result);
            }

            IConfigEnumerator enumerator = FindConfigEnumerator (currentEnumerable);

            if (enumerator != null) {
                enumerator.AddTo (currentEnumerable, genericValues);

                if (isArray)
                    currentEnumerable = currentEnumerable.ToArray ();

                configurable.GetConfig ().SetEntry (id, key, currentEnumerable, save, manual);
                configurable.GetConfigEntry (key).UpdateEntry (id);
                return;
            }

            throw new InvalidOperationException ($"Unable to add \"{input.Singlify ()}\" to \"{key}\" - No suitable adders exist.");
        }

        public static object RemoveFromEntry (this IConfigurable configurable, ulong id, string key, bool save, bool manual, int index) {
            dynamic currentEnumerable = configurable.GetConfig ().GetEntry (id, key) as IEnumerable;
            if (currentEnumerable == null)
                throw new InvalidOperationException ($"\"{key}\" is not an enumerable.");

            currentEnumerable = ConvertToListIfArray (currentEnumerable, out bool isArray);

            IConfigEnumerator enumerator = FindConfigEnumerator (currentEnumerable);
            object element = Enumerable.ElementAt (currentEnumerable, index);

            if (enumerator != null) {
                enumerator.RemoveFrom (currentEnumerable, index);

                if (isArray)
                    currentEnumerable = currentEnumerable.ToArray ();

                configurable.GetConfig ().SetEntry (id, key, currentEnumerable, save, manual);
                configurable.GetConfigEntry (key).UpdateEntry (id);

                return element;
            }

            throw new InvalidOperationException ($"Unable to remove \"{element}\" from \"{key}\" - No suitable removers exist.");
        }

        private static IConfigEnumerator FindConfigEnumerator(Object enumerable) => FindConfigEnumerator (enumerable.GetType ());
        private static IConfigEnumerator FindConfigEnumerator(Type enumerableType) => enumerators.Find (x => x.TargetType == enumerableType.GetGenericTypeDefinition ());

        private static IEnumerable ConvertToListIfArray (dynamic array, out bool isArray) {
            isArray = false;
            Type type = array.GetType ();

            if (type.IsArray) {
                Type arrayType = type.GetElementType ();
                IList list = Activator.CreateInstance (typeof (List<>).MakeGenericType (arrayType)) as IList;

                foreach (dynamic obj in array) {
                    Type t = obj.GetType ();
                    list.Add (obj);
                }

                isArray = true;
                return list;
            }
            return array;
        }

    }
}
