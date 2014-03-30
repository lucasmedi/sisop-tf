using System;
using System.Collections.Generic;

namespace sisop_tf
{
	public class Processor
	{
		private Memory memory { get; set; }
		private int quantum { get; set; }

		private Queue<Process> processing { get; set; }
		private List<Process> waiting { get; set; }

		public Processor(Memory mem, int qtn = 5)
		{
			memory = mem;
			quantum = qtn;

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

		public void Execute()
		{
			int control = 0;
			var processando = true;
			var bloqueado = false;

			var process = GetNext();

			Console.WriteLine("Processando: {0}", process.Id);

			// Executa programa
			while (processando)
			{
				processando = process.HasNext() && control < quantum;
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
				else if (op == Operators.SYSCALL)
				{
					int.TryParse(operando, out value);
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
						switch (value)
						{
							case 0:
								process.JumpTo(process.EndCode);
								
								// Log
								logString = string.Format(logString, "HALT");
								break;
							case 1:
								Console.WriteLine("Impressão do AC: {0}", process.Ac);

								// Bloqueia processo
								bloqueado = true;

								// Log
								logString = string.Format(logString, "OUTPUT: " + process.Ac);
								break;
							case 2:
								bool aceito = false;
								do
								{
									Console.Write("Leitura para AC: ");
									var input = Console.ReadLine();
									aceito = int.TryParse(input, out value);

									if (!aceito)
										Console.WriteLine("Valor inválido!");
								} while (!aceito);

								process.LoadAc(value);
								
								// Bloqueia processo
								bloqueado = true;

								// Log
								logString = string.Format(logString, "INPUT: {0}", value);
								break;
						}

						// finaliza o processamento
						processando = false;

						break;
					default:
						//throw new Exception("Better Call Saul!");
						break;
				}

				Console.WriteLine(logString);

				control++;
			}

			//if (bloqueado)
				// Bloquear processo por tempo determinado

            if (process.HasNext() && control == quantum)
				AddToQueue(process);

			Console.WriteLine();
		}
	}
}