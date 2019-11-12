using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO
{
    public static class Encryption
    {
        private static IEncryptor _encryptor = new ZeroEncryptor();

        public static string Encrypt (string input) {
            return _encryptor.Encrypt (input);
        }

        public static string Decrypt (string input) {
            return _encryptor.Decrypt (input);
        }
    }
}
