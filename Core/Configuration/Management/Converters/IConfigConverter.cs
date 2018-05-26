using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration.Management.Converters
{
    public interface IConfigConverter : IConfigModifier
    {

        object Convert(Type targetType, params string[] input);
    }
}
