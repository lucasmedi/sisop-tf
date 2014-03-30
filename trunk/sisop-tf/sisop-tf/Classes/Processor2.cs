using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace sisop_tf.Classes
{
    class Processor2
    {

        private Memory memory { get; set; }

        private Queue<Process> processing { get; set; }
        private List<Process> waiting { get; set; }

        public Processor2(Memory mem)
        {
            memory = mem;

            processing = new Queue<Process>();
            waiting = new List<Process>();
        }

        public void AddToQueue(Process p)
        {
            // TODO: Adicionar tratamento, verificando se há espaço na memória

            processing.Enqueue(p);
        }

        public Process GetNext()
        {
            return processing.Dequeue();
        }
        public bool IsEmpty()
        {
            return processing.Count == 0;
        }


        public void Executar()
        {
         
        }



    }
}
