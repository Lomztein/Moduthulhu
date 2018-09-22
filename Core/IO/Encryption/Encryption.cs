using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Encryption
{
    public static class Encryption
    {
        public static IEncryptor Encryptor { get; private set; } = new ZeroEncryptor ();

        public static string Encrypt (string input) {
            return Encryptor.Encrypt (input);
        }

        public static string Decrypt (string input) {
            return Encryptor.Decrypt (input);
        }
    }
}
