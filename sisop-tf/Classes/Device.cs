using sisop_tf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sisop_tf.Classes
{
    public class Device
    {
        //- Slot a que pertence
        //- Modos permitidos (escrita, leitura ou ambos)
        //- Nome do dispositivo
        //- Tempo da requisição de leitura
        //- Tempo da requisição de escrita
        public int Slot;
        public Method Method;
        public String Name;
        public TimeSpan ReadTime;
        public TimeSpan WriteTime;
        public Queue<Tuple<TimeSpan, TimeSpan>> Requests;

        public Device(int slot, Method method, String name, TimeSpan? read, TimeSpan? write)
        {
            Slot = slot;
            Method = method;
            Name = name;
            ReadTime = (read.HasValue ? read.Value : new TimeSpan(-1));
            WriteTime = (write.HasValue ? write.Value : new TimeSpan(-1));
            Requests = new Queue<Tuple<TimeSpan, TimeSpan>>();
            Requests.Enqueue(new Tuple<TimeSpan,TimeSpan>(ReadTime, WriteTime));
        }

    }
}
