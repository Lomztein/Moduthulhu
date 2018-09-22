using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Encryption
{
    public interface IEncryptor
    {
        string Encrypt(string input);

        string Decrypt(string input);
    }
}
