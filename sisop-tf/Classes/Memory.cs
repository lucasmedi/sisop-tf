using System.Collections.Generic;
using System.Linq;

namespace sisop_tf
{
    public class Memory
    {
        private string[] memory;
        private List<Page> pages;

        private int key;

        public int Size { get; private set; }

        public int PageSize { get; private set; }
        public int NumPages { get; private set; }

        public Memory()
        {
            memory = new string[1024];
            key = 0;
        }

        public Memory(int numPages, int tamPage)
        {
            NumPages = numPages;
            PageSize = tamPage;

            Size = numPages * tamPage;

            key = 0;
            memory = new string[Size];

            // Cria as páginas solicitadas ao usuário
            pages = new List<Page>();
            for (int i = 0; i < numPages; i++)
                pages.Add(new Page(i, tamPage, tamPage * i, PageState.Empty));
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

        /// <summary>
        /// Verifica se existe o espaço informado
        /// </summary>
        /// <param name="size">Tamanho a ser verificado</param>
        /// <param name="page">Primeira página disponível, se houver</param>
        /// <returns>Verdadeiro se existe espaço</returns>
        public bool HasSpace(int size, out int page)
        {
            // Calcula o número de páginas à verificar
            var tam = size / PageSize;

            // Busca número de páginas disponíveis
            var count = pages.Count(o => o.State == PageState.Empty);

            // Pega a primeira página livre
            page = pages.Where(o => o.State == PageState.Empty).FirstOrDefault().Id;

            return (tam <= count);
        }

        public Page GetNextEmptyPage()
        {
            // Pega a primeira página livre
            return pages.Where(o => o.State == PageState.Empty).FirstOrDefault();
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