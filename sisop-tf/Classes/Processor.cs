using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sisop_tf
{
    public abstract class Processor
    {
        protected PagedMemory memory { get; set; }
        protected int totalTime = 0;

        protected Queue<Process> processing { get; set; }
        protected List<Process> waiting { get; set; }

        protected Random random;
        
        public Processor(PagedMemory memory)
        {
            this.memory = memory;

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
        public abstract void Execute();

        /// <summary>
        /// Organiza execução dos processos
        /// </summary>
        public abstract void OrganizeWaiting();

        /// <summary>
        /// Remove o processo informado da memória
        /// </summary>
        /// <param name="process">Processo a ser removido</param>
        protected void Deallocate(Process process)
        {
            memory.Deallocate(process.Pages);
        
            if (process.State == State.Exit)
            {
                Console.WriteLine("Término do processamento");
                Console.WriteLine("Processo {0} - Estatíticas: WT={1} TT={2}", process.Id, process.Wt, process.Tt);
            }

            Console.WriteLine("Desalocou o processo {0} da memória principal.", process.Id);
        }

        /// <summary>
        /// Lê o arquivo carregando os dados para a memória principal
        /// </summary>
        /// <param name="process">Processo que está sendo carregado</param>
        /// <param name="pageNumber">Posição inicial de escrita na memória</param>
        /// <returns></returns>
        protected Process Read(Process process, int pageNumber)
        {
            // Define o ponto inicial a ser carregado o programa
            var page = pageNumber;
            var index = 0;
            
            // Variáveis de controle de abertura e fechamento de bloco
            var openDataRead = false;
            var openCodeRead = false;

            // Carrega lista de operadores
            var operators = CreateOperators();

            // Dicionário com as posições das variáveis
            var dataIndex = new Dictionary<string, KeyValuePair<int, int>>();

            // Dicionário com as posições das labels
            var labels = new Dictionary<string, KeyValuePair<int, int>>();

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
            var beginData = new KeyValuePair<int, int>(page, index);

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
                    var pagePair = memory.SetValue(page, index, s[1]);
                    page = pagePair.Key;
                    index = pagePair.Value;
                    process.AddPage(page);
                    
                    dataIndex.Add(s[0], new KeyValuePair<int,int>(page, index));
                    index++;
                }
            }

            // Guarda a última posição de data / primeira posição de código
            var codeLinesCount = 0;
            var firstCodeLine = true;
            Nullable<KeyValuePair<int, int>> beginCode = null;

            foreach (var line in precode)
            {
                // Se possui .code abre leitura de código
                if (line.Contains(".code"))
                {
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
                    var posOperator = 0;
                    var posValue = 1;

                    var s = line.Trim().Split(' ');

                    // Se possui mais de 2 posições é tratado como LABEL
                    if (s.Length > 2)
                    {
                        // Desloca posições de leitura
                        posOperator = 1;
                        posValue = 2;

                        labels.Add(s[0].Replace(":", string.Empty), new KeyValuePair<int, int>(page, index));
                    }

                    // Busca código do operador
                    var op = (int)operators[s[posOperator]];

                    var pagePair = memory.SetValue(page, index, op.ToString());
                    page = pagePair.Key;
                    index = pagePair.Value;
                    process.AddPage(page);

                    index++;

                    if (dataIndex.ContainsKey(s[posValue]))
                    {
                        pagePair = memory.SetValue(page, index, dataIndex[s[posValue]]);
                        page = pagePair.Key;
                        index = pagePair.Value;
                        process.AddPage(page);
                    }
                    else if (labels.ContainsKey(s[posValue]))
                    {
                        pagePair = memory.SetValue(page, index, labels[s[posValue]]);
                        page = pagePair.Key;
                        index = pagePair.Value;
                        process.AddPage(page);
                    }
                    else
                    {
                        pagePair = memory.SetValue(page, index, s[posValue]);
                        page = pagePair.Key;
                        index = pagePair.Value;
                        process.AddPage(page);
                    }

                    if (firstCodeLine)
                    {
                        firstCodeLine = false;
                        beginCode = new KeyValuePair<int, int>(page, index);
                    }

                    index++;
                    codeLinesCount++;
                }
            }

            // Guarda a última posição de código
            var endCode = new KeyValuePair<int, int>(page, index - 1);
            process.SetParameters(beginData, beginCode.Value, endCode, codeLinesCount - 1);

            // Imprime o espaço alocado pelo processo na memória
            Program.MemoryPreview(process);
            Console.WriteLine();

            return process;
        }

        /// <summary>
        /// Calcula o espaço necessário para o processo, simulando o carregamento em uma memória auxiliar
        /// </summary>
        /// <param name="process">Processo a ser avialiado</param>
        /// <returns></returns>
        protected int PreRead(Process process)
        {
            var preMemory = memory.GetPreMemoryInstance();
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