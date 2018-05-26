using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration
{
    /// <summary>
    /// This is not to be implemented directly, since it is only ment to be used as a common ancestor for generic IConfigurables.
    /// </summary>
    public interface IConfigurable {

        void Configure();

    }

    public interface IConfigurable<T> : IConfigurable where T : Config {

        T Configuration { get; set; }

    }
}
