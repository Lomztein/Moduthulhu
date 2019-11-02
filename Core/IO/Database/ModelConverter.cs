using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Lomztein.Moduthulhu.Core.IO.Database
{
    public static class ModelConverter
    {
        public static void ReadData (object model, Dictionary<string, object> data)
        {
            // Get the Type object of the incoming model object.
            Type modelType = model.GetType();

            // Grap all Property members with the ModelProperty attribuet of the previously recieved type, through reflection.
            IEnumerable<PropertyInfo> properties = modelType.GetProperties().Where (x => x.GetCustomAttribute<ModelPropertyAttribute>() != null);
            properties = properties.Count() == 0 ? modelType.GetProperties() : properties; // If none were found previously, just select all properties.

            for (int i = 0; i < properties.Count (); i++)
            {
                PropertyInfo property = properties.ElementAt (i);
                ModelPropertyNameAttribute modelPropertyName = property.GetCustomAttribute<ModelPropertyNameAttribute>();

                // Use a null-coalescing operator to overwrite the properties name if anoter is defined by the ModelProperty attribuet.
                string name = modelPropertyName?.DatabaseAttributeName ?? property.Name;
                object attribute = data[property.Name.ToLower()];

                // Manually set the value of the property through reflection.
                property.SetValue(model, attribute);
            }
        }

        public static T ReadData<T> (Dictionary<string, object> data) where T : new () // Declare that types without a no-arg constructor cannot be used with this generic method.
        {
            // Create an instance of the model based on the given generic type.
            var instance = (T)Activator.CreateInstance(typeof(T));
            // Use the previously defined method and return the newly filled model instance.
            ReadData(instance, data);
            return instance;
        }

        public static void ReadData (object[] models, Dictionary<string, object>[] data)
        {
            if (models.Length != data.Length)
            {
                throw new InvalidOperationException("Model array length does not equal data length");
            }
            
            int length = models.Length;
            for (int i = 0; i < length; i++)
            {
                // Run the basic method for every single given set of models and data.
                ReadData(models[i], data[i]);
            }
        }

        public static T[] ReadData<T> (Dictionary<string, object>[] data) where T : new () // Practically identical to non-generic variant, except generic.
        {
            T[] objects = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                objects[i] = ReadData<T>(data[i]);
            }
            return objects;
        }
    }

    [AttributeUsage (AttributeTargets.Property)]
    public class ModelPropertyAttribute : Attribute 
    {
        public ModelPropertyAttribute () { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ModelPropertyNameAttribute : Attribute
    {
        public string DatabaseAttributeName { get; private set; }


        public ModelPropertyNameAttribute(string databaseAttributeName)
        {
            DatabaseAttributeName = databaseAttributeName;
        }
    }
}
