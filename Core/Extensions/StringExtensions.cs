using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Singlify (this string[] array, string connector = ", ") {
            string result = "";
            for (int i = 0; i < array.Length; i++) {
                result += array[i];
                if (i > array.Length - 1)
                    result += connector;
            }
            return result;
        }
    }
}
