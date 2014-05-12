using System.Collections.Generic;
using System.Linq;
using sisop_tf.Structure;

namespace sisop_tf
{
    public class PagedMemory
    {
        private Memory memory;
        private List<Page> pages;

        public int PageSize { get; private set; }
        public int PageCount { get; private set; }

        public int Size { get { return memory.Size; } }

        public PagedMemory(int pageSize, int pageCount)
        {
            this.PageSize = pageSize;
            this.PageCount = pageCount;

            this.memory = new Memory(pageSize * pageCount);

            // Cria as páginas solicitadas ao usuário
            pages = new List<Page>();
            for (int i = 0; i < PageCount; i++)
                pages.Add(new Page(i, PageSize, PageSize * i, PageState.Free));
        }

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
            var count = pages.Count(o => o.State == PageState.Free);

            // Pega a primeira página livre
            page = pages.Where(o => o.State == PageState.Free).FirstOrDefault().Id;

            return (tam <= count);
        }

        public KeyValuePair<int, int> SetValue(int pageId, int index, string value)
        {
            var page = pages.Where(o => o.Id == pageId).First();

            if (index > page.Size -1)
            {
                page = pages.Where(o => o.State == PageState.Free).FirstOrDefault();
                index = 0;
            }

            page.State = PageState.Occupied;
            var key = page.FirstPosition + index;
            memory.SetValue(key, value);

            return new KeyValuePair<int, int>(page.Id, index);
        }

        public KeyValuePair<int, int> SetValue(int pageId, int index, KeyValuePair<int, int> data)
        {
            var value = GetValue(data.Key, data.Value);

            return SetValue(pageId, index, value);
        }

        public string GetValue(int pageId, int index)
        {
            var dataPage = pages.Where(o => o.Id == pageId).First();
            var dataIndex = dataPage.FirstPosition + index;

            return memory.GetValue(dataIndex);
        }

        public Page GetNextEmptyPage()
        {
            // Pega a primeira página livre
            return pages.Where(o => o.State == PageState.Free).FirstOrDefault();
        }

        public Memory GetPreMemoryInstance()
        {
            return new Memory();
        }

        public void Deallocate(List<int> list)
        {
            foreach (var item in list)
            {
                var page = pages.Where(o => o.Id == item).First();
                for (int i = page.FirstPosition; i < page.Size; i++)
                {
                    memory.SetValue(i, null);
                }

                page.State = PageState.Free;
            }
        }

        public IList<int> GetPageIds()
        {
            return pages.Select(o => o.Id).ToList();
        }
    }
}