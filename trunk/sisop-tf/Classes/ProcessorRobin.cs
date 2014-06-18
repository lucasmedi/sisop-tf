﻿using sisop_tf.Classes;
using sisop_tf.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace sisop_tf
{
    public class ProcessorRobin : Processor
    {
        private int quantum { get; set; }

        public ProcessorRobin(PagedMemory memory, int qtn = 5)
            : base(memory)
        {
            quantum = qtn;
        }

        /// <summary>
        /// Executa um processo da fila
        /// </summary>
        public override void Execute()
        {
            var waitTime = 0;
            OrganizeWaiting();

            while (!IsEmpty())
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
                    totalTime += control;
                    isProcessing = false;
                }

                KeyValuePair<int, int> pair;

                // Executa programa
                while (isProcessing)
                {
                    OrganizeWaiting();

                    isProcessing = process.HasNext() && control < quantum;
                    if (!isProcessing)
                        continue;

                    int value = -1;

                    // Buscar operador
                    var operador = memory.GetValue(process.Pg, process.Pc);
                    process.Next(memory.PageSize);

                    // Buscar operando
                    var operando = memory.GetValue(process.Pg, process.Pc);
                    process.Next(memory.PageSize);

                    var op = (Operators)int.Parse(operador);
                    var logString = Program.LogOperation(op);

                    // verifica se é acesso imediato
                    if (operando.Contains("#"))
                    {
                        // remove o indicador de acesso imediato
                        int.TryParse(operando.Replace("#", string.Empty), out value);
                    }
                    // verifica se é acesso direto
                    else if (op != Operators.SYSCALL && !int.TryParse(operando, out value))
                    {
                        if (process.Data.ContainsKey(operando))
                        {
                            // recupera valor do bloco de dados
                            pair = process.Data[operando];
                            value = int.Parse(memory.GetValue(pair.Key, pair.Value));
                        }
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
                            pair = process.Data[operando];
                            memory.SetValue(pair.Key, pair.Value, process.Ac.ToString());

                            // Log
                            logString = string.Format(logString, process.Ac, operando);
                            break;
                        case Operators.BRANY:
                            process.JumpTo(process.Labels[operando]);

                            // Log
                            logString = string.Format(logString, operando);
                            break;
                        case Operators.BRPOS:
                            if (process.Ac > 0)
                            {
                                process.JumpTo(process.Labels[operando]);
                            }

                            // Log
                            logString = string.Format(logString, process.Ac, operando);
                            break;
                        case Operators.BRZERO:
                            if (process.Ac == 0)
                            {
                                process.JumpTo(process.Labels[operando]);
                            }

                            // Log
                            logString = string.Format(logString, process.Ac, operando);
                            break;
                        case Operators.BRNEG:
                            if (process.Ac < 0)
                            {
                                process.JumpTo(process.Labels[operando]);
                            }

                            // Log
                            logString = string.Format(logString, process.Ac, operando);
                            break;
                        case Operators.SYSCALL:
                            Device device = null;
                            switch (value)
                            {
                                case 0:
                                    process.JumpTo(process.EndCode);

                                    // Log
                                    logString = string.Format(logString, "HALT - PROCESSO TERMINADO");
                                    break;
                                case 1:
                                    Console.WriteLine("Impressão do AC: {0}", process.Ac);
                                    //recupera o device para a operacao
                                    device = this.GetDevice(1);
                                    OrganizeSlotRequest(device, waitTime);

                                    // Bloqueia processo
                                    blocked = true;

                                    // Log
                                    logString = string.Format(logString, "OUTPUT: " + process.Ac);
                                    break;
                                case 2:
                                    bool aceito = false;
                                    device = this.GetDevice(2);
                                    OrganizeSlotRequest(device, waitTime);
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
                                case 3:
                                    device = this.GetDevice(3);
                                    OrganizeSlotRequest(device, waitTime);
                                    blocked = true;
                                    break;
                                case 4:
                                    device = this.GetDevice(4);
                                    OrganizeSlotRequest(device, waitTime);
                                    blocked = true;
                                    break;
                            }
                            PrintRequestQueueForAll();

                            // finaliza o processamento
                            isProcessing = false;

                            break;
                        default:
                            //throw new Exception("Better Call Saul!");
                            break;
                    }

                    Console.WriteLine(logString);

                    control++;
                    totalTime++;
                }

                OrganizeWt(control);

                if (process != null)
                {
                    process.Pt += control;

                    if (blocked)
                    {
                        process.State = State.Blocked;
                        //var waitTime = random.Next(10, 40); trocado pelo tempo do device - var no topo do metodo
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
        }

        /// <summary>
        /// Organiza execução dos processos
        /// </summary>
        public override void OrganizeWaiting()
        {
            var removed = new List<Process>();

            waiting = waiting.OrderBy(o => o.Priority).ToList<Process>();
            foreach (var process in waiting)
            {
                if (process.Size > memory.Size)
                {
                    removed.Add(process);
                    Console.WriteLine("Processo {0} ignorado por falta de espaço total na memória principal.", process.Id);
                    Console.WriteLine();
                    continue;
                }

                if ((process.State == State.New || process.State == State.ReadySuspend) && process.At <= totalTime)
                {
                    var page = -1;
                    if (memory.HasSpace(process.Size, out page))
                    {
                        // Adiciona para remoção da waiting
                        removed.Add(process);

                        // Carrega na memória
                        Read(process, page);

                        // Altera Processing Time para 0
                        process.Pt = 0;

                        if (process.State == State.New && process.At > 0)
                        {
                            // Alterar o WT para o tempo real
                            process.Wt -= (process.At);
                        }

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
                    //Em Blocked, espera tempo simulado de requisicao acabar e passa para ready
                    //sai da fila de requisicoes do device que solicitou a requisicao - atendido a requisicao
                    var pendingDevices = this.slots.Where(x => x.Requests.Any(y => y.Item1 > 0 || y.Item2 > 0));
                    foreach (var device in pendingDevices)
                    {
                        
                        Console.WriteLine("Atendendo ao dispositivo: {0}", device.Name.ToString());
                        if (device.Requests.First().Item1 > 0)
                        {
                            Console.WriteLine("Lendo do dispositivo: {0} por {1} segundos",
                                device.Name.ToString(), device.Requests.First().Item1);
                            //Thread.Sleep(new TimeSpan(0, 0, device.Requests.First().Item1));
                        }
                        if (device.Requests.First().Item2 > 0)
                        {
                            Console.WriteLine("Escrevendo no dispositivo: {0} por {1} segundos",
                                device.Name.ToString(), device.Requests.First().Item2);
                            //Thread.Sleep(new TimeSpan(0, 0, device.Requests.First().Item2));
                        }
                        device.RemoveRequest();
                    }

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

        private void OrganizeWt(int elapsed)
        {
            foreach (var p in processing)
            {
                p.Wt += elapsed;
            }

            foreach (var p in waiting)
            {
                p.Wt += elapsed;
            }
        }

        private void OrganizeSlotRequest(Device device, int waitTime)
        {
            //Adiciona requisicao na lista do dispositivo
            waitTime = new TimeSpan(0, 0, 10).Seconds;
            if (device != null)
            {
                if (device.Method.Equals(Method.READ))
                    device.AddRequest(waitTime, null);
                else if (device.Method.Equals(Method.WRITE))
                    device.AddRequest(null, waitTime);
                else if (device.Method.Equals(Method.ALL))
                    device.AddRequest(waitTime, waitTime);
            }
            //device.PrintRequestQueue(); imprime fila so do dispositivo - talvez precise na apresentacao
            
        }
    }
}