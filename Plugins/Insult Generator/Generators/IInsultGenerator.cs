using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.InsultGenerators
{
    public interface IInsultGenerator
    {
        string Insult(string target);
    }
}
