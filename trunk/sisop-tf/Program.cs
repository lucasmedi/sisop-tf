using sisop_tf.Classes;
using sisop_tf.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace sisop_tf
{
    class Program
    {
        private static PagedMemory memory;
        private static Processor processor;

        static void Main(string[] args)
        {
            // Log: início da aplicação
            Console.WriteLine("**** SISOP - Etapa 3 *****");
            Program.WriteLine("**** SISOP - Etapa 3 *****");

            Console.WriteLine();
            Program.WriteLine("");

            try
            {
                Execute();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Erro: pasta ou arquivos .asm não encontrados.");
                Program.WriteLine("Erro: pasta ou arquivos .asm não encontrados.");

                Console.WriteLine("Pressione qualquer tecla para finalizar.");
                Program.WriteLine("Pressione qualquer tecla para finalizar.");
            }
            catch (Exception e)
            {
                Console.WriteLine("\nErro ao executar aplicação!");
                Program.WriteLine("\nErro ao executar aplicação!");

                Console.WriteLine(e.StackTrace);
                Program.WriteLine(e.StackTrace);

                Console.WriteLine("Pressione qualquer tecla para finalizar.");
                Program.WriteLine("Pressione qualquer tecla para finalizar.");
            }

            Close();

            Console.ReadKey();
        }

        public static void Execute()
        {
            // Solicita tamanho da memória
            var pageSize = -1;
            do
            {
                Console.Write("Informe o tamanho da página: ");
            } while (!int.TryParse(Console.ReadLine(), out pageSize) || pageSize <= 0);

            var qtdPages = -1;
            do
            {
                Console.Write("Informe a quantidade de páginas: ");
            } while (!int.TryParse(Console.ReadLine(), out qtdPages) || qtdPages <= 0);

            // Cria a memória com o tamanho informado
            memory = new PagedMemory(pageSize, qtdPages);

            // Solicita o modo de escalonamento
            Nullable<SchedulerType> scheduler = null;
            do
            {
                var value = -1;
                Console.Write("Informe o modo de escalonamento (1 - SJF Preemptivo, 2 - Round Robin): ");
                if (int.TryParse(Console.ReadLine(), out value))
                {
                    switch (value)
                    {
                        case 1:
                            scheduler = SchedulerType.SJF_P;
                            Console.WriteLine("SJF Preemptivo indisponível. Favor utilizar Round Robin.");
                            scheduler = null;
                            break;
                        case 2:
                            scheduler = SchedulerType.RoundRobin;
                            break;
                    }
                }
            } while (!scheduler.HasValue);

            // Cria o processador para os processos da fila conforme o tipo informado
            switch (scheduler.Value)
            {
                case SchedulerType.SJF_P:
                    //processor = new ProcessorSJFP(memory);
                    break;
                case SchedulerType.RoundRobin:
                    processor = new ProcessorRobin(memory);
                    break;
                default:
                    break;
            }

            // Slots dos dispositivos
            var slots = 1;
            var syscall = 0;
            var readTime = 0;
            var writeTime = 0;
            var inputMethod = Method.READ;
            var inputSelected = InputType.VGA;

            do
            {
                var value = -1;
                Console.WriteLine("Dispositivos:");
                Console.WriteLine("1 - Impressao na tela (Escrita)");
                Console.WriteLine("2 - Teclado (Leitura)");
                Console.WriteLine("3 - Impressora  (Leitura/Escrita)");
                Console.WriteLine("4 - Scanner (Leitura)");
                Console.Write("Escolha o dispositivo do Slot (syscall) {0}: ", slots);
                if (int.TryParse(Console.ReadLine(), out value))
                {
                    switch (value)
                    {
                        case 1:
                            inputSelected = InputType.VGA;
                            inputMethod = Method.WRITE;
                            syscall = 1;
                            readTime = 0;
                            writeTime = 10;
                            break;
                        case 2:
                            inputSelected = InputType.KEYBOARD;
                            inputMethod = Method.READ;
                            syscall = 2;
                            readTime = 8;
                            writeTime = 0;
                            break;
                        case 3:
                            inputSelected = InputType.PRINTER;
                            inputMethod = Method.ALL;
                            syscall = 3;
                            readTime = 16;
                            writeTime = 20;
                            break;
                        case 4:
                            inputSelected = InputType.SCANNER;
                            inputMethod = Method.READ;
                            syscall = 4;
                            readTime = 16;
                            writeTime = 0;
                            break;
                    }
                }

                processor.AddToSlotList(new Device(slots, syscall, inputMethod, inputSelected.ToString(), readTime, writeTime));
                slots++;
            } while (slots <= 4);

            Console.WriteLine();
            Program.WriteLine("");

            // Log: estado da memória
            MemoryPreview();

            Console.WriteLine();
            Program.WriteLine("");

            Console.WriteLine("Passo 1: carregar arquivos");
            var filePaths = Directory.GetFiles(@"..\..\Files\", "*.asm");
            
            for (int i = 0; i < filePaths.Length; i++)
            {
                var filePath = filePaths[i];
                var fInfo = new FileInfo(filePath);

                string opt = "";
                while(!(opt=="s" || opt =="n"))
                {
                    Console.Write("Deseja carregar o programa " + fInfo.Name + " (s/n)?");
                    opt = Console.ReadLine();
                }
                if(!opt.Equals("s"))
                    continue;

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
                    Console.Write("Informe a prioridade do processo (0 - Alta, 1 - Media, 2 - Baixa): ");
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
            Program.WriteLine("");

            processor.Execute();

            // Log: estado da memória
            MemoryPreview();

            Console.WriteLine();
            Program.WriteLine("");

            Console.WriteLine("**** HALT! *****");
            Program.WriteLine("**** HALT! *****");

            Console.ReadKey();
        }

        private static StreamWriter writer;

        public static void MemoryPreview(Process p = null)
        {
            IList<int> pages = null;
            if (p == null)
            {
                WriteLine("Imprimir estado da memória");
                pages = memory.GetPageIds();
            }
            else
            {
                WriteLine(string.Format("Imprimir estado da memória do processo {0}", p.Id));
                pages = p.Pages;
            }

            var preview = new StringBuilder();
            preview.AppendLine("Formato: <Page>-<Line>: <Value>");

            foreach (var pageId in pages)
            {
                for (int i = 0; i < memory.PageSize; i++)
                {
                    var o = memory.GetValue(pageId, i);

                    var n = 0;
                    if (int.TryParse(o, out n))
                    {
                        o = n.ToString("X");
                    }

                    preview.AppendLine(string.Format("> {0}-{1}: {2}", pageId.ToString("X"), i.ToString("X").PadLeft(8, '0'), o));
                }
            }

            if (!string.IsNullOrEmpty(preview.ToString()))
            {
                Write(preview.ToString());
            }
            else
            {
                WriteLine("> Memória vazia");
            }

            WriteLine("Fim imprimir");
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

        #region Export To File

        public static void Create()
        {
            writer = new StreamWriter(string.Format("log_{0}.txt", DateTime.Now.Ticks), false);
        }

        public static void Write(string text)
        {
            if (writer == null)
                Create();

            writer.Write(text);
        }

        public static void WriteLine(string text)
        {
            if (writer == null)
                Create();

            writer.WriteLine(text);
        }

        public static void Close()
        {
            writer.Flush();
            writer.Close();
        }

        #endregion
    }
}