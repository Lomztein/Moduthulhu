using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration
{
    public interface IConfigurable {

        void Configure();

    }

    public interface IConfigurable<T> : IConfigurable where T : Config {

        T Configuration { get; set; }

    }
}
