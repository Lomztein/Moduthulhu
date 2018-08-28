using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Clock
{
    public interface ITickable
    {
        void Tick(DateTime prevTick, DateTime now);

        void Initialize();
    }
}
