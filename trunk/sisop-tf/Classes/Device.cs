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
        public int SyscallValue;
        public Method Method;
        public String Name;
        public int ReadTime;
        public int WriteTime;
        public Queue<Tuple<int, int>> Requests;
        

        public Device(int slot, int syscall, Method method, String name, int? read, int? write)
        {
            Slot = slot;
            SyscallValue = syscall;
            Method = method;
            Name = name;
            ReadTime = FormatTimespan(read);
            WriteTime = FormatTimespan(write);
            Requests = new Queue<Tuple<int, int>>();
        }

        public void AddRequest(int? read, int? write)
        {
            Requests.Enqueue(new Tuple<int, int>(FormatTimespan(read), FormatTimespan(write)));
        }

        public void RemoveRequest()
        {
            Requests.Dequeue();
        }

        private int FormatTimespan(int? time)
        {
            return (time.HasValue ? time.Value : -1);
        }

        public void PrintRequestQueue()
        {
            var count = 1;
            Console.WriteLine();
            Console.WriteLine("-> Impressão da fila de requests: {0}", this.Name.ToString());
            foreach (Tuple<int, int> fila in this.Requests)
            {
                Console.WriteLine("->->-> Item {0} da fila tem tempos (Read/Write): {1} e {2}", count, fila.Item1, fila.Item2);
                count++;
            }
        }
    }
}
