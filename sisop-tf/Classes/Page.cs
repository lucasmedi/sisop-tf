using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sisop_tf.Classes
{
    public class Page
    {
        public int Size { get; set; }
        public int PosInMemory { get; set; }
        public PageState State { get; set; }

        public Page(int size, int initialPosInMemory, PageState state)
        {
            PosInMemory = initialPosInMemory;
            State = state;
            Size = size;
        }
    }

    public enum PageState
    {
        Full = 0,
        Empty = 1
    }
}
