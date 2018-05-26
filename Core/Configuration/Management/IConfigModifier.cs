using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration.Management
{
    public interface IConfigModifier
    {
        /// <summary>
        /// This is only used to figure out which converter to use. The specific type is parsed through the Convert method.
        /// </summary>
        Type TargetType { get; }
    }
}
