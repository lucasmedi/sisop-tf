using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sisop_tf
{
	public class Processor
	{
		private Memory memory { get; set; }
		private int quantum { get; set; }
		private int totalTime = 0;

		private Queue<Process> processing { get; set; }
		private List<Process> waiting { get; set; }

		private Random random;
        
		public Processor(Memory mem, int qtn = 5)
		{
			memory = mem;
			quantum = qtn;

			processing = new Queue<Process>();
			waiting = new List<Process>();

			random = new Random(new Random().Next(0, 1000000));
		}

		#region Controling

		/// <summary>
		/// Adiciona processo a fila de execução
		/// </summary>
		/// <param name="p"></param>
		public void AddToQueue(Process p)
		{
			processing.Enqueue(p);
		}

		/// <summary>
		/// Adiciona processo a lista de bloqueio/suspensão
		/// </summary>
		/// <param name="p"></param>
        public void AddToWaiting(Process p)
        {
			if (p.Size <= 0)
				p.Size = PreRead(p);

            waiting.Add(p);
        }

		/// <summary>
		/// Pega próximo processo da fila
		/// </summary>
		/// <returns></returns>
		public Process GetNext()
		{
			return processing.Dequeue();
		}

		/// <summary>
		/// Verifica se existe algo a processar
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty()
		{
			return processing.Count == 0 && waiting.Count == 0;
		}

		#endregion

		#region Execution

		/// <summary>
		/// Executa um processo da fila
		/// </summary>
		public void Execute()
		{
			Console.WriteLine("Tempo Total: {0}\n", totalTime);

			OrganizeWaiting();

			int control = 0;
			var isProcessing = true;
			var blocked = false;
			
			Process process = null;
			if (processing.Count > 0)
			{
				process = GetNext();
				process.State = State.Running;
				Console.WriteLine("Processando: {0}", process.Id);
			}
			else
			{
				control = quantum;
				isProcessing = false;
			}

			// Executa programa
			while (isProcessing)
			{
                isProcessing = process.HasNext() && control < quantum;
				if (!isProcessing)
					continue;

				int value = -1;

				var operador = memory.GetValue(process.Pc);
				process.Next();
				var operando = memory.GetValue(process.Pc);
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
					value = int.Parse(memory.GetValue(value));
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
						memory.SetValue(int.Parse(operando), process.Ac.ToString());

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
						switch (value)
						{
							case 0:
								process.JumpTo(process.EndCode);
								
								// Log
								logString = string.Format(logString, "HALT - PROCESSO TERMINADO");
								break;
							case 1:
								Console.WriteLine("Impressão do AC: {0}", process.Ac);

								// Bloqueia processo
								blocked = true;

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
								blocked = true;

								// Log
								logString = string.Format(logString, "INPUT: {0}", value);
								break;
						}

						// finaliza o processamento
						isProcessing = false;

						break;
					default:
						//throw new Exception("Better Call Saul!");
						break;
				}

				Console.WriteLine(logString);

				control++;
			}

			totalTime += control;

			if (process != null)
			{
				if (blocked)
				{
					process.State = State.Blocked;
					var waitTime = random.Next(10, 40);
					process.At = totalTime + waitTime;
					AddToWaiting(process);

					Console.WriteLine("Bloqueia processo {0} com AT para {1}", process.Id, process.At);
				}
				else if (process.HasNext() && control == quantum)
				{
					process.State = State.Ready;
					AddToQueue(process);
				}
				else
				{
					process.State = State.Exit;
					Deallocate(process);
				}

				Console.WriteLine();
			}
		}

		/// <summary>
		/// Remove o processo informado da memória
		/// </summary>
		/// <param name="process">Processo a ser removido</param>
		private void Deallocate(Process process)
		{
			for (int i = process.BeginData; i <= process.EndCode; i++)
			{
				this.memory.SetValue(i, null);
			}

			Console.WriteLine("Desalocou o processo {0} da memória principal.", process.Id);
		}

		/// <summary>
		/// Organiza execução dos processos
		/// </summary>
		private void OrganizeWaiting()
		{
			var removed = new List<Process>();

			waiting = waiting.OrderBy(o => o.Priority).ToList<Process>();
			foreach (var process in waiting)
			{
				if (process.Size > memory.Size)
				{
					removed.Add(process);
					Console.WriteLine("Processo {0} ignorado por falta de espaço na memória principal.", process.Id);
					Console.WriteLine();
					continue;
				}

				if ((process.State == State.New || process.State == State.ReadySuspend) && process.At <= totalTime)
				{
					var position = -1;
					if (memory.HasSpace(process.Size, out position))
					{
						// Adiciona para remoção da waiting
						removed.Add(process);

						// Carrega na memória
						Read(process, position);

						// Altera estado para State.Ready
						process.State = State.Ready;

						// Adiciona na fila
						AddToQueue(process);
					}
					else
					{
						// Altera estado para State.ReadySuspend
						process.State = State.ReadySuspend;
					}
				}

				if (process.State == State.Blocked && process.At <= totalTime)
				{
					// Adiciona para remoção da waiting
					removed.Add(process);

					// Altera estado para State.Ready
					process.State = State.Ready;

					// Adiciona na fila
					AddToQueue(process);
				}
			}

			foreach (var item in removed)
			{
				waiting.Remove(item);
			}
		}

		/// <summary>
		/// Lê o arquivo carregando os dados para a memória principal
		/// </summary>
		/// <param name="process">Processo que está sendo carregado</param>
		/// <param name="position">Posição inicial de escrita na memória</param>
		/// <returns></returns>
		private Process Read(Process process, int position)
		{
			// Define o ponto inicial a carregado o programa
			var key = position;

			// Variáveis de controle de abertura e fechamento de bloco
			var openDataRead = false;
			var openCodeRead = false;

			// Carrega lista de operadores
			var operators = CreateOperators();

			var dataIndex = new Dictionary<string, int>();

			// Dicionário com as posições das labels
			var labels = new Dictionary<string, int>();

			// Log: nome do arquivo a ser executado
			Console.WriteLine("> Carregamento do processo {0}. Arquivo: '{1}'", process.Id, process.FilePath);

			// Carrega código do arquivo .asm
			var file = new StreamReader(process.FilePath);

			var precode = new List<string>();
			while (!file.EndOfStream)
			{
				var line = file.ReadLine();

				// Linha vazia é descartada
				if (string.IsNullOrEmpty(line))
					continue;

				precode.Add(line);
			}

			// Guarda a primeira posição de data
			int beginData = key;

			// Inicia o carregamento do arquivo
			foreach (var line in precode)
			{
				// Se possui .data abre leitura de dados
				if (line.Contains(".data"))
				{
					openDataRead = true;
					continue;
				}

				// Se possui .enddata fecha leitura de dados
				if (line.Contains(".enddata"))
				{
					openDataRead = false;
					continue;
				}

				// Se está lendo dados
				if (openDataRead)
				{
					var s = line.Trim().Split(' ');
					memory.SetValue(key, s[1]);
					key++;
					dataIndex.Add(s[0], key - 1);
				}
			}

			// Guarda a última posição de data / primeira posição de código
			int beginCode = key;
			int pc = -1;

			foreach (var line in precode)
			{
				// Se possui .code abre leitura de código
				if (line.Contains(".code"))
				{
					pc = key;
					openCodeRead = true;
					continue;
				}

				// Se possui .endcode fecha leitura de código
				if (line.Contains(".endcode"))
				{
					openCodeRead = false;
					continue;
				}

				// Se está lendo código
				if (openCodeRead)
				{
					// Controla posição de leitura
					var operador = 0;
					var valor = 1;

					var s = line.Trim().Split(' ');

					// Se possui mais de 2 posições é tratado como LABEL
					if (s.Length > 2)
					{
						// Desloca posição de leitura
						operador = 1;
						valor = 2;

						labels.Add(s[0].Replace(":", string.Empty), key);
					}

					// Busca código do operador
					var op = (int)operators[s[operador]];

					memory.SetValue(key, op.ToString());
					key++;

					if (dataIndex.ContainsKey(s[valor]))
					{
						memory.SetValue(key, dataIndex[s[valor]].ToString());
					}
					else if (labels.ContainsKey(s[valor]))
					{
						memory.SetValue(key, labels[s[valor]].ToString());
					}
					else
					{
						memory.SetValue(key, s[valor]);
					}

					key++;
				}
			}

			// Guarda a última posição de código
			int endCode = key - 1;
			process.SetParameters(beginData, beginCode, endCode);
			process.JumpTo(pc);

			// Imprime o espaço alocado pelo processo na memória
			Program.MemoryPreview(process.BeginData, process.Id);
			Console.WriteLine();

			return process;
		}

		/// <summary>
		/// Calcula o espaço necessário para o processo, simulando o carregamento em uma memória auxiliar
		/// </summary>
		/// <param name="process">Processo a ser avialiado</param>
		/// <returns></returns>
		private int PreRead(Process process)
		{
			var preMemory = new Memory();
			var key = 0;

			// Variáveis de controle de abertura e fechamento de bloco
			var openDataRead = false;
			var openCodeRead = false;

			// Carrega lista de operadores
			var operators = CreateOperators();

			var dataIndex = new Dictionary<string, int>();

			// Dicionário com as posições das labels
			var labels = new Dictionary<string, int>();

			// Carrega código do arquivo .asm
			var file = new StreamReader(process.FilePath);

			var precode = new List<string>();
			while (!file.EndOfStream)
			{
				var line = file.ReadLine();

				// Linha vazia é descartada
				if (string.IsNullOrEmpty(line))
					continue;

				precode.Add(line);
			}

			// Guarda a primeira posição de data
			int beginData = key;

			// Inicia o carregamento do arquivo
			foreach (var line in precode)
			{
				// Se possui .data abre leitura de dados
				if (line.Contains(".data"))
				{
					openDataRead = true;
					continue;
				}

				// Se possui .enddata fecha leitura de dados
				if (line.Contains(".enddata"))
				{
					openDataRead = false;
					continue;
				}

				// Se está lendo dados
				if (openDataRead)
				{
					var s = line.Trim().Split(' ');
					preMemory.SetValue(key, s[1]);
					key++;
					dataIndex.Add(s[0], key - 1);
				}
			}

			// Guarda a última posição de data / primeira posição de código
			int beginCode = key;
			int pc = -1;

			foreach (var line in precode)
			{
				// Se possui .code abre leitura de código
				if (line.Contains(".code"))
				{
					pc = key;
					openCodeRead = true;
					continue;
				}

				// Se possui .endcode fecha leitura de código
				if (line.Contains(".endcode"))
				{
					openCodeRead = false;
					continue;
				}

				// Se está lendo código
				if (openCodeRead)
				{
					// Controla posição de leitura
					var operador = 0;
					var valor = 1;

					var s = line.Trim().Split(' ');

					// Se possui mais de 2 posições é tratado como LABEL
					if (s.Length > 2)
					{
						// Desloca posição de leitura
						operador = 1;
						valor = 2;

						labels.Add(s[0].Replace(":", string.Empty), key);
					}

					// Busca código do operador
					var op = (int)operators[s[operador]];
					preMemory.SetValue(key, op.ToString());
					key++;

					if (dataIndex.ContainsKey(s[valor]))
					{
						preMemory.SetValue(key, dataIndex[s[valor]].ToString());
					}
					else if (labels.ContainsKey(s[valor]))
					{
						preMemory.SetValue(key, labels[s[valor]].ToString());
					}
					else
					{
						preMemory.SetValue(key, s[valor]);
					}
					
					key++;
				}
			}

			// Guarda a última posição de código
			int endCode = key - 1;

			return (endCode - beginData) + 1;
		}

		#endregion

		#region Custom

		/// <summary>
		/// Cria lista de operadores para auxiliar na leitura do arquivo
		/// </summary>
		/// <returns></returns>
		private static Dictionary<string, int> CreateOperators()
		{
			/*
			Categoria (Código)  Mnemônico       Função                      Código (binário)
			Aritmético (00)     ADD op1         acc=acc+(op1)               00
								SUB op1         acc=acc–(op1)               01
								MULT op1        acc=acc*(op1)               10
								DIV op1         acc=acc/(op1)               11
			Memória (01)        LOAD op1        acc=(op1)                   00
								STORE op1       (op1)=acc                   01
			Salto (10)          BRANY label     pc <- label                 00
								BRPOS label     Se acc > 0 então pc <-op1   01
								BRZERO label    Se acc = 0 então pc <-op1   10
								BRNEG label     Se acc < 0 então pc <-op1   11
			Sistema (11)        SYSCALL index   Chamada de sistema          00
								LABEL           label de sistema            01
								.data           area de dados               11
			*/

			var operators = new Dictionary<string, int>();
			operators.Add("ADD", (int)Operators.ADD);
			operators.Add("SUB", (int)Operators.SUB);
			operators.Add("MULT", (int)Operators.MULT);
			operators.Add("DIV", (int)Operators.DIV);

			operators.Add("LOAD", (int)Operators.LOAD);
			operators.Add("STORE", (int)Operators.STORE);

			operators.Add("BRANY", (int)Operators.BRANY);
			operators.Add("BRPOS", (int)Operators.BRPOS);
			operators.Add("BRZERO", (int)Operators.BRZERO);
			operators.Add("BRNEG", (int)Operators.BRNEG);

			operators.Add("SYSCALL", (int)Operators.SYSCALL);

			return operators;
		}

		#endregion
	}
}