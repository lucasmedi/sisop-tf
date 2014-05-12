
namespace sisop_tf.Structure
{
    public class Memory
    {
        private string[] memory;

        private int key;

        public int Size { get; private set; }

        public Memory()
        {
            memory = new string[1024];
            key = 0;
        }

        public Memory(int size)
        {
            Size = size;
            memory = new string[Size];

            key = 0;
        }

        /// <summary>
        /// Busca valor a partir de uma posição
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(int key)
        {
            return memory[key];
        }

        /// <summary>
        /// Seta valor na posição informada
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(int key, string value)
        {
            memory[key] = value;
        }

        /// <summary>
        /// Somente para testes.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasNext(int key)
        {
            return !string.IsNullOrEmpty(memory[key]);
        }

        /// <summary>
        /// Verifica se existe o espaço informado
        /// </summary>
        /// <param name="size">Tamanho a ser verificado</param>
        /// <param name="position">Valor da posição inicial do espaço, se houver</param>
        /// <returns>Verdadeiro se existe espaço</returns>
        //public bool HasSpace(int size, out int position)
        //{
        //    var startPosition = -1;
        //    var freeSize = 0;
        //    for (int i = 0; i < memory.Length; i++)
        //    {
        //        if (string.IsNullOrEmpty(memory[i]))
        //        {
        //            freeSize++;
        //            if (freeSize == 1)
        //            {
        //                startPosition = i;
        //            }
                    
        //            if (freeSize == size)
        //            {
        //                position = startPosition;
        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            freeSize = 0;
        //            startPosition = -1;
        //        }
        //    }

        //    position = -1;
        //    return false;
        //}

        //// Imprime estrutura de páginas
        //public void PrintPages()
        //{
        //    int cont = 0;
        //    foreach (var p in pages)
        //    {
        //        //Console.WriteLine();
        //        //Console.WriteLine("Bloco " + cont + ": " + p.PosInMemory + "até" + (p.PosInMemory + p.Size - 1));
        //        // Verificar questão de imprimir a página ocupada por processo.(Só tem empty e full)
        //    }
        //}
    }
}