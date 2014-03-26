using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace sisop_tf
{
	class Program
	{
		// Blocos do programa
		private static Memory memory;
        private static Processor processor;

		static void Main(string[] args)
		{            
			// Performance
			var watch = new Stopwatch();
			watch.Start();

			memory = new Memory();
            //Cria o processador para os processos da fila
            processor = new Processor(memory);

			// Log: início da aplicação
			Console.WriteLine("**** SISOP - Etapa 1 *****");
			Console.WriteLine();
			Console.WriteLine("Passo 1: carregar arquivo .asm");
			Console.WriteLine("> Instancia variáveis de ambiente");

			var filePath = @"..\..\Files\code.asm";
			var process = Read(filePath);

			// Log: fim do carregamento do arquivo e início do processamento
			watch.Stop();
			Console.WriteLine("Fim passo 1 - Tempo Total: {0}ms", watch.Elapsed.TotalSeconds * 1000.0);

			Console.WriteLine();

			ImprimeMemoria();

			Console.WriteLine();
			Console.WriteLine("Passo 2: executar processamento");
			watch.Reset();
			watch.Start();

            //Adiciona o processo na Fila de ToProcess
            processor.AddProcessToQueue(process);

			Console.WriteLine("Fim passo 2 - Tempo Total: {0}ms", watch.Elapsed.TotalSeconds * 1000.0);
			
			// TODO: Rever maneira de gerar um print dos dados
			//Console.WriteLine();

			//Console.WriteLine("Listar resultado das variáveis");
			//foreach (var item in dataIndex)
			//{
			//	Console.WriteLine("> '{0}': {1}", Convert.ToString(item.Value, 2).PadLeft(8, '0'), memory.Get(item.Value));
			//}
			//Console.WriteLine("Fim listar");

			Console.WriteLine();

			Console.WriteLine("**** HALT! *****");

			Console.ReadKey();
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

			return operators;
		}

		private static Process Read(string filePath)
		{
			// Variáveis de controle de abertura e fechamento de bloco
			var openDataRead = false;
			var openCodeRead = false;

			// Carrega lista de operadores
			var operators = MontaOperadores();

			var dataIndex = new Dictionary<string, int>();

			// Dicionário com as posições das labels
			var labels = new Dictionary<string, int>();

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

			// Guarda a primeira posição de data
			int beginData = memory.GetIndex();

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
					memory.Add(s[1]);
					dataIndex.Add(s[0], memory.GetIndex() - 1);
				}
			}

			// Guarda a última posição de data / primeira posição de código
			int beginCode = memory.GetIndex();
			int pc = -1;

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

			// Guarda a última posição de código
			int endCode = memory.GetIndex() -1;

			var process = new Process(beginData, beginCode, endCode);
			process.JumpTo(pc);
			return process;
		}

		private static void ImprimeMemoria()
		{
			Console.WriteLine("Imprimir estado da memória");
			var icount = 0;
			while (memory.HasNext(icount))
			{
				var o = memory.Get(icount);
				var i = 0;
				if (int.TryParse(o, out i))
				{
					o = i.ToString("X");
				}

				Console.WriteLine("> {0}: {1}", icount.ToString("X").PadLeft(8, '0'), o);

				icount++;
			}
			Console.WriteLine("Fim imprimir");
		}

		public static string LogOperation(Operators op)
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
					log = "> (LOAD) Carregou valor {0} da posição '{1}' ao acumulador ";
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
				default:
					break;
			}

			return log;
		}
	}
}