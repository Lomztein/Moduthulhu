using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Encryption
{
    public class ZeroEncryptor : IEncryptor {

        public string Decrypt(string input) {
            return input;
        }

        public string Encrypt(string input) {
            return input;
        }
    }
}
