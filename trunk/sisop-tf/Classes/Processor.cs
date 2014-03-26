using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sisop_tf
{
	public class Processor
	{
        public int Quantum { get; set; }
        public Memory memory { get; set; }

        public Processor(Memory mem, int qtn = 0)
        {
            memory = mem;
            Quantum = qtn;
            ToProcess = new Queue<Process>();
            Suspended = new List<Process>();
        }
        
        private Queue<Process> ToProcess { get; set; }
        public List<Process> Suspended { get; set; }

        public Process GetProcessToExecute()
        {
            return ToProcess.Dequeue();
        }

        public void AddProcessToQueue(Process p)
        {
            ToProcess.Enqueue(p);
        }

        public void ExecutaCodigo(Process process)
        {
            var processando = true;

            // Executa programa
            while (processando)
            {
                processando = process.HasNext();
                if (!processando)
                    continue;

                int value = -1;

                var operador = memory.Get(process.Pc);
                process.Next();
                var operando = memory.Get(process.Pc);
                process.Next();

                var op = (Operators)int.Parse(operador);
                var logString = Program.LogOperation(op);

                // verifica se é acesso imediato
                if (operando.Contains("#"))
                {
                    // remove o indicador de acesso imediato
                    int.TryParse(operando.Replace("#", string.Empty), out value);
                }
                // verifica se é acesso direto
                else if (op != Operators.SYSCALL && int.TryParse(operando, out value))
                {
                    // recupera valor do bloco de dados
                    value = int.Parse(memory.Get(value));
                }
                else
                {
                    continue;
                }

                // Executa a operação
                switch (op)
                {
                    case Operators.ADD:
                        process.LoadAc(process.Ac + value);

                        // Log
                        logString = string.Format(logString, value, process.Ac);
                        break;
                    case Operators.SUB:
                        process.LoadAc(process.Ac - value);

                        // Log
                        logString = string.Format(logString, value, process.Ac);
                        break;
                    case Operators.MULT:
                        process.LoadAc(process.Ac * value);

                        // Log
                        logString = string.Format(logString, value, process.Ac);
                        break;
                    case Operators.DIV:
                        process.LoadAc(process.Ac / value);

                        // Log
                        logString = string.Format(logString, value, process.Ac);
                        break;
                    case Operators.LOAD:
                        process.LoadAc(value);

                        // Log
                        logString = string.Format(logString, process.Ac, operando);
                        break;
                    case Operators.STORE:
                        memory.Set(int.Parse(operando), process.Ac.ToString());

                        // Log
                        logString = string.Format(logString, process.Ac, operando);
                        break;
                    case Operators.BRANY:
                        process.JumpTo(int.Parse(operando));

                        // Log
                        logString = string.Format(logString, int.Parse(operando).ToString("X"));
                        break;
                    case Operators.BRPOS:
                        if (process.Ac > 0)
                        {
                            process.JumpTo(int.Parse(operando));
                        }

                        // Log
                        logString = string.Format(logString, process.Ac, int.Parse(operando).ToString("X"));
                        break;
                    case Operators.BRZERO:
                        if (process.Ac == 0)
                        {
                            process.JumpTo(int.Parse(operando));
                        }

                        // Log
                        logString = string.Format(logString, process.Ac, int.Parse(operando).ToString("X"));
                        break;
                    case Operators.BRNEG:
                        if (process.Ac < 0)
                        {
                            process.JumpTo(int.Parse(operando));
                        }

                        // Log
                        logString = string.Format(logString, process.Ac, int.Parse(operando).ToString("X"));
                        break;
                    case Operators.SYSCALL:
                        // TODO: Rever funcionalidade
                        if (value == 0)
                        {
                            processando = false;

                            // Log
                            logString = string.Format(logString, "HALT");
                        }
                        else
                        {
                            // Log
                            logString = string.Format(logString, "?");
                        }
                        break;
                    default:
                        //throw new Exception("Better Call Saul!");
                        break;
                }

                Console.WriteLine(logString);
            }
        }
	}
}