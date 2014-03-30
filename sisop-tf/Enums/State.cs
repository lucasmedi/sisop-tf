using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sisop_tf.Enums
{
    public enum State
    {
        New = 0,
        Ready = 1,
        ReadySuspend = 2,
        Blocked = 3,
        BlockedSuspend = 4,
        Running = 5,
        Exit = 6
    }
}
