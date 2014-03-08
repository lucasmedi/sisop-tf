using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ASMReader
{
	class Program
	{
		static void Main(string[] args)
		{
			int pc = -1;

			// Log: início da aplicação
			Console.WriteLine("**** SISOP - Etapa 1 *****");
			Console.WriteLine();
			Console.WriteLine("Passo 1: carregar arquivo .asm");
			Console.WriteLine("> Instancia variáveis de ambiente");

			// Performance
			var watch = new Stopwatch();
			watch.Start();

			// Variáveis de controle de abertura e fechamento de bloco
			var openDataRead = false;
			var openCodeRead = false;

			// Carrega lista de operadores
			var operators = MontaOperadores();

			// Blocos do programa
			var memory = new Memory();

			var dataIndex = new Dictionary<string, int>();

			// Dicionário com as posições das labels
			var labels = new Dictionary<string, int>();

			var filePath = @"..\..\files\code.asm";

			// Log: nome do arquivo a ser executado
			Console.WriteLine("> Inicia leitura do arquivo '{0}'", filePath);

			// Carrega código do arquivo .asm
			var file = new StreamReader(filePath);

			var precode = new List<string>();
			while (!file.EndOfStream)
			{
				var line = file.ReadLine();

				// Linha vazia é descartada
				if (string.IsNullOrEmpty(line))
					continue;

				precode.Add(line);
			}

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

					memory.Add(s[0]);
					memory.Add(s[1]);
					dataIndex.Add(s[0], memory.GetIndex() - 1);
				}
			}

			memory.Add(string.Empty);

			foreach (var line in precode)
			{
				// Se possui .code abre leitura de código
				if (line.Contains(".code"))
				{
					pc = memory.GetIndex();
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

						labels.Add(s[0].Replace(":", string.Empty), memory.GetIndex());
					}

					// Busca código do operador
					var op = (int)operators[s[operador]];

					memory.Add(op.ToString());

					if (dataIndex.ContainsKey(s[valor]))
					{
						memory.Add(dataIndex[s[valor]].ToString());
					}
					else if (labels.ContainsKey(s[valor]))
					{
						memory.Add(labels[s[valor]].ToString());
					}
					else
					{
						memory.Add(s[valor]);
					}
				}
			}

			// Log: fim do carregamento do arquivo e início do processamento
			watch.Stop();
			Console.WriteLine("Fim passo 1 - Tempo Total: {0}ms", watch.Elapsed.TotalSeconds * 1000.0);

			Console.WriteLine();

			Console.WriteLine("Imprimir estado da memória");
			var icount = 0;
			while (memory.HasNext(icount))
			{
				var o = memory.Get(icount);
				Console.WriteLine("> {0}: {1}", Convert.ToString(icount, 2).PadLeft(8, '0'), o);

				icount++;
			}
			Console.WriteLine("Fim imprimir");

			Console.WriteLine();
			Console.WriteLine("Passo 2: executar processamento");
			watch.Reset();
			watch.Start();

			// Valor inicial do contador
			var acc = -1;

			var processando = true;

			// Executa programa
			while (processando)
			{
				processando = memory.HasNext(pc);
				if (!processando)
					continue;

				int value = -1;

				var operador = memory.Get(pc);
				pc++;
				var operando = memory.Get(pc);
				pc++;
				
				var op = (Operators)int.Parse(operador);
				var logString = LogOperation(op);

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
						acc += value;

						// Log
						logString = string.Format(logString, value, acc);
						break;
					case Operators.SUB:
						acc -= value;

						// Log
						logString = string.Format(logString, value, acc);
						break;
					case Operators.MULT:
						acc *= value;

						// Log
						logString = string.Format(logString, value, acc);
						break;
					case Operators.DIV:
						acc /= value;

						// Log
						logString = string.Format(logString, value, acc);
						break;
					case Operators.LOAD:
						acc = value;

						// Log
						logString = string.Format(logString, acc, operando);
						break;
					case Operators.STORE:
						memory.Set(int.Parse(operando), acc.ToString());

						// Log
						logString = string.Format(logString, acc, operando);
						break;
					case Operators.BRANY:
					    pc = int.Parse(operando);

					    // Log
						logString = string.Format(logString, Convert.ToString(int.Parse(operando), 2));
					    break;
					case Operators.BRPOS:
					    if (acc > 0)
					    {
							pc = int.Parse(operando);
					    }

					    // Log
						logString = string.Format(logString, acc, Convert.ToString(int.Parse(operando), 2));
					    break;
					case Operators.BRZERO:
					    if (acc == 0)
					    {
							pc = int.Parse(operando);
					    }

					    // Log
						logString = string.Format(logString, acc, Convert.ToString(int.Parse(operando), 2));
					    break;
					case Operators.BRNEG:
					    if (acc < 0)
					    {
							pc = int.Parse(operando);
					    }

					    // Log
						logString = string.Format(logString, acc, Convert.ToString(int.Parse(operando), 2));
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

			Console.WriteLine("Fim passo 2 - Tempo Total: {0}ms", watch.Elapsed.TotalSeconds * 1000.0);
			Console.WriteLine();
			Console.WriteLine("Listar resultado das variáveis");
			foreach (var item in dataIndex)
			{
				Console.WriteLine("> '{0}': {1}", Convert.ToString(item.Value, 2).PadLeft(8, '0'), memory.Get(item.Value));
			}
			Console.WriteLine("Fim listar");
			Console.WriteLine();
			Console.WriteLine("**** HALT! *****");

			Console.ReadKey();
		}

		private static string LogOperation(Operators op)
		{
			var log = String.Empty;
			switch (op)
			{
				case Operators.ADD:
					log = "> (ADD) Adicionou {0} ao acumulador resultando em {1} ";
					break;
				case Operators.SUB:
					log = "> (SUB) Subtraiu {0} do acumulador resultando em {1} ";
					break;
				case Operators.MULT:
					log = "> (MULT) Multiplicou acumulador por {0} resultando em {1} ";
					break;
				case Operators.DIV:
					log = "> (DIV) Dividiu acumulador por {0} resultando em {1} ";
					break;
				case Operators.LOAD:
					log = "> (LOAD) Carregou valor {0} da variável '{1}' ao acumulador ";
					break;
				case Operators.STORE:
					log = "> (STORE) Salvou valor {0} do acumulador na variável '{1}' ";
					break;
				case Operators.BRANY:
					log = "> (BRANY) Salto incondicional para label '{0}' ";
					break;
				case Operators.BRPOS:
					log = "> (BRPOS) Salto condicional se ({0} > 0) ir para '{1}' ";
					break;
				case Operators.BRZERO:
					log = "> (BRZERO) Salto condicional se ({0} = 0) ir para '{1}' ";
					break;
				case Operators.BRNEG:
					log = "> (BRNEG) Salto condicional se ({0} < 0) ir para '{1}' ";
					break;
				case Operators.SYSCALL:
					log = "> (SYSCALL) Chamada de sistema: {0} ";
					break;
				case Operators.LABEL:
					log = "> (LABEL) Armazenou ponto de retorno '{0}' ";
					break;
				default:
					break;
			}

			return log;
		}

		private static Dictionary<string, int> MontaOperadores()
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
			operators.Add("LABEL", (int)Operators.LABEL);

			return operators;
		}

		public enum Operators
		{
			// Aritméticos
			ADD = 0000,
			SUB = 0001,
			MULT = 0010,
			DIV = 0011,

			// Acesso a memória
			LOAD = 0100,
			STORE = 0101,

			// Saltos
			BRANY = 1000,
			BRPOS = 1001,
			BRZERO = 1010,
			BRNEG = 1011,

			// Sistema
			SYSCALL = 1100,

			// Outro
			LABEL = 1101
		}
	}

	public class Memory
	{
		private string[] memory;
		private int key;

		public Memory()
		{
			memory = new string[8 * 1024];
			key = 0;
		}

		public void Add(string value)
		{
			memory[key] = value;
			key++;
		}

		public string Get(int key)
		{
			return memory[key];
		}

		public void Set(int key, string value)
		{
			memory[key] = value;
		}

		public int GetIndex()
		{
			return key;
		}

		public bool HasNext(int key)
		{
			return !((string.IsNullOrEmpty(memory[key]) && string.IsNullOrEmpty(memory[key + 1])) || key >= memory.Length);
		}
	}
}