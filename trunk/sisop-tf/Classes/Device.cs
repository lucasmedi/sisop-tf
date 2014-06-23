using System;
using System.Linq;
using System.Collections.Generic;
using sisop_tf.Enums;

namespace sisop_tf.Classes
{
    public class Device
    {
        //- Slot a que pertence
        private int slot;

        //- Tempo da requisição de leitura
        private int readTime;

        //- Tempo da requisição de escrita
        private int writeTime;

        // Fila de requisições
        private Queue<DeviceRequest> requests;

        //- Nome do dispositivo
        public String Name { get; private set; }

        // Valor do syscall
        public int SyscallValue { get; private set; }

        //- Modos permitidos (escrita, leitura ou ambos)
        public Method Method { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="syscall"></param>
        /// <param name="method"></param>
        /// <param name="name"></param>
        /// <param name="read"></param>
        /// <param name="write"></param>
        public Device(int slot, int syscall, Method method, String name, int read, int write)
        {
            this.slot = slot;
            this.SyscallValue = syscall;
            this.Method = method;
            this.Name = name;
            this.readTime = read;
            this.writeTime = write;
            this.requests = new Queue<DeviceRequest>();
        }

        public void AddRequest(string pId)
        {
            var pTime = 0;
            switch (Method)
            {
                case Method.READ:
                    pTime = readTime;
                    break;
                case Method.WRITE:
                    pTime = writeTime;
                    break;
                case Method.ALL:
                    pTime = readTime + writeTime;
                    break;
            }

            requests.Enqueue(new DeviceRequest {
                Id = pId,
                Time = pTime
            });
        }

        public void ControlRequest(int time, string ignoredId)
        {
            var request = requests.FirstOrDefault();
            if (request == null || request.Id == ignoredId)
                return;

            var pTime = request.Time;
            if (pTime > time)
            {
                request.Time -= time;
            }
            else
            {
                Program.WriteLine(string.Format("> Finalizado atendimento do processo {0}", request.Id));
                Program.WriteLine("");
                requests.Dequeue();

                if (pTime < time)
                {
                    request = requests.FirstOrDefault();
                    if (request != null && request.Id != ignoredId)
                    {
                        request.Time -= (time - pTime);
                    }
                }
            }
        }

        public bool HasDevice(string pId)
        {
            return requests.Count(o => o.Id == pId) > 0;
        }

        public void PrintRequestQueue()
        {
            Program.WriteLine(string.Format("> Impressão da fila do dispositivo: {0}", this.Name.ToString()));
            if (requests.Count() > 0)
            {
                foreach (var fila in this.requests)
                {
                    Program.WriteLine(string.Format("Processo {0} da fila tem tempo (Leitura+Escrita): {1}", fila.Id, fila.Time));
                }
            }
            else
            {
                Program.WriteLine("Fila vazia.");
            }
        }
    }
}