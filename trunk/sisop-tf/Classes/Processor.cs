using System;
using System.Collections.Generic;

namespace sisop_tf
{
	public class Processor
	{
		private Memory memory { get; set; }
        private Memory mainMemory { get; set; }
		private int quantum { get; set; }

		private Queue<Process> processing { get; set; }
		private List<Process> waiting { get; set; }

        public Processor(Memory mem, Memory main, int qtn = 5)
		{
			memory = mem;
            mainMemory = main;
			quantum = qtn;

			processing = new Queue<Process>();
			waiting = new List<Process>();
		}

		public void AddToQueue(Process p)
		{
			// TODO: Adicionar tratamento, verificando se há espaço na memória
            if (mainMemory.GetFreeMemorySize() >= p.ProgramSize)
            {
                if (waiting.Contains(p))
                    waiting.Remove(p);

                processing.Enqueue(p);
                MoveToMainMemory(p);
            }
            else
                waiting.Add(p);
		}

        private void MoveToMainMemory(Process p)
        {
            //Valida se tem espaco partindo do primeiro espaco vazio na memoria
            var countNullConsecutivos = 0;
            var indexInicial = 0;
            for (var i = 0; i < mainMemory.GetSize(); i++)
            {
                if (string.IsNullOrEmpty(mainMemory.Get(i)))
                    countNullConsecutivos++;
                else
                {
                    indexInicial = i - countNullConsecutivos;
                    countNullConsecutivos = 0;
                }

                if (countNullConsecutivos == p.ProgramSize)
                {
                    var codeLine = p.BeginData;
                    var j = (indexInicial != 0) ? indexInicial + 1 : indexInicial;
                    var size = p.ProgramSize;
                    while (size > 0)
                    {                        
                        mainMemory.Set(j, memory.Get(codeLine));
                        codeLine++;
                        j++;
                        size--;
                    }
                }
            }            
        }

		public Process GetNext()
		{
			return processing.Dequeue();
		}

		public bool IsEmpty()
		{
            List<Process> actualWaiting = new List<Process>();
            actualWaiting = waiting;

            if (processing.Count == 0)
            {
                waiting.ForEach(x => {
                    if (x.Priority == Priority.Alta)
                        AddToQueue(x);
                    else if(x.Priority == Priority.Medio)
                        AddToQueue(x);
                    else
                        AddToQueue(x);

                    //actualWaiting.Remove(x);
                });
            }

            waiting = actualWaiting;

			return processing.Count == 0 && waiting.Count == 0;
		}

        public void RemoveFromMainMemory(Process p)
        {
            //for (int i = p.BeginCode - 1; i < p.EndCode; i++)
            //    mainMemory.Set(i, null);
        }

		public void Execute()
		{
			int control = 0;
			var processando = true;

			var process = GetNext();

			Console.WriteLine("Processando: {0}", process.Id);

			// Executa programa
			while (processando)
			{
				processando = process.HasNext() && control < quantum;
                if (!processando)
                {
                    RemoveFromMainMemory(process);
                    continue;
                }

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
				else if (op == Operators.SYSCALL)
				{

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
							process.JumpTo(process.EndCode);
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

				control++;
			}

            //if (process.HasNext() && control == quantum)
            //    AddToQueue(process);

			Console.WriteLine();
		}
	}
}