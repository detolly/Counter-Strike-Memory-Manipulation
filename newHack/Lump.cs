using laExternalMulti.Objects.Implementation.CSGO.Data.BSP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace laExternalMulti.Objects.Implementation.CSGO.Data.BSP.Structs
{
    public struct Lump
    {
        public int offset, length, version, fourCC;
        public LumpType type;
    }
}
