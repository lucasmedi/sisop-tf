﻿using System;
using System.IO;
using System.Text;

namespace sisop_tf
{
	class Program
	{
		private static Memory memory;
        private static Processor processor;

		static void Main(string[] args)
		{
			// Log: início da aplicação
			Console.WriteLine("**** SISOP - Etapa 1 *****");
			Console.WriteLine();

			// Solicita tamanho da memória
			var memorySize = -1;
			do
			{
				Console.Write("Informe o tamanho da memória principal: ");
			} while (!int.TryParse(Console.ReadLine(), out memorySize) || memorySize <= 0);

			// Cria a memória com o tamanho informado
			memory = new Memory(memorySize);

			// Cria o processador para os processos da fila
			processor = new Processor(memory);

			Console.WriteLine("Passo 1: carregar arquivos");
			string[] filePaths = Directory.GetFiles(@"..\..\Files\", "*.asm");
			for (int i = 0; i < filePaths.Length; i++)
			{
				var filePath = filePaths[i];
				var fInfo = new FileInfo(filePath);

				Console.WriteLine("> Arquivo: {0}", fInfo.Name);

				// Solicita AT
				var at = -1;
				do
				{
					Console.Write("Informe o Arrival Time (AT) do processo: ");
				} while (!int.TryParse(Console.ReadLine(), out at));

				// Solicita prioridade
				Nullable<Priority> prioridade = null;
				do
				{
					var value = -1;
					Console.Write("Informe a prioridade do processo (0 - Alta, 1 - Media, 2): ");
					if (int.TryParse(Console.ReadLine(), out value))
					{
						switch (value)
						{
							case 0:
								prioridade = Priority.Alta;
								break;
							case 1:
								prioridade = Priority.Media;
								break;
							case 2:
								prioridade = Priority.Baixa;
								break;
						}
					}
				} while (!prioridade.HasValue);

				var process = new Process(filePath, at, prioridade.Value);
				processor.AddToWaiting(process);
			}

			Console.WriteLine();

			while (!processor.IsEmpty())
			{
				processor.Execute();
			}

			// Log: estado da memória
			MemoryPreview();

			Console.WriteLine();

			Console.WriteLine("**** HALT! *****");

			Console.ReadKey();
		}

		public static void MemoryPreview(int icount = 0, int id = 0)
		{
			if (id == 0)
				Console.WriteLine("Imprimir estado da memória");
			else
				Console.WriteLine("Imprimir estado da memória do processo {0}", id);

			var preview = new StringBuilder();
			while (memory.HasNext(icount))
			{
				var o = memory.GetValue(icount);
				var i = 0;
				if (int.TryParse(o, out i))
				{
					o = i.ToString("X");
				}

				preview.AppendLine(string.Format("> {0}: {1}", icount.ToString("X").PadLeft(8, '0'), o));

				icount++;
			}

			if (!string.IsNullOrEmpty(preview.ToString()))
			{
				Console.Write(preview.ToString());
			}
			else
			{
				Console.WriteLine("> Memória vazia");
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