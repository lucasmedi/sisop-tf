using System.Collections.Generic;
using System.Linq;
using sisop_tf.Classes;
using sisop_tf.Enums;

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
            OrganizeWaiting();

            while (!IsEmpty())
            {
                Program.WriteLine(string.Format("Tempo Total: {0}\n", totalTime));

                OrganizeWaiting();

                int control = 0;
                var isProcessing = true;
                var blocked = false;

                Process process = null;
                if (processing.Count > 0)
                {
                    process = GetNext();
                    process.State = State.Running;
                    Program.WriteLine(string.Format("Processando: {0}", process.Id));
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
                                    device = this.GetDevice(1);
                                    break;
                                case 2:
                                    device = this.GetDevice(2);
                                    break;
                                case 3:
                                    device = this.GetDevice(3);
                                    break;
                                case 4:
                                    device = this.GetDevice(4);
                                    break;
                            }

                            if (device != null)
                            {
                                // Adiciona processo no dispositivo
                                AddDeviceRequest(device, process.Id);

                                // Bloqueia processo
                                blocked = true;

                                // Log
                                logString = string.Format(logString, device.Name);
                            }

                            // finaliza o processamento
                            isProcessing = false;

                            break;
                        default:
                            //throw new Exception("Better Call Saul!");
                            break;
                    }

                    Program.WriteLine(logString);

                    control++;
                    totalTime++;
                }

                OrganizeWt(control);
                OrganizeDevices(control);

                if (process != null)
                {
                    process.Pt += control;

                    if (blocked)
                    {
                        process.State = State.Blocked;
                        AddToWaiting(process);

                        Program.WriteLine(string.Format("Bloqueia processo {0}", process.Id));
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

                    Program.WriteLine("");
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
                    Program.WriteLine(string.Format("Processo {0} ignorado por falta de espaço total na memória principal.", process.Id));
                    Program.WriteLine("");
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

                if (process.State == State.Blocked)
                {
                    if (this.slots.Where(x => x.HasDevice(process.Id)).Count() == 0)
                    {
                        // Adiciona para remoção da waiting
                        removed.Add(process);

                        // Altera estado para State.Ready
                        process.State = State.Ready;

                        // Adiciona na fila
                        AddToQueue(process);
                    }
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

        private void OrganizeDevices(int elapsed)
        {
            foreach (var device in slots)
            {
                device.ControlRequest(elapsed);
            }

            PrintRequestQueueForAll();
        }

        private void AddDeviceRequest(Device device, string pId)
        {
            if (device == null)
            {
                return;
            }

            // Adiciona requisicao na lista do dispositivo
            switch (device.Method)
            {
                case Method.READ:
                    device.AddRequest(pId);
                    break;
                case Method.WRITE:
                    device.AddRequest(pId);
                    break;
                case Method.ALL:
                    device.AddRequest(pId);
                    break;
                default:
                    break;
            }

            device.PrintRequestQueue(); // imprime fila do dispositivo
        }
    }
}