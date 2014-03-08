using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace sisop_tf
{
	class Program
	{
		static void Main(string[] args)
		{
			// Log: início da aplicação
            Console.WriteLine("**** ASMReader ver. 1.0 *****");
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
            var data = new Dictionary<string, string>();
            var code = new List<KeyValuePair<int, string>>();

            var filePath = @"..\..\files\code.asm";

            // Log: nome do arquivo a ser executado
            Console.WriteLine("> Inicia leitura do arquivo '{0}'", filePath);

            // Carrega código do arquivo .asm
            var file = new StreamReader(filePath);

            // Inicia o carregamento do arquivo
            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                // Linha vazia é descartada
                if (string.IsNullOrEmpty(line))
                    continue;

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

                // Se está lendo dados
                if (openDataRead)
                {
                    var s = line.Trim().Split(' ');

                    // Armazena no formato (<NOME>, <VALOR>);
                    data.Add(s[0], s[1]);
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

                        // Armazena operador LABEL no formato (LABEL, <NOME>)
                        code.Add(new KeyValuePair<int, string>(operators["LABEL"], s[0].Replace(":", string.Empty)));
                    }

                    // Busca código do operador
                    var op = operators[s[operador]];

                    // Armazena no formato (<OPERADOR>, <VALOR>)
                    code.Add(new KeyValuePair<int, string>(op, s[valor]));
                }
            }

            // Log: fim do carregamento do arquivo e início do processamento
            watch.Stop();
            Console.WriteLine("Fim passo 1 - Tempo Total: {0}ms", watch.Elapsed.TotalSeconds * 1000.0);
            Console.WriteLine();
            Console.WriteLine("Passo 2: executar processamento");
            watch.Reset();
            watch.Start();

            // Dicionário com as posições das labels
            var labels = new Dictionary<string, int>();

            // Valor inicial do contador
            var acc = -1;

            // Executa programa
            for (int i = 0; i < code.Count; i++)
            {
                var item = code[i];
                int value = -1;

                // Tenta transformar em um valor inteiro
                if (!int.TryParse(item.Value, out value))
                {
                    // Verifica se é acesso imediato
                    if (item.Value.Contains("#"))
                    {
                        // Remove o indicador de acesso imediato
                        int.TryParse(item.Value.Replace("#", string.Empty), out value);
                    }
                    // Verifica se é acesso direto
                    else if (data.ContainsKey(item.Value))
                    {
                        // Recupera valor do bloco de dados
                        value = int.Parse(data[item.Value]);
                    }
                }

                var logString = LogOperation(item);

                // Executa a operação
                switch ((Operators)item.Key)
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
                        logString = string.Format(logString, acc, item.Value);
                        break;
                    case Operators.STORE:
                        data[item.Value] = acc.ToString();

                        // Log
                        logString = string.Format(logString, acc, item.Value);
                        break;
                    case Operators.BRANY:
                        i = labels[item.Value];

                        // Log
                        logString = string.Format(logString, item.Value);
                        break;
                    case Operators.BRPOS:
                        if (acc > 0)
                        {
                            i = labels[item.Value];
                        }

                        // Log
                        logString = string.Format(logString, acc, item.Value);
                        break;
                    case Operators.BRZERO:
                        if (acc == 0)
                        {
                            i = labels[item.Value];
                        }

                        // Log
                        logString = string.Format(logString, acc, item.Value);
                        break;
                    case Operators.BRNEG:
                        if (acc < 0)
                        {
                            i = labels[item.Value];
                        }

                        // Log
                        logString = string.Format(logString, acc, item.Value);
                        break;
                    case Operators.SYSCALL:
                        // TODO: Rever funcionalidade
                        if (value == 0)
                        {
                            i = code.Count;

                            // Log
                            logString = string.Format(logString, "HALT");
                        }
                        else
                        {
                            // Log
                            logString = string.Format(logString, "?");
                        }
                        break;
                    case Operators.LABEL:
                        labels.Add(item.Value, i);

                        // Log
                        logString = string.Format(logString, item.Value);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Console.WriteLine(logString);
            }

            Console.WriteLine("Fim passo 2 - Tempo Total: {0}ms", watch.Elapsed.TotalSeconds * 1000.0);
            Console.WriteLine();
            Console.WriteLine("Listar resultado das variáveis");
            foreach (var item in data)
            {
                Console.WriteLine("> '{0}': {1}", item.Key, item.Value);
            }
            Console.WriteLine("Fim listar");
            Console.WriteLine();
            Console.WriteLine("**** Fim ASMReader *****");
            Console.ReadKey();
        }

        private static string LogOperation(KeyValuePair<int, string> item)
        {
            var log = String.Empty;
            switch ((Operators)item.Key)
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
            operators.Add("ADD", 0000);
            operators.Add("SUB", 0001);
            operators.Add("MULT", 0010);
            operators.Add("DIV", 0011);

            operators.Add("LOAD", 0100);
            operators.Add("STORE", 0101);

            operators.Add("BRANY", 1000);
            operators.Add("BRPOS", 1001);
            operators.Add("BRZERO", 1010);
            operators.Add("BRNEG", 1011);

            operators.Add("SYSCALL", 1100);
            operators.Add("LABEL", 1101);

            return operators;
        }

        public enum Operators
        {
            ADD = 0000,
            SUB = 0001,
            MULT = 0010,
            DIV = 0011,

            LOAD = 0100,
            STORE = 0101,

            BRANY = 1000,
            BRPOS = 1001,
            BRZERO = 1010,
            BRNEG = 1011,

            SYSCALL = 1100,
            LABEL = 1101
        }
    }
}